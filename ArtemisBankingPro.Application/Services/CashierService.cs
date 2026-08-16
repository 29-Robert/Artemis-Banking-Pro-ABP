using ArtemisBankingPro.Application.DTOs.Cashier;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
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

        public CashierService(
            ILoanRepository loanRepository,
            ILoanInstallmentRepository loanInstallmentRepository,
            ICreditCardRepository creditCardRepository,
            ISavingsAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork)
        {
            _loanRepository = loanRepository;
            _loanInstallmentRepository = loanInstallmentRepository;
            _creditCardRepository = creditCardRepository;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task<TransactionResponseDto> ProcessDepositAsync(DepositRequestDto request, int cashierId)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("El monto a depositar debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(request.DestinationAccountNumber);

            if (account == null || account.Status != AccountStatus.Activa)
            {
                return await RegisterRejectedAsync(request.DestinationAccountNumber, request.Amount,
                    "El número de cuenta ingresado no corresponde a una cuenta válida.", cashierId);
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
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            bool emailSent = true;

            if (account.User != null && !string.IsNullOrEmpty(account.User.Email))
            {
                try
                {
                    string ultimosCuatro = account.AccountNumber[^4..];
                    string subject = $"Depósito realizado a su cuenta {ultimosCuatro}";
                    var fechaHoraLocal = transaction.CreatedAt.ToLocalTime();
                    string body = $@"
                        <p>Hola {account.User.FirstName},</p>
                        <p>Se ha realizado un depósito a su cuenta terminada en {ultimosCuatro}.</p>
                        <ul>
                            <li><strong>Monto depositado:</strong> {request.Amount:C}</li>
                            <li><strong>Fecha y hora:</strong> {fechaHoraLocal:dd/MM/yyyy hh:mm:ss tt}</li>
                        </ul>
                        <p>Si usted no reconoce esta operación, comuníquese con la entidad bancaria.</p>";

                    await _emailService.SendNotificationEmailAsync(account.User.Email, subject, body);
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
                return await RegisterRejectedAsync(accountNum, request.Amount,
                    "El número de cuenta ingresado no corresponde a una cuenta válida.", cashierId);
            }

            if (account.Balance < request.Amount)
            {
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
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            if (account.User != null && !string.IsNullOrEmpty(account.User.Email))
            {
                try
                {
                    string ultimosCuatro = account.AccountNumber[^4..];
                    string subject = $"Retiro realizado desde su cuenta {ultimosCuatro}";
                    var fechaHoraLocal = transaction.CreatedAt.ToLocalTime();
                    string body = $@"
                        <p>Hola {account.User.FirstName},</p>
                        <p>Se ha realizado un retiro desde su cuenta terminada en {ultimosCuatro}.</p>
                        <ul>
                            <li><strong>Monto retirado:</strong> {request.Amount:C}</li>
                            <li><strong>Fecha y hora:</strong> {fechaHoraLocal:dd/MM/yyyy hh:mm:ss tt}</li>
                        </ul>
                        <p>Si usted no reconoce esta operación, comuníquese con la entidad bancaria.</p>";

                    await _emailService.SendNotificationEmailAsync(account.User.Email, subject, body);
                }
                catch { }
            }

            return BuildApproved(transaction, request.Amount);
        }

        public async Task<TransactionResponseDto> ProcessCreditCardPaymentAsync(PayCreditCardRequestDto request, int cashierId)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("El monto a pagar debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(request.SourceAccountNumber);
            if (account == null || account.Status != AccountStatus.Activa)
            {
                string accNum = account?.AccountNumber ?? request.SourceAccountNumber;
                return await RegisterRejectedAsync(accNum, request.Amount,
                    "El número de cuenta ingresado no corresponde a una cuenta válida.", cashierId);
            }

            var card = await _creditCardRepository.GetByCardNumberAsync(request.CardNumber);
            if (card == null || card.Status != "Activa")
            {
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El número de tarjeta ingresado no corresponde a una tarjeta válida.", cashierId);
            }

            if (card.CurrentDebt <= 0)
            {
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "La tarjeta seleccionada no tiene deuda pendiente.", cashierId);
            }

            var appliedAmount = Math.Min(request.Amount, card.CurrentDebt);

            if (account.Balance < appliedAmount)
            {
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
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

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
            catch { }

            return BuildApproved(transaction, appliedAmount);
        }

        public async Task<TransactionResponseDto> ProcessLoanPaymentAsync(PayLoanRequestDto request, int cashierId)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("El monto a pagar debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(request.SourceAccountNumber);
            if (account == null || account.Status != AccountStatus.Activa)
            {
                string accNum = account?.AccountNumber ?? request.SourceAccountNumber;
                return await RegisterRejectedAsync(accNum, request.Amount,
                    "El número de cuenta ingresado no corresponde a una cuenta válida.", cashierId);
            }

            var loan = await _loanRepository.GetByLoanNumberWithInstallmentsAsync(request.LoanNumber);
            if (loan == null || loan.Status == "Completado")
            {
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El número de préstamo ingresado no corresponde a un préstamo válido.", cashierId);
            }

            var remainingDebt = loan.Installments.Sum(i => i.PendingInstallmentAmount);
            if (remainingDebt <= 0)
            {
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El préstamo seleccionado no tiene cuotas pendientes de pago.", cashierId);
            }

            var appliedAmount = Math.Min(request.Amount, remainingDebt);

            if (account.Balance < appliedAmount)
            {
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
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

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
            catch { }

            return BuildApproved(transaction, appliedAmount);
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
                return await RegisterRejectedAsync(accNum, request.Amount,
                    "El número de cuenta origen ingresado no corresponde a una cuenta válida.", cashierId);
            }

            var tgtAcc = await _accountRepository.GetByAccountNumberAsync(request.DestinationAccountNumber);
            if (tgtAcc == null || tgtAcc.Status != AccountStatus.Activa)
            {
                return await RegisterRejectedAsync(srcAcc.AccountNumber, request.Amount,
                    "El número de cuenta destino ingresado no corresponde a una cuenta válida.", cashierId);
            }

            if (srcAcc.Balance < request.Amount)
            {
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
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            try
            {
                var fechaHoraLocal = debitTx.CreatedAt.ToLocalTime();
                string srcLast4 = srcAcc.AccountNumber[^4..];
                string tgtLast4 = tgtAcc.AccountNumber[^4..];

                if (srcAcc.User != null && !string.IsNullOrEmpty(srcAcc.User.Email))
                {
                    string subjectSrc = $"Transacción realizada a la cuenta {tgtLast4}";
                    string bodySrc = $@"
                        <p>Hola {srcAcc.User.FirstName},</p>
                        <p>Se ha debitado dinero de su cuenta para realizar una transferencia.</p>
                        <ul>
                            <li><strong>Monto transferido:</strong> {request.Amount:C}</li>
                            <li><strong>Cuenta origen:</strong> terminada en {srcLast4}</li>
                            <li><strong>Cuenta destino:</strong> terminada en {tgtLast4}</li>
                            <li><strong>Fecha y hora:</strong> {fechaHoraLocal:dd/MM/yyyy hh:mm:ss tt}</li>
                        </ul>
                        <p>Si usted no reconoce esta operación, comuníquese con la entidad bancaria inmediatamente.</p>";

                    await _emailService.SendNotificationEmailAsync(srcAcc.User.Email, subjectSrc, bodySrc);
                }

                if (tgtAcc.User != null && !string.IsNullOrEmpty(tgtAcc.User.Email))
                {
                    string subjectTgt = $"Transacción enviada desde la cuenta {srcLast4}";
                    string bodyTgt = $@"
                        <p>Hola {tgtAcc.User.FirstName},</p>
                        <p>Se ha acreditado una transferencia en su cuenta.</p>
                        <ul>
                            <li><strong>Monto recibido:</strong> {request.Amount:C}</li>
                            <li><strong>Cuenta origen:</strong> terminada en {srcLast4}</li>
                            <li><strong>Cuenta destino:</strong> terminada en {tgtLast4}</li>
                            <li><strong>Fecha y hora:</strong> {fechaHoraLocal:dd/MM/yyyy hh:mm:ss tt}</li>
                        </ul>
                        <p>Si usted no reconoce esta operación, comuníquese con la entidad bancaria.</p>";

                    await _emailService.SendNotificationEmailAsync(tgtAcc.User.Email, subjectTgt, bodyTgt);
                }
            }
            catch { }

            return BuildApproved(debitTx, request.Amount);
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