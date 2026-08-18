using ArtemisBankingPro.Application.DTOs.Cashier;
using ArtemisBankingPro.Application.Extensions;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Services
{
    public class CashierService : ICashierService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanInstallmentRepository _loanInstallmentRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CashierService> _logger;

        public CashierService(
            ILoanRepository loanRepository,
            ILoanInstallmentRepository loanInstallmentRepository,
            ICreditCardRepository creditCardRepository,
            ISavingsAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            ILogger<CashierService> logger)
        {
            _loanRepository = loanRepository;
            _loanInstallmentRepository = loanInstallmentRepository;
            _creditCardRepository = creditCardRepository;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<TransactionResponseDto> ProcessDepositAsync(DepositRequestDto request, int cashierId)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("El monto a depositar debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(request.DestinationAccountNumber);

            if (account == null || account.Status != AccountStatus.Activa)
            {
                _logger.LogWarning("Depósito fallido: cuenta {AccountNo} no existe o no está activa. Cajero: {CashierId}", request.DestinationAccountNumber, cashierId);
                return await RegisterRejectedAsync(request.DestinationAccountNumber, request.Amount,
                    "El número de cuenta ingresado no corresponde a una cuenta válida.", cashierId);
            }

            var user = await _userRepository.GetByIdAsync(account.UserId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Depósito fallido: el usuario de la cuenta {AccountNo} no se encuentra activo. Cajero: {CashierId}", account.AccountNumber, cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El usuario de la cuenta no se encuentra activo.", cashierId);
            }

            var transaction = new Transaction
            {
                AccountNumber = account.AccountNumber,
                Type = TransactionType.Credito,
                Amount = request.Amount,
                Status = TransactionStatus.Aprobada,
                Description = "DEPÓSITO",
                PerformedByUserId = cashierId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                account.Balance += request.Amount;
                await _accountRepository.UpdateAsync(account);
                await _transactionRepository.AddAsync(transaction);
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Depósito APROBADO: Cuenta: {AccountNo}, Monto: RD$ {Amount:N2}, Cajero: {CashierId}", account.AccountNumber, request.Amount, cashierId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error al procesar depósito de cajero para la cuenta {AccountNo}.", account.AccountNumber);
                throw;
            }

            bool emailSent = true;

            var emailDest = user.Email ?? account.User?.Email;
            if (!string.IsNullOrEmpty(emailDest))
            {
                try
                {
                    string ultimosCuatro = account.AccountNumber[^4..];
                    string subject = $"Depósito realizado a su cuenta {ultimosCuatro}";
                    var fechaHoraLocal = transaction.CreatedAt.ToLocalTime();
                    string body = $@"
                        <p>Hola {user.FirstName},</p>
                        <p>Se ha realizado un depósito a su cuenta terminada en {ultimosCuatro}.</p>
                        <ul>
                            <li><strong>Monto depositado:</strong> {request.Amount:C}</li>
                            <li><strong>Fecha y hora:</strong> {fechaHoraLocal:dd/MM/yyyy hh:mm:ss tt}</li>
                        </ul>
                        <p>Si usted no reconoce esta operación, comuníquese con la entidad bancaria.</p>";

                    await _emailService.SendNotificationEmailAsync(emailDest, subject, body);
                }
                catch
                {
                    emailSent = false;
                }
            }

            var response = BuildApproved(transaction, request.Amount);

            if (!emailSent)
            {
                response.WarningMessage =
                    "El depósito fue realizado correctamente, pero no fue posible enviar el correo de notificación.";
            }

            return response;
        }

        public async Task<TransactionResponseDto> ProcessWithdrawalAsync(WithdrawRequestDto request, int cashierId)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("El monto a retirar debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(request.SourceAccountNumber);

            if (account == null || account.Status != AccountStatus.Activa)
            {
                string accountNum = account?.AccountNumber ?? request.SourceAccountNumber;
                _logger.LogWarning("Retiro fallido: cuenta {AccountNo} no existe o no está activa. Cajero: {CashierId}", accountNum, cashierId);
                return await RegisterRejectedAsync(accountNum, request.Amount,
                    "El número de cuenta ingresado no corresponde a una cuenta válida.", cashierId);
            }

            if (account.IsBlocked)
            {
                _logger.LogWarning("Retiro fallido: cuenta {AccountNo} se encuentra bloqueada. Cajero: {CashierId}", account.AccountNumber, cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "La cuenta seleccionada está bloqueada.", cashierId);
            }

            var user = await _userRepository.GetByIdAsync(account.UserId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Retiro fallido: el usuario de la cuenta {AccountNo} no se encuentra activo. Cajero: {CashierId}", account.AccountNumber, cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El usuario de la cuenta no se encuentra activo.", cashierId);
            }

            if (account.Balance - account.BlockedAmount < request.Amount)
            {
                _logger.LogWarning("Retiro fallido: fondos insuficientes (saldo congelado o insuficiente) en cuenta {AccountNo}. Cajero: {CashierId}", account.AccountNumber, cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El monto ingresado excede el saldo disponible de la cuenta.", cashierId);
            }

            var transaction = new Transaction
            {
                AccountNumber = account.AccountNumber,
                Type = TransactionType.Debito,
                Amount = request.Amount,
                Status = TransactionStatus.Aprobada,
                Description = "RETIRO",
                PerformedByUserId = cashierId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                account.Balance -= request.Amount;
                await _accountRepository.UpdateAsync(account);
                await _transactionRepository.AddAsync(transaction);
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Retiro APROBADO: Cuenta: {AccountNo}, Monto: RD$ {Amount:N2}, Cajero: {CashierId}", account.AccountNumber, request.Amount, cashierId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error al procesar retiro de cajero para la cuenta {AccountNo}.", account.AccountNumber);
                throw;
            }

            var response = BuildApproved(transaction, request.Amount);

            if (account.User != null && !string.IsNullOrEmpty(account.User.Email))
            {
                try
                {
                    string ultimosCuatro = account.AccountNumber[^4..];
                    string subject = $"Retiro realizado desde su cuenta {ultimosCuatro}";
                    var fechaHoraLocal = transaction.CreatedAt.ToLocalTime();
                    string body = $@"
                        <p>Hola {user.FirstName},</p>
                        <p>Se ha realizado un retiro desde su cuenta terminada en {ultimosCuatro}.</p>
                        <ul>
                            <li><strong>Monto retirado:</strong> {request.Amount:C}</li>
                            <li><strong>Fecha y hora:</strong> {fechaHoraLocal:dd/MM/yyyy hh:mm:ss tt}</li>
                        </ul>
                        <p>Si usted no reconoce esta operación, comuníquese con la entidad bancaria.</p>";

                    await _emailService.SendNotificationEmailAsync(emailDest, subject, body);
                }
                catch 
                {
                    response.WarningMessage =
            "El retiro fue realizado correctamente, pero no fue posible enviar el correo de notificación.";
                }
            }

            return response;
        }

        public async Task<TransactionResponseDto> ProcessCreditCardPaymentAsync(PayCreditCardRequestDto request, int cashierId)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("El monto a pagar debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(request.SourceAccountNumber);
            if (account == null || account.Status != AccountStatus.Activa)
            {
                string accNum = account?.AccountNumber ?? request.SourceAccountNumber;
                _logger.LogWarning("Pago tarjeta fallido: cuenta {AccountNo} no existe o no está activa. Cajero: {CashierId}", accNum, cashierId);
                return await RegisterRejectedAsync(accNum, request.Amount,
                    "El número de cuenta ingresado no corresponde a una cuenta válida.", cashierId);
            }

            if (account.IsBlocked)
            {
                _logger.LogWarning("Pago tarjeta fallido: cuenta {AccountNo} se encuentra bloqueada. Cajero: {CashierId}", account.AccountNumber, cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "La cuenta seleccionada está bloqueada.", cashierId);
            }

            var user = await _userRepository.GetByIdAsync(account.UserId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Pago tarjeta fallido: el usuario de la cuenta {AccountNo} no se encuentra activo. Cajero: {CashierId}", account.AccountNumber, cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El usuario de la cuenta no se encuentra activo.", cashierId);
            }

            var card = await _creditCardRepository.GetByCardNumberAsync(request.CardNumber);
            if (card == null || card.Status != "Activa")
            {
                _logger.LogWarning("Pago tarjeta fallido: tarjeta {CardNo} no existe o no está activa. Cajero: {CashierId}", request.CardNumber.MaskCardNumber(), cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El número de tarjeta ingresado no corresponde a una tarjeta válida.", cashierId);
            }

            if (card.CurrentDebt <= 0)
            {
                _logger.LogWarning("Pago tarjeta fallido: tarjeta {CardNo} no tiene deuda pendiente. Cajero: {CashierId}", card.CardNumber.MaskCardNumber(), cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "La tarjeta seleccionada no tiene deuda pendiente.", cashierId);
            }

            var appliedAmount = Math.Min(request.Amount, card.CurrentDebt);

            if (account.Balance - account.BlockedAmount < appliedAmount)
            {
                _logger.LogWarning("Pago tarjeta fallido: fondos insuficientes en cuenta {AccountNo}. Cajero: {CashierId}", account.AccountNumber, cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El monto ingresado excede el saldo disponible de la cuenta.", cashierId);
            }

            var transaction = new Transaction
            {
                AccountNumber = account.AccountNumber,
                Type = TransactionType.Debito,
                Amount = appliedAmount,
                Status = TransactionStatus.Aprobada,
                Description = $"PAGO A TARJETA {card.CardNumber[^4..]}",
                PerformedByUserId = cashierId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                account.Balance -= appliedAmount;
                card.CurrentDebt -= appliedAmount;

                await _accountRepository.UpdateAsync(account);
                await _creditCardRepository.UpdateAsync(card);
                await _transactionRepository.AddAsync(transaction);
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Pago de tarjeta APROBADO: Cuenta origen: {AccountNo}, Tarjeta: {CardNo}, Monto: RD$ {Amount:N2}, Cajero: {CashierId}", account.AccountNumber, card.CardNumber.MaskCardNumber(), appliedAmount, cashierId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error al procesar pago de tarjeta de crédito desde cajero.");
                throw;
            }
            var response = BuildApproved(transaction, appliedAmount);
            try
            {
                var fechaHoraLocal = transaction.CreatedAt.ToLocalTime();
                string cardLast4 = card.CardNumber[^4..];
                string accountLast4 = account.AccountNumber[^4..];

                if (card.Client != null && !string.IsNullOrEmpty(card.Client.Email))
                {
                    string subjectCard = $"Pago realizado a la tarjeta {cardLast4}";
                    string bodyCard = $@"
                        <p>Hola {card.Client.FirstName},</p>
                        <p>Se ha realizado un pago a su tarjeta de crédito terminada en {cardLast4}.</p>
                        <ul>
                            <li><strong>Monto pagado:</strong> {appliedAmount:C}</li>
                            <li><strong>Cuenta origen terminada en:</strong> {accountLast4}</li>
                            <li><strong>Fecha y hora:</strong> {fechaHoraLocal:dd/MM/yyyy hh:mm:ss tt}</li>
                        </ul>
                        <p>Si usted no reconoce esta operación, comuníquese con la entidad bancaria.</p>";

                    await _emailService.SendNotificationEmailAsync(card.Client.Email, subjectCard, bodyCard);
                }

                if (account.UserId != card.ClientId && account.User != null && !string.IsNullOrEmpty(account.User.Email))
                {
                    string subjectAccount = $"Débito por pago a tarjeta {cardLast4}";
                    string bodyAccount = $@"
                        <p>Hola {account.User.FirstName},</p>
                        <p>Se ha debitado dinero de su cuenta terminada en {accountLast4} para el pago de una tarjeta de crédito.</p>
                        <ul>
                            <li><strong>Monto debitado:</strong> {appliedAmount:C}</li>
                            <li><strong>Tarjeta destino:</strong> terminada en {cardLast4}</li>
                            <li><strong>Fecha y hora:</strong> {fechaHoraLocal:dd/MM/yyyy hh:mm:ss tt}</li>
                        </ul>
                        <p>Si usted no reconoce esta operación, comuníquese con la entidad bancaria inmediatamente.</p>";

                    await _emailService.SendNotificationEmailAsync(account.User.Email, subjectAccount, bodyAccount);
                }
            }
            catch 
            {
                response.WarningMessage =
          "El pago a la tarjeta fue realizado correctamente, pero no fue posible enviar el correo de notificación.";
            }

            return response;
        }

            
        

        public async Task<TransactionResponseDto> ProcessLoanPaymentAsync(PayLoanRequestDto request, int cashierId)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("El monto a pagar debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(request.SourceAccountNumber);
            if (account == null || account.Status != AccountStatus.Activa)
            {
                string accNum = account?.AccountNumber ?? request.SourceAccountNumber;
                _logger.LogWarning("Pago préstamo fallido: cuenta {AccountNo} no existe o no está activa. Cajero: {CashierId}", accNum, cashierId);
                return await RegisterRejectedAsync(accNum, request.Amount,
                    "El número de cuenta ingresado no corresponde a una cuenta válida.", cashierId);
            }

            if (account.IsBlocked)
            {
                _logger.LogWarning("Pago préstamo fallido: cuenta {AccountNo} se encuentra bloqueada. Cajero: {CashierId}", account.AccountNumber, cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "La cuenta seleccionada está bloqueada.", cashierId);
            }

            var user = await _userRepository.GetByIdAsync(account.UserId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Pago préstamo fallido: el usuario de la cuenta {AccountNo} no se encuentra activo. Cajero: {CashierId}", account.AccountNumber, cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El usuario de la cuenta no se encuentra activo.", cashierId);
            }

            var loan = await _loanRepository.GetByLoanNumberWithInstallmentsAsync(request.LoanNumber);
            if (loan == null || loan.Status == "Completado")
            {
                _logger.LogWarning("Pago préstamo fallido: préstamo {LoanNo} no existe o está completado. Cajero: {CashierId}", request.LoanNumber, cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El número de préstamo ingresado no corresponde a un préstamo válido.", cashierId);
            }

            var remainingDebt = loan.Installments.Sum(i => i.PendingInstallmentAmount);
            if (remainingDebt <= 0)
            {
                _logger.LogWarning("Pago préstamo fallido: préstamo {LoanNo} no tiene cuotas pendientes. Cajero: {CashierId}", loan.LoanNumber, cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El préstamo seleccionado no tiene cuotas pendientes de pago.", cashierId);
            }

            var appliedAmount = Math.Min(request.Amount, remainingDebt);

            if (account.Balance - account.BlockedAmount < appliedAmount)
            {
                _logger.LogWarning("Pago préstamo fallido: fondos insuficientes en cuenta {AccountNo}. Cajero: {CashierId}", account.AccountNumber, cashierId);
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El monto ingresado excede el saldo disponible de la cuenta.", cashierId);
            }

            var remainingPayment = appliedAmount;
            var pendingInstallments = loan.Installments
                .Where(i => i.PaymentStatus != "Pagada")
                .OrderBy(i => i.InstallmentNumber)
                .ToList();

            var transaction = new Transaction
            {
                AccountNumber = account.AccountNumber,
                Type = TransactionType.Debito,
                Amount = appliedAmount,
                Status = TransactionStatus.Aprobada,
                Description = $"PAGO A PRÉSTAMO {request.LoanNumber}",
                PerformedByUserId = cashierId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();
           

            try
            {
                foreach (var installment in pendingInstallments)
                {
                    if (remainingPayment <= 0) break;

                    if (remainingPayment >= installment.PendingInstallmentAmount)
                    {
                        remainingPayment -= installment.PendingInstallmentAmount;
                        installment.PendingInstallmentAmount = 0;
                        installment.PaymentStatus = "Pagada";
                        installment.IsLate = false;
                    }
                    else
                    {
                        installment.PendingInstallmentAmount -= remainingPayment;
                        installment.PaymentStatus = "Parcial";
                        remainingPayment = 0;
                    }

                    await _loanInstallmentRepository.UpdateAsync(installment);
                }

                if (loan.Installments.All(i => i.PendingInstallmentAmount == 0))
                {
                    loan.Status = "Completado";
                    await _loanRepository.UpdateAsync(loan);
                }

                account.Balance -= appliedAmount;
                await _accountRepository.UpdateAsync(account);
                await _transactionRepository.AddAsync(transaction);
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Pago de préstamo APROBADO: Cuenta origen: {AccountNo}, Préstamo: {LoanNo}, Monto: RD$ {Amount:N2}, Cajero: {CashierId}", account.AccountNumber, loan.LoanNumber, appliedAmount, cashierId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error al procesar pago de préstamo desde cajero.");
                throw;
            }

        
            var response = BuildApproved(transaction, appliedAmount);
            try
            {
                var fechaHoraLocal = transaction.CreatedAt.ToLocalTime();
                string accountLast4 = account.AccountNumber[^4..];

                if (loan.Client != null && !string.IsNullOrEmpty(loan.Client.Email))
                {
                    string subjectLoan = $"Pago realizado al préstamo {request.LoanNumber}";
                    string bodyLoan = $@"
                        <p>Hola {loan.Client.FirstName},</p>
                        <p>Se ha realizado un pago a su préstamo {request.LoanNumber}.</p>
                        <ul>
                            <li><strong>Monto pagado:</strong> {appliedAmount:C}</li>
                            <li><strong>Cuenta origen terminada en:</strong> {accountLast4}</li>
                            <li><strong>Fecha y hora:</strong> {fechaHoraLocal:dd/MM/yyyy hh:mm:ss tt}</li>
                        </ul>
                        <p>Si usted no reconoce esta operación, comuníquese con la entidad bancaria.</p>";

                    await _emailService.SendNotificationEmailAsync(loan.Client.Email, subjectLoan, bodyLoan);
                }

                if (account.UserId != loan.ClientId && account.User != null && !string.IsNullOrEmpty(account.User.Email))
                {
                    string subjectAccount = $"Débito por pago a préstamo {request.LoanNumber}";
                    string bodyAccount = $@"
                        <p>Hola {account.User.FirstName},</p>
                        <p>Se ha debitado dinero de su cuenta terminada en {accountLast4} para el pago de un préstamo.</p>
                        <ul>
                            <li><strong>Monto debitado:</strong> {appliedAmount:C}</li>
                            <li><strong>Préstamo destino:</strong> {request.LoanNumber}</li>
                            <li><strong>Fecha y hora:</strong> {fechaHoraLocal:dd/MM/yyyy hh:mm:ss tt}</li>
                        </ul>
                        <p>Si usted no reconoce esta operación, comuníquese con la entidad bancaria inmediatamente.</p>";

                    await _emailService.SendNotificationEmailAsync(account.User.Email, subjectAccount, bodyAccount);
                }
            }
            catch 
            {
              response.WarningMessage ="El pago al préstamo fue realizado correctamente, pero no fue posible enviar el correo de notificación.";
            }

            return response;
        }

        public async Task<TransactionResponseDto> ProcessThirdPartyTransferAsync(ThirdPartyTransferRequestDto request, int cashierId)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("El monto de la transacción debe ser mayor que cero.");

            if (request.SourceAccountNumber == request.DestinationAccountNumber)
            {
                return await RegisterRejectedAsync(request.SourceAccountNumber, request.Amount,
                    "La cuenta origen y la cuenta destino no pueden ser la misma.", cashierId);
            }

            var srcAcc = await _accountRepository.GetByAccountNumberAsync(request.SourceAccountNumber);
            if (srcAcc == null || srcAcc.Status != AccountStatus.Activa)
            {
                string accNum = srcAcc?.AccountNumber ?? request.SourceAccountNumber;
                _logger.LogWarning("Transferencia fallida: cuenta origen {AccountNo} no existe o no está activa. Cajero: {CashierId}", accNum, cashierId);
                return await RegisterRejectedAsync(accNum, request.Amount,
                    "El número de cuenta origen ingresado no corresponde a una cuenta válida.", cashierId);
            }

            if (srcAcc.IsBlocked)
            {
                _logger.LogWarning("Transferencia fallida: cuenta origen {AccountNo} está bloqueada. Cajero: {CashierId}", srcAcc.AccountNumber, cashierId);
                return await RegisterRejectedAsync(srcAcc.AccountNumber, request.Amount,
                    "La cuenta de origen seleccionada está bloqueada.", cashierId);
            }

            var srcUser = await _userRepository.GetByIdAsync(srcAcc.UserId);
            if (srcUser == null || !srcUser.IsActive)
            {
                _logger.LogWarning("Transferencia fallida: el usuario de la cuenta origen {AccountNo} no se encuentra activo. Cajero: {CashierId}", srcAcc.AccountNumber, cashierId);
                return await RegisterRejectedAsync(srcAcc.AccountNumber, request.Amount,
                    "El usuario de la cuenta de origen no se encuentra activo.", cashierId);
            }

            var tgtAcc = await _accountRepository.GetByAccountNumberAsync(request.DestinationAccountNumber);
            if (tgtAcc == null || tgtAcc.Status != AccountStatus.Activa)
            {
                _logger.LogWarning("Transferencia fallida: cuenta destino {AccountNo} no existe o no está activa. Cajero: {CashierId}", request.DestinationAccountNumber, cashierId);
                return await RegisterRejectedAsync(srcAcc.AccountNumber, request.Amount,
                    "El número de cuenta destino ingresado no corresponde a una cuenta válida.", cashierId);
            }

            if (tgtAcc.IsBlocked)
            {
                _logger.LogWarning("Transferencia fallida: cuenta destino {AccountNo} está bloqueada. Cajero: {CashierId}", tgtAcc.AccountNumber, cashierId);
                return await RegisterRejectedAsync(srcAcc.AccountNumber, request.Amount,
                    "La cuenta de destino seleccionada está bloqueada.", cashierId);
            }

            var tgtUser = await _userRepository.GetByIdAsync(tgtAcc.UserId);
            if (tgtUser == null || !tgtUser.IsActive)
            {
                _logger.LogWarning("Transferencia fallida: el usuario de la cuenta destino {AccountNo} no se encuentra activo. Cajero: {CashierId}", tgtAcc.AccountNumber, cashierId);
                return await RegisterRejectedAsync(srcAcc.AccountNumber, request.Amount,
                    "El usuario de la cuenta de destino no se encuentra activo.", cashierId);
            }

            if (srcAcc.Balance - srcAcc.BlockedAmount < request.Amount)
            {
                _logger.LogWarning("Transferencia fallida: fondos insuficientes en cuenta origen {AccountNo}. Cajero: {CashierId}", srcAcc.AccountNumber, cashierId);
                return await RegisterRejectedAsync(srcAcc.AccountNumber, request.Amount,
                    "El monto ingresado excede el saldo disponible de la cuenta.", cashierId);
            }

            var debitTx = new Transaction
            {
                AccountNumber = srcAcc.AccountNumber,
                Type = TransactionType.Debito,
                Amount = request.Amount,
                Status = TransactionStatus.Aprobada,
                Description = $"TRANSFERENCIA A TERCEROS {tgtAcc.AccountNumber}",
                PerformedByUserId = cashierId,
                CreatedAt = DateTime.UtcNow
            };

            var creditTx = new Transaction
            {
                AccountNumber = tgtAcc.AccountNumber,
                Type = TransactionType.Credito,
                Amount = request.Amount,
                Status = TransactionStatus.Aprobada,
                Description = $"TRANSFERENCIA A TERCEROS {srcAcc.AccountNumber}",
                PerformedByUserId = cashierId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                srcAcc.Balance -= request.Amount;
                tgtAcc.Balance += request.Amount;

                await _accountRepository.UpdateAsync(srcAcc);
                await _accountRepository.UpdateAsync(tgtAcc);

                await _transactionRepository.AddAsync(debitTx);
                await _transactionRepository.AddAsync(creditTx);

                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Transferencia a terceros APROBADA: Origen: {SrcAccount}, Destino: {TgtAccount}, Monto: RD$ {Amount:N2}, Cajero: {CashierId}", srcAcc.AccountNumber, tgtAcc.AccountNumber, request.Amount, cashierId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error al procesar transferencia a terceros desde cajero.");
                throw;
            }

            var response = BuildApproved(debitTx, request.Amount);

            try
            {
                var fechaHoraLocal = debitTx.CreatedAt.ToLocalTime();
                string srcLast4 = srcAcc.AccountNumber[^4..];
                string tgtLast4 = tgtAcc.AccountNumber[^4..];

                var srcEmail = srcUser.Email ?? srcAcc.User?.Email;
                if (!string.IsNullOrEmpty(srcEmail))
                {
                    string subjectSrc = $"Transacción realizada a la cuenta {tgtLast4}";
                    string bodySrc = $@"
                        <p>Hola {srcUser.FirstName},</p>
                        <p>Se ha debitado dinero de su cuenta para realizar una transferencia.</p>
                        <ul>
                            <li><strong>Monto transferido:</strong> {request.Amount:C}</li>
                            <li><strong>Cuenta origen:</strong> terminada en {srcLast4}</li>
                            <li><strong>Cuenta destino:</strong> terminada en {tgtLast4}</li>
                            <li><strong>Fecha y hora:</strong> {fechaHoraLocal:dd/MM/yyyy hh:mm:ss tt}</li>
                        </ul>
                        <p>Si usted no reconoce esta operación, comuníquese con la entidad bancaria inmediatamente.</p>";

                    await _emailService.SendNotificationEmailAsync(srcEmail, subjectSrc, bodySrc);
                }

                var tgtEmail = tgtUser.Email ?? tgtAcc.User?.Email;
                if (!string.IsNullOrEmpty(tgtEmail))
                {
                    string subjectTgt = $"Transacción enviada desde la cuenta {srcLast4}";
                    string bodyTgt = $@"
                        <p>Hola {tgtUser.FirstName},</p>
                        <p>Se ha acreditado una transferencia en su cuenta.</p>
                        <ul>
                            <li><strong>Monto recibido:</strong> {request.Amount:C}</li>
                            <li><strong>Cuenta origen:</strong> terminada en {srcLast4}</li>
                            <li><strong>Cuenta destino:</strong> terminada en {tgtLast4}</li>
                            <li><strong>Fecha y hora:</strong> {fechaHoraLocal:dd/MM/yyyy hh:mm:ss tt}</li>
                        </ul>
                        <p>Si usted no reconoce esta operación, comuníquese con la entidad bancaria.</p>";

                    await _emailService.SendNotificationEmailAsync(tgtEmail, subjectTgt, bodyTgt);
                }
            }
            

            catch 
            {
                response.WarningMessage = "La transferencia fue realizada correctamente, pero no fue posible enviar el correo de notificación.";
            }

            return response;
        }

        public async Task<CajeroHomeIndicatorsResponseDto> GetHomeIndicatorsAsync(int cashierId)
        {
            var today = DateTime.UtcNow.Date;
            var todaysTransactions = await _transactionRepository.GetByPerformedUserAndDateAsync(cashierId, today);
            var approvedTransactions = todaysTransactions
                .Where(t => t.Status == TransactionStatus.Aprobada)
                .ToList();

            int deposits = approvedTransactions.Count(t =>
                t.Type == TransactionType.Credito &&
                t.Description.StartsWith("DEPÓSITO", StringComparison.OrdinalIgnoreCase));

            int withdrawals = approvedTransactions.Count(t =>
                t.Type == TransactionType.Debito &&
                t.Description.StartsWith("RETIRO", StringComparison.OrdinalIgnoreCase));

            int payments = approvedTransactions.Count(t =>
                t.Type == TransactionType.Debito &&
                (t.Description.StartsWith("PAGO A TARJETA", StringComparison.OrdinalIgnoreCase) ||
                 t.Description.StartsWith("PAGO A PRÉSTAMO", StringComparison.OrdinalIgnoreCase)));

            int totalTransfers = approvedTransactions.Count(t =>
                t.Type == TransactionType.Debito &&
                t.Description.StartsWith("TRANSFERENCIA A TERCEROS", StringComparison.OrdinalIgnoreCase));

            int totalTransactions = deposits + withdrawals + payments + totalTransfers;

            return new CajeroHomeIndicatorsResponseDto
            {
                TotalTransactionsToday = totalTransactions,
                PaymentsToday = payments,
                DepositsToday = deposits,
                WithdrawalsToday = withdrawals
            };
        }

        public async Task<AccountPreviewResponseDto> GetAccountPreviewAsync(string accountNumber)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                throw new KeyNotFoundException("La cuenta no existe.");

            return new AccountPreviewResponseDto
            {
                AccountNumber = account.AccountNumber,
                AccountHolderFullName = $"{account.User.FirstName} {account.User.LastName}",
                Status = account.Status == AccountStatus.Activa ? "Activa" : "Cancelada"
            };
        }

        public async Task<CreditCardPreviewResponseDto> GetCreditCardPreviewAsync(string cardNumber)
        {
            var card = await _creditCardRepository.GetByCardNumberWithClientAsync(cardNumber);
            if (card == null)
                throw new KeyNotFoundException("La tarjeta no existe.");

            return new CreditCardPreviewResponseDto
            {
                MaskedCardNumber = "**** **** **** " + card.CardNumber[^4..],
                ClientFullName = $"{card.Client.FirstName} {card.Client.LastName}",
                CurrentDebt = card.CurrentDebt,
                Status = card.Status
            };
        }

        public async Task<LoanPreviewResponseDto> GetLoanPreviewAsync(string loanNumber)
        {
            var loan = await _loanRepository.GetByLoanNumberWithInstallmentsAsync(loanNumber);
            if (loan == null)
                throw new KeyNotFoundException("El préstamo no existe.");

            return new LoanPreviewResponseDto
            {
                LoanNumber = loan.LoanNumber,
                ClientFullName = $"{loan.Client.FirstName} {loan.Client.LastName}",
                RemainingBalance = loan.Installments.Sum(i => i.PendingInstallmentAmount),
                Status = loan.Status
            };
        }

        private async Task<TransactionResponseDto> RegisterRejectedAsync(string accountNumber, decimal amount, string reason, int cashierId)
        {
            var transaction = new Transaction
            {
                AccountNumber = accountNumber,
                Type = TransactionType.Debito,
                Amount = amount,
                Status = TransactionStatus.Rechazada,
                Description = reason,
                PerformedByUserId = cashierId,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.SaveChangesAsync();

            return new TransactionResponseDto
            {
                TransactionId = transaction.Id,
                Approved = false,
                AppliedAmount = 0,
                RejectionReason = reason,
                DateTime = transaction.CreatedAt
            };
        }

        private static TransactionResponseDto BuildApproved(Transaction transaction, decimal appliedAmount) => new()
        {
            TransactionId = transaction.Id,
            Approved = true,
            AppliedAmount = appliedAmount,
            RejectionReason = null,
            DateTime = transaction.CreatedAt
        };
    }
}