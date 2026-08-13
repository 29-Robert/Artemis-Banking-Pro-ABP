using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Services
{
    public class SavingsAccountService : ISavingsAccountService
    {
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILoanRepository _loanRepository;

        public SavingsAccountService(
            ISavingsAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            ILoanRepository loanRepository)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _loanRepository = loanRepository;
        }

        public async Task<SavingsAccountDetailDto> CreateSecondaryAccountAsync(CreateSecondaryAccountDto dto)
        {
            if (dto.InitialBalance < 0)
                throw new Exception("El balance inicial no puede ser negativo.");

            var user = await _userRepository.GetByCedulaAsync(dto.ClientCedula);

            if (user == null)
                throw new Exception("No existe un cliente registrado con esta cédula.");

            if (!user.IsActive)
                throw new Exception("No se puede asignar una cuenta a un cliente inactivo.");

            int userId = user.Id;

            var principalAccount = await _accountRepository.GetPrincipalByClientAsync(userId);

            if (principalAccount == null || principalAccount.Status != AccountStatus.Activa)
                throw new Exception("El cliente debe tener una cuenta de ahorro principal activa antes de asignarle una cuenta secundaria.");

            string newAccountNumber;
            bool isUnique = false;
            var random = new Random();

            do
            {
                newAccountNumber = random.Next(100000000, 999999999).ToString();
                var existingAccount = await _accountRepository.GetByAccountNumberAsync(newAccountNumber);
                var existingLoan = await _loanRepository.GetByLoanNumberAsync(newAccountNumber);
                if (existingAccount == null && existingLoan == null)
                {
                    isUnique = true;
                }
            } while (!isUnique);

            var newAccount = new SavingsAccount
            {
                UserId = userId,
                AccountNumber = newAccountNumber,
                Balance = dto.InitialBalance,
                Type = AccountType.Secundaria,
                IsPrincipal = false,
                Status = AccountStatus.Activa,
                CreatedAt = DateTime.UtcNow
            };

            await _accountRepository.AddAsync(newAccount);

            if (dto.InitialBalance > 0)
            {
                var initialTransaction = new Transaction
                {
                    AccountNumber = newAccount.AccountNumber,
                    Type = TransactionType.Credito,
                    Amount = dto.InitialBalance,
                    Status = TransactionStatus.Aprobada,
                    Description = "Asignación de balance inicial en cuenta secundaria",
                    CreatedAt = DateTime.UtcNow
                };

                await _transactionRepository.AddAsync(initialTransaction);
            }

            return new SavingsAccountDetailDto
            {
                AccountNumber = newAccount.AccountNumber,
                Balance = newAccount.Balance,
                Status = newAccount.Status
            };
        }

        public async Task CancelSecondaryAccountAsync(string accountNumber)
        {
            var accountToCancel = await _accountRepository.GetByAccountNumberAsync(accountNumber);

            if (accountToCancel == null)
            {
                throw new Exception("La cuenta seleccionada no existe.");
            }

            if (accountToCancel.Status == AccountStatus.Cancelada)
            {
                throw new Exception("La cuenta seleccionada ya se encuentra cancelada.");
            }

            if (accountToCancel.IsPrincipal || accountToCancel.Type == AccountType.Principal)
            {
                throw new Exception("Las cuentas principales no pueden ser canceladas.");
            }

            if (accountToCancel.IsBlocked)
            {
                throw new Exception("La cuenta seleccionada está bloqueada.");
            }

            var principalAccount = await _accountRepository.GetPrincipalByClientAsync(accountToCancel.UserId);

            if (principalAccount == null || principalAccount.Status != AccountStatus.Activa)
            {
                throw new Exception("No es posible cancelar la cuenta porque el cliente no tiene una cuenta principal activa para recibir los fondos.");
            }

            if (principalAccount.IsBlocked)
            {
                throw new Exception("La cuenta principal del cliente se encuentra bloqueada.");
            }

            if (accountToCancel.Balance > 0 )
            {
                decimal transferAmount = accountToCancel.Balance;
                
                var debitTransaction = new Transaction
                {
                    AccountNumber = accountToCancel.AccountNumber,
                    Type = TransactionType.Debito,
                    Amount = transferAmount,
                    Status = TransactionStatus.Aprobada,
                    Description = $"Transferencia automática por cancelación hacia cuenta principal {principalAccount.AccountNumber}",
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(debitTransaction);

                var creditTransaction = new Transaction
                {
                    AccountNumber = principalAccount.AccountNumber,
                    Type = TransactionType.Credito,
                    Amount = transferAmount,
                    Status = TransactionStatus.Aprobada,
                    Description = $"Transferencia recibida por cancelación de cuenta secundaria {accountToCancel.AccountNumber}",
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(creditTransaction);

                principalAccount.Balance += transferAmount;
                accountToCancel.Balance = 0;

                await _accountRepository.UpdateAsync(principalAccount);
            }

            accountToCancel.Status = AccountStatus.Cancelada;

            await _accountRepository.UpdateAsync(accountToCancel);
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(string accountNumber, int page, int pageSize)
        {
            var transactions = await _transactionRepository.GetPagedByAccountAsync(accountNumber, page, pageSize);

            var history = transactions.Select(t => new TransactionDto
            {
                Date = t.CreatedAt,
                Amount = t.Amount,

                Type = t.Type,
                Status = t.Status,

                Beneficiary = t.Type == TransactionType.Debito ? t.RelatedEntity : accountNumber,
                Origin = t.Type == TransactionType.Credito ? t.RelatedEntity : accountNumber,

                Description = t.Description
            }).ToList();

            return history;
        }

        public async Task<ClientHomeDto> GetClientHomeDataAsync(string clientId)
        {
            int idCliente = int.Parse(clientId);
            var user = await _userRepository.GetByIdAsync(idCliente);
            if (user == null) throw new Exception("Cliente no encontrado.");

            var allAccounts = await _accountRepository.GetAllAsync();
            var clientAccounts = allAccounts
                .Where(a => a.UserId == idCliente && a.Status == AccountStatus.Activa)
                .Select(a => new SavingsAccountListItemDto
                {
                    AccountNumber = a.AccountNumber,
                    ClientFullName = $"{user.FirstName} {user.LastName}",
                    Balance = a.Balance,
                    Type = a.Type,
                    Status = a.Status
                }).ToList();

            var loans = await _loanRepository.GetLoansByClientAsync(user.Cedula);
            var activeLoans = loans.Where(l => l.Status == "Activo" || l.Status == "Aprobado").ToList();
            var loanDtos = activeLoans.Select(l => new LoanResponseDto
            {
                Id = l.Id,
                LoanNumber = l.LoanNumber,
                ClientId = l.ClientId,
                ClientFullName = $"{user.FirstName} {user.LastName}",
                CapitalAmount = l.CapitalAmount,
                TermInMonths = l.TermInMonths,
                AnnualInterestRate = l.AnnualInterestRate,
                Status = l.Status,
                TotalAmountToPay = l.Installments != null ? l.Installments.Sum(i => i.PendingInstallmentAmount) : 0,
                ClientPaymentStatus = l.Installments != null && l.Installments.Any(i => i.PaymentStatus == "Pendiente" && i.DueDate < DateTime.UtcNow) ? "Atrasado" : "Al día"
            }).ToList();

            var cards = await _creditCardRepository.GetCardsByClientAsync(user.Cedula);
            var activeCards = cards.Where(c => c.Status == "Activa").Select(c => new CreditCardResponseDto
            {
                Id = c.Id,
                MaskedCardNumber = c.CardNumber,
                LastFourDigits = c.CardNumber.Length >= 4 ? c.CardNumber.Substring(c.CardNumber.Length - 4) : c.CardNumber,
                CreditLimit = c.CreditLimit,
                CurrentDebt = c.CurrentDebt,
                AvailableCredit = c.CreditLimit - c.CurrentDebt,
                ExpirationDate = $"{c.ExpirationMonth}/{c.ExpirationYear}",
                Status = c.Status
            }).ToList();

            return new ClientHomeDto
            {
                Accounts = clientAccounts,
                Loans = loanDtos,
                CreditCards = activeCards
            };
        }

        public async Task ProcessCreditCardPaymentOwnAccountAsync(string sourceAccountNumber, string cardNumber, decimal amount, string clientId)
        {
            if (amount <= 0) throw new Exception("El monto a pagar debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(sourceAccountNumber);
            if (account == null || account.UserId != int.Parse(clientId))
                throw new Exception("La cuenta de ahorro de origen no existe o no le pertenece.");

            if (account.Status == AccountStatus.Cancelada)
                throw new Exception("La cuenta seleccionada está cancelada.");

            if (account.IsBlocked)
                throw new Exception("La cuenta seleccionada está bloqueada.");

            if (account.Balance - account.BlockedAmount < amount)
                throw new Exception($"Fondos insuficientes. Balance disponible: RD$ {(account.Balance - account.BlockedAmount):N2} (debido a retenciones de RD$ {account.BlockedAmount:N2}).");

            var card = await _creditCardRepository.GetByCardNumberAsync(cardNumber);
            if (card == null)
                throw new Exception("La tarjeta de crédito no existe.");

            if (card.Status == "Cancelada")
                throw new Exception("La tarjeta de crédito se encuentra cancelada.");

            if (amount > card.CurrentDebt)
                throw new Exception("No se permite sobrepagar la tarjeta. El monto máximo a pagar es de RD$ " + card.CurrentDebt.ToString("N2"));

            card.CurrentDebt -= amount;
            account.Balance -= amount;

            await _creditCardRepository.UpdateAsync(card);
            await _accountRepository.UpdateAsync(account);

            var transaction = new Transaction
            {
                AccountNumber = sourceAccountNumber,
                Type = TransactionType.Debito,
                Amount = amount,
                Status = TransactionStatus.Aprobada,
                Description = $"Pago de tarjeta de crédito propia {cardNumber}",
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();

            // Notificación por correo
            try
            {
                string body = $"Hola,\nSe ha realizado el pago de tu tarjeta de crédito desde tu cuenta propia.\n" +
                              $"Cuenta de ahorro: {sourceAccountNumber}\n" +
                              $"Tarjeta de crédito: {cardNumber}\n" +
                              $"Monto pagado: RD$ {amount:N2}\n" +
                              $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n";
                await _emailService.SendNotificationEmailAsync("correo@ejemplo.com", "Pago de Tarjeta de Crédito Realizado", body);
            }
            catch { }
        }

        public async Task ProcessLoanPaymentOwnAccountAsync(string sourceAccountNumber, string loanNumber, decimal amount, string clientId)
        {
            if (amount <= 0) throw new Exception("El monto a pagar debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(sourceAccountNumber);
            if (account == null || account.UserId != int.Parse(clientId))
                throw new Exception("La cuenta de ahorro de origen no existe o no le pertenece.");

            if (account.Status == AccountStatus.Cancelada)
                throw new Exception("La cuenta seleccionada está cancelada.");

            if (account.IsBlocked)
                throw new Exception("La cuenta seleccionada está bloqueada.");

            if (account.Balance - account.BlockedAmount < amount)
                throw new Exception($"Fondos insuficientes. Balance disponible: RD$ {(account.Balance - account.BlockedAmount):N2} (debido a retenciones de RD$ {account.BlockedAmount:N2}).");

            var loan = await _loanRepository.GetByLoanNumberAsync(loanNumber);
            if (loan == null)
                throw new Exception("El préstamo no existe.");

            if (loan.Status == "Cancelado" || loan.Status == "Pagado")
                throw new Exception("El préstamo ya se encuentra cancelado o pagado.");

            decimal remainingPayment = amount;
            var pendingInstallments = loan.Installments
                .Where(i => i.PaymentStatus == "Pendiente")
                .OrderBy(i => i.InstallmentNumber)
                .ToList();

            foreach (var installment in pendingInstallments)
            {
                if (remainingPayment <= 0) break;

                if (remainingPayment >= installment.PendingInstallmentAmount)
                {
                    remainingPayment -= installment.PendingInstallmentAmount;
                    installment.PendingInstallmentAmount = 0;
                    installment.PaymentStatus = "Pagada";
                }
                else
                {
                    installment.PendingInstallmentAmount -= remainingPayment;
                    remainingPayment = 0;
                }
            }

            if (!loan.Installments.Any(i => i.PaymentStatus == "Pendiente"))
            {
                loan.Status = "Pagado";
            }

            await _loanRepository.UpdateAsync(loan);

            account.Balance -= amount;
            await _accountRepository.UpdateAsync(account);

            var transaction = new Transaction
            {
                AccountNumber = sourceAccountNumber,
                Type = TransactionType.Debito,
                Amount = amount,
                Status = TransactionStatus.Aprobada,
                Description = $"Pago de préstamo propio {loanNumber}",
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();

            // Notificación por correo
            try
            {
                string body = $"Hola,\nSe ha realizado el pago de tu préstamo desde tu cuenta propia.\n" +
                              $"Cuenta de ahorro: {sourceAccountNumber}\n" +
                              $"Préstamo: {loanNumber}\n" +
                              $"Monto pagado: RD$ {amount:N2}\n" +
                              $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n";
                await _emailService.SendNotificationEmailAsync("correo@ejemplo.com", "Pago de Préstamo Realizado", body);
            }
            catch { }
        }

        public async Task ProcessCashAdvanceAsync(string sourceAccountNumber, string cardNumber, decimal amount, string clientId)
        {
            if (amount <= 0) throw new Exception("El monto del avance debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(sourceAccountNumber);
            if (account == null || account.UserId != int.Parse(clientId))
                throw new Exception("La cuenta de ahorro de destino no existe o no le pertenece.");

            if (account.Status == AccountStatus.Cancelada)
                throw new Exception("La cuenta seleccionada está cancelada.");

            if (account.IsBlocked)
                throw new Exception("La cuenta seleccionada está bloqueada.");

            var card = await _creditCardRepository.GetByCardNumberAsync(cardNumber);
            if (card == null)
                throw new Exception("La tarjeta de crédito no existe.");

            if (card.Status == "Cancelada")
                throw new Exception("La tarjeta de crédito se encuentra cancelada.");

            decimal interest = amount * 0.0625m;
            decimal totalCharge = amount + interest;

            decimal availableCredit = card.CreditLimit - card.CurrentDebt;
            if (availableCredit < totalCharge)
                throw new Exception($"Crédito disponible insuficiente. Requiere RD$ {totalCharge:N2} (Avance: RD$ {amount:N2} + Interés 6.25%: RD$ {interest:N2}), pero solo dispone de RD$ {availableCredit:N2}");

            card.CurrentDebt += totalCharge;
            account.Balance += amount;

            await _creditCardRepository.UpdateAsync(card);
            await _accountRepository.UpdateAsync(account);

            var transaction = new Transaction
            {
                AccountNumber = sourceAccountNumber,
                Type = TransactionType.Credito,
                Amount = amount,
                Status = TransactionStatus.Aprobada,
                Description = $"Avance de efectivo desde tarjeta {cardNumber}",
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();

            // Notificación por correo
            try
            {
                string body = $"Hola,\nSe ha procesado un avance de efectivo en tu tarjeta de crédito.\n" +
                              $"Tarjeta afectada: {cardNumber}\n" +
                              $"Monto depositado en cuenta de ahorros {sourceAccountNumber}: RD$ {amount:N2}\n" +
                              $"Comisión de avance cobrada (6.25%): RD$ {interest:N2}\n" +
                              $"Total cargado a la tarjeta: RD$ {totalCharge:N2}\n" +
                              $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n";
                await _emailService.SendNotificationEmailAsync("correo@ejemplo.com", "Avance de Efectivo Procesado", body);
            }
            catch { }
        }

        public async Task BlockAccountAsync(string accountNumber)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null) throw new Exception("La cuenta no existe.");
            account.IsBlocked = true;
            await _accountRepository.UpdateAsync(account);
            await _accountRepository.SaveChangesAsync();
        }

        public async Task UnblockAccountAsync(string accountNumber)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null) throw new Exception("La cuenta no existe.");
            account.IsBlocked = false;
            await _accountRepository.UpdateAsync(account);
            await _accountRepository.SaveChangesAsync();
        }

        public async Task SetBlockedAmountAsync(string accountNumber, decimal amount)
        {
            if (amount < 0) throw new Exception("El monto a retener no puede ser negativo.");
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null) throw new Exception("La cuenta no existe.");
            account.BlockedAmount = amount;
            await _accountRepository.UpdateAsync(account);
            await _accountRepository.SaveChangesAsync();
        }
    }
}