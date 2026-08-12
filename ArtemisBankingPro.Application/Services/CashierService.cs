using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace ArtemisBankingPro.Application.Services
{
    public class CashierService : ICashierService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanInstallmentRepository _installmentRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ILoanRepository _loanRepository;

        public CashierService(
            ILoanRepository loanRepository,
            ILoanInstallmentRepository installmentRepository,
            ICreditCardRepository creditCardRepository,
            ISavingsAccountRepository accountRepository, 
            ITransactionRepository transactionRepository,
            ICreditCardRepository creditCardRepository,
            ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
            _installmentRepository = installmentRepository;
            _creditCardRepository = creditCardRepository;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _creditCardRepository = creditCardRepository;
            _loanRepository = loanRepository;
        }

        public async Task ProcessDepositAsync(string targetAccountNumber, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto a depositar debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(targetAccountNumber);
            if (account == null) throw new Exception("La cuenta de destino no existe.");
            if (account.Status == AccountStatus.Cancelada) throw new Exception("Operación denegada. La cuenta de destino se encuentra cancelada.");

            account.Balance += amount;
            await _accountRepository.UpdateAsync(account);

            var transaction = new Transaction
            {
                AccountNumber = targetAccountNumber,
                Type = TransactionType.Credito,
                Amount = amount,
                Status = TransactionStatus.Aprobada,
                Description = "Depósito en efectivo por cajero",
                PerformedByUserId = !string.IsNullOrEmpty(cashierId) ? int.Parse(cashierId) : null,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();
        }
        }

        public async Task ProcessWithdrawalAsync(string sourceAccountNumber, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto a retirar debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(sourceAccountNumber);
            if (account == null) throw new Exception("La cuenta de origen no existe.");
            if (account.Status == AccountStatus.Cancelada) throw new Exception("Operación denegada. La cuenta se encuentra cancelada.");
            if (account.Balance < amount) throw new Exception("Fondos insuficientes en la cuenta de ahorros.");

            if (account.Balance < amount)
            {
                await RegisterRejectedAsync(account.AccountNumber, amount, "Retiro rechazado por fondos insuficientes.", cashierId);
                throw new Exception("Fondos insuficientes.");
            }

            account.Balance -= amount;
            await _accountRepository.UpdateAsync(account);

            var transaction = new Transaction
            {
                AccountNumber = sourceAccountNumber,
                Type = TransactionType.Debito,
                Amount = amount,
                Status = TransactionStatus.Aprobada,
                Description = "Retiro en efectivo por cajero",
                PerformedByUserId = !string.IsNullOrEmpty(cashierId) ? int.Parse(cashierId) : null,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();
        }

        public async Task ProcessCreditCardPaymentAsync(string sourceAccountNumber, string cardNumber, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto a pagar debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(sourceAccountNumber);
            if (account == null) throw new Exception("La cuenta de origen no existe.");
            if (account.Status == AccountStatus.Cancelada) throw new Exception("Operación denegada. La cuenta se encuentra cancelada.");
            if (account.Balance < amount) throw new Exception("Fondos insuficientes en la cuenta de ahorros.");

            var card = await _creditCardRepository.GetByCardNumberAsync(cardNumber);
            if (card == null) throw new Exception("La tarjeta de crédito especificada no existe.");
            if (card.Status == "Cancelada") throw new Exception("Operación denegada. La tarjeta de crédito se encuentra cancelada.");

            card.CurrentDebt = Math.Max(0, card.CurrentDebt - amount);
            await _creditCardRepository.UpdateAsync(card);

            if (account.Balance < amount)
            {
                await RegisterRejectedAsync(account.AccountNumber, amount, "Pago a tarjeta rechazado por fondos insuficientes.", cashierId);
                throw new Exception("Fondos insuficientes.");
            }

            account.Balance -= amount;
            card.CurrentDebt -= amount;

            await _accountRepository.UpdateAsync(account);
            await _creditCardRepository.UpdateAsync(card);

            var transaction = new Transaction
            {
                AccountNumber = sourceAccountNumber,
                Type = TransactionType.Debito,
                Amount = amount,
                Status = TransactionStatus.Aprobada,
                Description = $"Pago de tarjeta de crédito {cardNumber}",
                PerformedByUserId = !string.IsNullOrEmpty(cashierId) ? int.Parse(cashierId) : null,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();
        }

        public async Task ProcessLoanPaymentAsync(string sourceAccountNumber, string loanNumber, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto a pagar debe ser mayor que cero.");

            var account = await _accountRepository.GetByAccountNumberAsync(sourceAccountNumber);
            if (account == null) throw new Exception("La cuenta de origen no existe.");
            if (account.Status == AccountStatus.Cancelada) throw new Exception("Operación denegada. La cuenta se encuentra cancelada.");
            if (account.Balance < amount) throw new Exception("Fondos insuficientes en la cuenta de ahorros.");

            var loan = await _loanRepository.GetByLoanNumberAsync(loanNumber);
            if (loan == null) throw new Exception("El préstamo especificado no existe.");
            if (loan.Status == "Cancelado" || loan.Status == "Pagado") throw new Exception("Operación denegada. El préstamo ya se encuentra cancelado o pagado.");

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
                    installment.IsLate = false;
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

            if (!(await _installmentRepository.GetPendingInstallmentsAsync(loan.Id)).Any())
            {
                loan.Status = "Completado";
            await _loanRepository.UpdateAsync(loan);

            account.Balance -= amount;
            await _accountRepository.UpdateAsync(account);

            var transaction = new Transaction
            {
                AccountNumber = sourceAccountNumber,
                Type = TransactionType.Debito,
                Amount = amount,
                Status = TransactionStatus.Aprobada,
                Description = $"Pago de préstamo {loanNumber}",
                PerformedByUserId = !string.IsNullOrEmpty(cashierId) ? int.Parse(cashierId) : null,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();
        }

        public async Task ProcessPartyTransferAsync(string sourceAccount, string targetAccount, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto a transferir debe ser mayor que cero.");
            if (sourceAccount == targetAccount) throw new Exception("La cuenta de origen y destino no pueden ser la misma.");

            var srcAcc = await _accountRepository.GetByAccountNumberAsync(sourceAccount);
            if (srcAcc == null) throw new Exception("La cuenta de origen no existe.");
            if (srcAcc.Status == AccountStatus.Cancelada) throw new Exception("Operación denegada. La cuenta de origen se encuentra cancelada.");
            if (srcAcc.Balance < amount) throw new Exception("Fondos insuficientes en la cuenta de origen.");

            var tgtAcc = await _accountRepository.GetByAccountNumberAsync(targetAccount);
            if (tgtAcc == null) throw new Exception("La cuenta de destino no existe.");
            if (tgtAcc.Status == AccountStatus.Cancelada) throw new Exception("Operación denegada. La cuenta de destino se encuentra cancelada.");

            srcAcc.Balance -= amount;
            tgtAcc.Balance += amount;

            await _accountRepository.UpdateAsync(srcAcc);
            await _accountRepository.UpdateAsync(tgtAcc);

            var debitTx = new Transaction
            {
                AccountNumber = sourceAccount,
                Type = TransactionType.Debito,
                Amount = amount,
                Status = TransactionStatus.Aprobada,
                Description = $"Transferencia a terceros (Caja) hacia cuenta {targetAccount}",
                PerformedByUserId = !string.IsNullOrEmpty(cashierId) ? int.Parse(cashierId) : null,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(debitTx);

            var creditTx = new Transaction
            {
                AccountNumber = targetAccount,
                Type = TransactionType.Credito,
                Amount = amount,
                Status = TransactionStatus.Aprobada,
                Description = $"Transferencia recibida (Caja) desde cuenta {sourceAccount}",
                PerformedByUserId = !string.IsNullOrEmpty(cashierId) ? int.Parse(cashierId) : null,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(creditTx);

            await _accountRepository.SaveChangesAsync();
        }

        private async Task RegisterRejectedAsync(string accountNumber, decimal amount, string reason, string cashierId)
        {
            var transaction = new Transaction
            {
                AccountNumber = accountNumber,
                Type = TransactionType.Debito,
                Amount = amount,
                Status = TransactionStatus.Rechazada, 
                Description = reason,
                PerformedByUserId = !string.IsNullOrEmpty(cashierId) && int.TryParse(cashierId, out var id) ? id : null,
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.SaveChangesAsync();
        }
    }
}
    
