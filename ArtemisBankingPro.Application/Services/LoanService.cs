using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Application.Exceptions;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using System.Security.Cryptography;

namespace ArtemisBankingPro.Application.Services
{
    public class LoanService : ILoanService
    {
        private static readonly int[] AllowedTerms = { 6, 12, 18, 24, 30, 36, 42, 48, 54, 60 };

        private readonly ILoanRepository _loanRepository;
        private readonly ILoanInstallmentRepository _installmentRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public LoanService(
            ILoanRepository loanRepository,
            ILoanInstallmentRepository installmentRepository,
            ICreditCardRepository creditCardRepository,
            ISavingsAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IGenericRepository<User> userRepository,
            IEmailService emailService,
            IMapper mapper)
        {
            _loanRepository = loanRepository;
            _installmentRepository = installmentRepository;
            _creditCardRepository = creditCardRepository;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _mapper = mapper;
        }

        
        // LISTADO PAGINADO
        public async Task<PagedResult<LoanResponseDto>> GetLoansAsync(string? cedula, string? status, int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _loanRepository.SearchAsync(cedula, status, pageNumber, pageSize);

            if (!string.IsNullOrWhiteSpace(cedula) && totalCount == 0)
            {
                var clientExists = (await _userRepository.GetAllAsync())
                    .Any(u => u.Cedula == cedula && u.Role?.Name == "Cliente");

                if (!clientExists)
                    throw new KeyNotFoundException("No existe un cliente registrado con esta cédula.");

                throw new InvalidOperationException("Este cliente no tiene préstamos registrados.");
            }

            return new PagedResult<LoanResponseDto>
            {
                Items = _mapper.Map<List<LoanResponseDto>>(items),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // DETALLE
        
        public async Task<LoanResponseDto> GetLoanByIdAsync(int id)
        {
            var loan = await _loanRepository.GetByIdWithDetailsAsync(id);
            if (loan == null) throw new KeyNotFoundException("El préstamo seleccionado no existe.");

            return _mapper.Map<LoanResponseDto>(loan);
        }

        // CLIENTES ELEGIBLES 
        
        public async Task<EligibleClientsResponseDto> GetEligibleClientsAsync(string? cedula, int pageNumber, int pageSize)
        {
            var allUsers = await _userRepository.GetAllAsync();

            var candidates = allUsers
                .Where(u => u.IsActive && u.Role?.Name == "Cliente")
                .Where(u => string.IsNullOrWhiteSpace(cedula) || u.Cedula.Contains(cedula))
                .ToList();

            var eligible = new List<EligibleClientDto>();
            foreach (var user in candidates)
            {
                if (await _loanRepository.HasActiveLoanAsync(user.Id)) continue;

                var loanDebt = await _loanRepository.GetTotalActiveDebtByClientAsync(user.Id);
                var cardDebt = await _creditCardRepository.GetTotalActiveDebtByClientAsync(user.Id);

                eligible.Add(new EligibleClientDto
                {
                    Id = user.Id.ToString(),
                    Cedula = user.Cedula,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    TotalDebt = loanDebt + cardDebt
                });
            }

            var totalCount = eligible.Count;
            var paged = eligible.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new EligibleClientsResponseDto
            {
                SystemAverageDebt = await CalculateSystemAverageDebtAsync(),
                Clients = new PagedResult<EligibleClientDto>
                {
                    Items = paged,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                }
            };
        }

        
        // ASIGNAR PRÉSTAMO
        
        public async Task<LoanResponseDto> AssignLoanAsync(CreateLoanRequestDto request, int adminId)
        {
            if (string.IsNullOrWhiteSpace(request.ClientId))
                throw new ArgumentException("Debe seleccionar un cliente para continuar.");

            var clientId = int.Parse(request.ClientId);

            var client = await _userRepository.GetByIdAsync(clientId);
            if (client == null) throw new KeyNotFoundException("El cliente seleccionado no existe.");
            if (!client.IsActive) throw new InvalidOperationException("Solo se puede asignar préstamos a clientes activos.");
            if (await _loanRepository.HasActiveLoanAsync(clientId))
                throw new InvalidOperationException("Este cliente ya tiene un préstamo activo asignado.");
            if (!AllowedTerms.Contains(request.TermInMonths))
                throw new ArgumentException("El plazo seleccionado no es válido.");
            if (request.CapitalAmount <= 0)
                throw new ArgumentException("El monto a prestar debe ser mayor que cero.");
            if (request.AnnualInterestRate < 0)
                throw new ArgumentException("La tasa de interés anual no puede ser negativa.");

            var principalAccount = await _accountRepository.GetPrincipalByClientAsync(clientId);
            if (principalAccount == null || principalAccount.Status != AccountStatus.Activa)
                throw new InvalidOperationException(
                    "El cliente no tiene una cuenta de ahorro principal activa para recibir el desembolso del préstamo.");

            // Tabla de amortización 
            var installments = GenerateAmortizationTable(request.CapitalAmount, request.AnnualInterestRate, request.TermInMonths);
            var totalToPay = installments.Sum(i => i.InstallmentAmount);

            // Evaluación de riesgo 
            var currentDebt = await _loanRepository.GetTotalActiveDebtByClientAsync(clientId)
                             + await _creditCardRepository.GetTotalActiveDebtByClientAsync(clientId);
            var projectedDebt = currentDebt + totalToPay;
            var averageDebt = await CalculateSystemAverageDebtAsync();

            if (!request.ConfirmHighRisk)
            {
                if (currentDebt > averageDebt)
                    throw new HighRiskClientException(
                        "Este cliente se considera de alto riesgo, ya que su deuda actual supera el promedio del sistema.",
                        "CurrentHighRisk", currentDebt, projectedDebt, averageDebt);

                if (projectedDebt > averageDebt)
                    throw new HighRiskClientException(
                        "Asignar este préstamo convertirá al cliente en un cliente de alto riesgo, ya que su deuda superará el umbral promedio del sistema.",
                        "ProjectedHighRisk", currentDebt, projectedDebt, averageDebt);
            }

            // Número de préstamo único 
            var loanNumber = await GenerateUniqueLoanNumberAsync();

            var loan = new Loan
            {
                ClientId = clientId,
                LoanNumber = loanNumber,
                CapitalAmount = request.CapitalAmount,
                TermInMonths = request.TermInMonths,
                AnnualInterestRate = request.AnnualInterestRate,
                Status = "Activo",
                AdminId = (adminId),
                CreatedAt = DateTime.UtcNow
            };

            var createdLoan = await _loanRepository.AddAsync(loan);
            await _loanRepository.SaveChangesAsync(); 

            foreach (var installment in installments)
                installment.LoanId = createdLoan.Id;

            await _installmentRepository.AddRangeAsync(installments);
            await _installmentRepository.SaveChangesAsync();

            // Desembolso a la cuenta principal
            principalAccount.Balance += request.CapitalAmount;
            await _accountRepository.UpdateAsync(principalAccount);

            await _transactionRepository.AddAsync(new Transaction
            {
                AccountNumber = principalAccount.AccountNumber,
                Type = TransactionType.Credito,
                Amount = request.CapitalAmount,
                Status = TransactionStatus.Aprobada,
                Description = $"Desembolso de préstamo {loanNumber}",
                PerformedByUserId = adminId,
                CreatedAt = DateTime.UtcNow
            });
            await _accountRepository.SaveChangesAsync();


            var loanWithDetails = await _loanRepository.GetByIdWithDetailsAsync(createdLoan.Id);
            var response = _mapper.Map<LoanResponseDto>(loanWithDetails);

            try
            {
                var monthlyPayment = installments.First().InstallmentAmount;
                await _emailService.SendNotificationEmailAsync(
                    client.Email,
                    "Préstamo aprobado",
                    $"Su préstamo ha sido aprobado correctamente.\n" +
                    $"Número de préstamo: {loanNumber}\n" +
                    $"Monto aprobado: RD${request.CapitalAmount:N2}\n" +
                    $"Plazo: {request.TermInMonths} meses\n" +
                    $"Tasa de interés anual: {request.AnnualInterestRate}%\n" +
                    $"Cuota mensual: RD${monthlyPayment:N2}\n" +
                    "El monto aprobado ha sido depositado en su cuenta de ahorro principal.");
            }
            catch
            {
                response.EmailNotificationFailed = true;

            }
               return response;

        }

        // MODIFICAR TASA

        public async Task<LoanResponseDto> UpdateInterestRateAsync(int loanId, decimal newAnnualRate)
        {
            var loan = await _loanRepository.GetByIdWithDetailsAsync(loanId);
            if (loan == null) throw new KeyNotFoundException("El préstamo seleccionado no existe.");
            if (loan.Status != "Activo")
                throw new InvalidOperationException("Solo se puede modificar la tasa de interés de préstamos activos.");
            if (newAnnualRate < 0)
                throw new ArgumentException("La tasa de interés anual no puede ser negativa.");

            var now = DateTime.UtcNow;
            var futurePending = loan.Installments
                .Where(i => i.PaymentStatus == "Pendiente" && !i.IsLate && i.DueDate > now)
                .OrderBy(i => i.InstallmentNumber)
                .ToList();

            if (futurePending.Count == 0)
                throw new InvalidOperationException("No existen cuotas futuras pendientes para recalcular.");

            loan.AnnualInterestRate = newAnnualRate;

            var remainingCapital = futurePending.Sum(i => i.CapitalAmount);
            var recalculated = GenerateAmortizationTable(remainingCapital, newAnnualRate, futurePending.Count);

            for (var k = 0; k < futurePending.Count; k++)
            {
                var installment = futurePending[k];
                var recalc = recalculated[k];

                installment.InstallmentAmount = recalc.InstallmentAmount;
                installment.InterestAmount = recalc.InterestAmount;
                installment.CapitalAmount = recalc.CapitalAmount;
                installment.PendingInstallmentAmount = recalc.InstallmentAmount;

                await _installmentRepository.UpdateAsync(installment);
            }

            await _loanRepository.UpdateAsync(loan);
            await _loanRepository.SaveChangesAsync();
            await _installmentRepository.SaveChangesAsync();

            var response = _mapper.Map<LoanResponseDto>(loan);

            try
            {
                var nextInstallment = futurePending.First();
                await _emailService.SendNotificationEmailAsync(
                    loan.Client.Email,
                    "Actualización de tasa de interés de préstamo",
                    $"La tasa de interés de su préstamo {loan.LoanNumber} ha sido actualizada.\n" +
                    $"Nueva tasa de interés anual: {newAnnualRate}%\n" +
                    $"Nuevo valor de la próxima cuota: RD${nextInstallment.InstallmentAmount:N2}\n" +
                    $"Fecha de vencimiento de la próxima cuota: {nextInstallment.DueDate:dd/MM/yyyy}\n" +
                    "Esta modificación aplica únicamente a las cuotas futuras pendientes.");
            }
            catch
            {
                response.EmailNotificationFailed = true;
            }

            return response;
        }



        // Helpers privados

        private static List<LoanInstallment> GenerateAmortizationTable(decimal capital, decimal annualRate, int months)
        {
            var monthlyRate = annualRate / 100m / 12m;

            decimal monthlyInstallment;
            if (monthlyRate == 0)
            {
                monthlyInstallment = capital / months;
            }
            else
            {
                var power = (decimal)Math.Pow((double)(1 + monthlyRate), months);
                monthlyInstallment = capital * (monthlyRate * power) / (power - 1);
            }
            monthlyInstallment = Math.Round(monthlyInstallment, 2);

            var schedule = new List<LoanInstallment>();
            var pendingCapital = capital;
            var dueDate = DateTime.UtcNow;

            for (var i = 1; i <= months; i++)
            {
                var interestAmount = Math.Round(pendingCapital * monthlyRate, 2);
                var capitalAmount = monthlyInstallment - interestAmount;
                var installmentAmount = monthlyInstallment;

                if (i == months) // última cuota: absorbe el redondeo
                {
                    capitalAmount = pendingCapital;
                    installmentAmount = capitalAmount + interestAmount;
                }

                pendingCapital -= capitalAmount;
                dueDate = dueDate.AddMonths(1);

                schedule.Add(new LoanInstallment
                {
                    InstallmentNumber = i,
                    DueDate = dueDate,
                    InstallmentAmount = Math.Round(installmentAmount, 2),
                    InterestAmount = interestAmount,
                    CapitalAmount = Math.Round(capitalAmount, 2),
                    PendingInstallmentAmount = Math.Round(installmentAmount, 2),
                    PaymentStatus = "Pendiente",
                    IsLate = false
                });
            }

            return schedule;
        }

        private async Task<decimal> CalculateSystemAverageDebtAsync()
        {
            var allUsers = await _userRepository.GetAllAsync();
            var activeClients = allUsers.Count(u => u.IsActive && u.Role?.Name == "Cliente");
            if (activeClients == 0) return 0m;

            var totalLoanDebt = await _loanRepository.GetTotalActiveDebtSystemWideAsync();
            var totalCardDebt = await _creditCardRepository.GetTotalActiveDebtSystemWideAsync();

            return (totalLoanDebt + totalCardDebt) / activeClients;
        }

        private async Task<string> GenerateUniqueLoanNumberAsync()
        {
            for (var attempt = 0; attempt < 20; attempt++)
            {
                var number = RandomNumberGenerator.GetInt32(100_000_000, 999_999_999).ToString();

                var existsAsLoan = await _loanRepository.LoanNumberExistsAsync(number);
                var existsAsAccount = await _accountRepository.GetByAccountNumberAsync(number) != null;

                if (!existsAsLoan && !existsAsAccount) return number;
            }
            throw new InvalidOperationException("No fue posible generar un número de préstamo único.");
        }

    }
}