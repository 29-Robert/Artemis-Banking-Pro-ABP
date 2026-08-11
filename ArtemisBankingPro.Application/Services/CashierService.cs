using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;

namespace ArtemisBankingPro.Application.Services
{
    public class CashierService : ICashierService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanInstallmentRepository _installmentRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IEmailService _emailService;

        public CashierService(
            ILoanRepository loanRepository,
            ILoanInstallmentRepository installmentRepository,
            ICreditCardRepository creditCardRepository,
            ISavingsAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IEmailService emailService)
        {
            _loanRepository = loanRepository;
            _installmentRepository = installmentRepository;
            _creditCardRepository = creditCardRepository;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _emailService = emailService;
        }

        public async Task ProcessDepositAsync(string targetAccountNumber, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto del depósito debe ser mayor a cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(targetAccountNumber);
            if (account == null) throw new Exception("La cuenta destino no existe.");
            if (account.Status != AccountStatus.Activa) throw new Exception("La cuenta está cancelada.");

            account.Balance += amount;

            await _accountRepository.UpdateAsync(account);
            await _transactionRepository.AddAsync(new Transaction
            {
                AccountNumber = account.AccountNumber,
                Type = TransactionType.Credito,
                Amount = amount,
                Status = TransactionStatus.Aprobada,
                Description = "Depósito realizado por cajero.",
                RelatedEntity = "CashierDeposit",
                PerformedByUserId = ParseUserId(cashierId),
                CreatedAt = DateTime.UtcNow
            });

            await _transactionRepository.SaveChangesAsync();

            if (account.User != null)
            {
                await _emailService.SendNotificationEmailAsync(
                    account.User.Email,
                    "Depósito realizado",
                    $"Se realizó un depósito de RD${amount:N2} a su cuenta terminada en {Last4(account.AccountNumber)}.");
            }
        }

        public async Task ProcessWithdrawalAsync(string sourceAccountNumber, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto del retiro debe ser mayor a cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(sourceAccountNumber);
            if (account == null) throw new Exception("La cuenta origen no existe.");
            if (account.Status != AccountStatus.Activa) throw new Exception("La cuenta está cancelada.");

            if (account.Balance < amount)
            {
                await RegisterRejectedAsync(account.AccountNumber, amount, "Retiro rechazado por fondos insuficientes.", cashierId);
                throw new Exception("Fondos insuficientes.");
            }

            account.Balance -= amount;

            await _accountRepository.UpdateAsync(account);
            await _transactionRepository.AddAsync(new Transaction
            {
                AccountNumber = account.AccountNumber,
                Type = TransactionType.Debito,
                Amount = amount,
                Status = TransactionStatus.Aprobada,
                Description = "Retiro realizado por cajero.",
                RelatedEntity = "CashierWithdrawal",
                PerformedByUserId = ParseUserId(cashierId),
                CreatedAt = DateTime.UtcNow
            });

            await _transactionRepository.SaveChangesAsync();
        }

        public async Task ProcessCreditCardPaymentAsync(string sourceAccountNumber, string cardNumber, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto del pago debe ser mayor a cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(sourceAccountNumber);
            if (account == null) throw new Exception("La cuenta origen no existe.");
            if (account.Status != AccountStatus.Activa) throw new Exception("La cuenta está cancelada.");

            var card = await _creditCardRepository.GetByCardNumberAsync(cardNumber);
            if (card == null || card.Status != "Activa") throw new Exception("La tarjeta no existe o está cancelada.");
            if (card.CurrentDebt <= 0) throw new Exception("La tarjeta no tiene deuda pendiente.");

            if (amount > card.CurrentDebt)
            {
                await RegisterRejectedAsync(account.AccountNumber, amount, "Pago a tarjeta rechazado por sobrepago.", cashierId);
                throw new Exception("No se permiten sobrepagos a tarjetas.");
            }

            if (account.Balance < amount)
            {
                await RegisterRejectedAsync(account.AccountNumber, amount, "Pago a tarjeta rechazado por fondos insuficientes.", cashierId);
                throw new Exception("Fondos insuficientes.");
            }

            account.Balance -= amount;
            card.CurrentDebt -= amount;

            await _accountRepository.UpdateAsync(account);
            await _creditCardRepository.UpdateAsync(card);

            await _transactionRepository.AddAsync(new Transaction
            {
                AccountNumber = account.AccountNumber,
                Type = TransactionType.Debito,
                Amount = amount,
                Status = TransactionStatus.Aprobada,
                Description = $"Pago a tarjeta terminada en {Last4(card.CardNumber)}.",
                RelatedEntity = "CreditCardPayment",
                PerformedByUserId = ParseUserId(cashierId),
                CreatedAt = DateTime.UtcNow
            });

            await _transactionRepository.SaveChangesAsync();
        }

        public async Task ProcessLoanPaymentAsync(string sourceAccountNumber, string loanNumber, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto del pago debe ser mayor a cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(sourceAccountNumber);
            if (account == null) throw new Exception("La cuenta origen no existe.");
            if (account.Status != AccountStatus.Activa) throw new Exception("La cuenta está cancelada.");

            var loan = await _loanRepository.GetByLoanNumberAsync(loanNumber);
            if (loan == null) throw new Exception("El préstamo no existe.");
            if (loan.Status == "Completado") throw new Exception("Este préstamo ya fue completado.");

            var pendingInstallments = await _installmentRepository.GetPendingInstallmentsAsync(loan.Id);
            var totalPending = pendingInstallments.Sum(x => x.PendingInstallmentAmount);

            if (amount > totalPending)
            {
                await RegisterRejectedAsync(account.AccountNumber, amount, "Pago a préstamo rechazado por sobrepago.", cashierId);
                throw new Exception("No se permiten sobrepagos a préstamos.");
            }

            if (account.Balance < amount)
            {
                await RegisterRejectedAsync(account.AccountNumber, amount, "Pago a préstamo rechazado por fondos insuficientes.", cashierId);
                throw new Exception("Fondos insuficientes.");
            }

            account.Balance -= amount;
            await _accountRepository.UpdateAsync(account);

            var remaining = amount;

            foreach (var installment in pendingInstallments)
            {
                if (remaining <= 0) break;

                if (remaining >= installment.PendingInstallmentAmount)
                {
                    remaining -= installment.PendingInstallmentAmount;
                    installment.PendingInstallmentAmount = 0;
                    installment.PaymentStatus = "Pagada";
                    installment.IsLate = false;
                }
                else
                {
                    installment.PendingInstallmentAmount -= remaining;
                    installment.PaymentStatus = "Parcialmente pagada";
                    remaining = 0;
                }

                await _installmentRepository.UpdateAsync(installment);
            }

            if (!(await _installmentRepository.GetPendingInstallmentsAsync(loan.Id)).Any())
            {
                loan.Status = "Completado";
                await _loanRepository.UpdateAsync(loan);
            }

            await _transactionRepository.AddAsync(new Transaction
            {
                AccountNumber = account.AccountNumber,
                Type = TransactionType.Debito,
                Amount = amount,
                Status = TransactionStatus.Aprobada,
                Description = $"Pago a préstamo {loan.LoanNumber}.",
                RelatedEntity = "LoanPayment",
                PerformedByUserId = ParseUserId(cashierId),
                CreatedAt = DateTime.UtcNow
            });

            await _transactionRepository.SaveChangesAsync();
        }

        public async Task ProcessThirdPartyTransferAsync(string sourceAccount, string targetAccount, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto debe ser mayor a cero.");
            if (sourceAccount == targetAccount) throw new Exception("La cuenta origen y destino no pueden ser la misma.");

            var source = await _accountRepository.GetByAccountNumberAsync(sourceAccount);
            var target = await _accountRepository.GetByAccountNumberAsync(targetAccount);

            if (source == null) throw new Exception("La cuenta origen no existe.");
            if (target == null) throw new Exception("La cuenta destino no existe.");
            if (source.Status != AccountStatus.Activa || target.Status != AccountStatus.Activa) throw new Exception("Ambas cuentas deben estar activas.");

            if (source.Balance < amount)
            {
                await RegisterRejectedAsync(source.AccountNumber, amount, "Transferencia rechazada por fondos insuficientes.", cashierId);
                throw new Exception("Fondos insuficientes.");
            }

            source.Balance -= amount;
            target.Balance += amount;

            await _accountRepository.UpdateAsync(source);
            await _accountRepository.UpdateAsync(target);

            await _transactionRepository.AddCrossEntryAsync(
                new Transaction
                {
                    AccountNumber = source.AccountNumber,
                    Type = TransactionType.Debito,
                    Amount = amount,
                    Status = TransactionStatus.Aprobada,
                    Description = $"Transferencia hacia cuenta {target.AccountNumber}.",
                    RelatedEntity = "ThirdPartyTransfer",
                    PerformedByUserId = ParseUserId(cashierId),
                    CreatedAt = DateTime.UtcNow
                },
                new Transaction
                {
                    AccountNumber = target.AccountNumber,
                    Type = TransactionType.Credito,
                    Amount = amount,
                    Status = TransactionStatus.Aprobada,
                    Description = $"Transferencia recibida desde cuenta {source.AccountNumber}.",
                    RelatedEntity = "ThirdPartyTransfer",
                    PerformedByUserId = ParseUserId(cashierId),
                    CreatedAt = DateTime.UtcNow
                });
        }

        private async Task RegisterRejectedAsync(string accountNumber, decimal amount, string reason, string cashierId)
        {
            await _transactionRepository.AddAsync(new Transaction
            {
                AccountNumber = accountNumber,
                Type = TransactionType.Debito,
                Amount = amount,
                Status = TransactionStatus.Rechazada,
                Description = reason,
                RelatedEntity = "CashierRejected",
                PerformedByUserId = ParseUserId(cashierId),
                CreatedAt = DateTime.UtcNow
            });

            await _transactionRepository.SaveChangesAsync();
        }

        private static int? ParseUserId(string userId)
        {
            return int.TryParse(userId, out var id) ? id : null;
        }

        private static string Last4(string value)
        {
            return string.IsNullOrWhiteSpace(value) || value.Length < 4
                ? value
                : value[^4..];
        }
    }
}