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
using ArtemisBankingPro.Application.DTOs.Ac;

namespace ArtemisBankingPro.Application.Services
{
    public class CashierService : ICashierService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanInstallmentRepository _loanInstallmentRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;

        public CashierService(
            ILoanRepository loanRepository,
            ILoanInstallmentRepository loanInstallmentRepository,
            ICreditCardRepository creditCardRepository,
            ISavingsAccountRepository accountRepository,
            ITransactionRepository transactionRepository)
        {
            _loanRepository = loanRepository;
            _loanInstallmentRepository = loanInstallmentRepository;
            _creditCardRepository = creditCardRepository;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
        }

        // ============================================================
        // DEPÓSITO
        // ============================================================
        public async Task<TransactionResponseDto> ProcessDepositAsync(DepositRequestDto request, int cashierId)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(request.DestinationAccountNumber);
            if (account == null)
                throw new KeyNotFoundException("La cuenta de destino no existe.");

            if (account.Status == AccountStatus.Cancelada)
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "La cuenta de destino se encuentra cancelada.", cashierId);

            account.Balance += request.Amount;
            await _accountRepository.UpdateAsync(account);

            var transaction = new Transaction
            {
                AccountNumber = account.AccountNumber,
                Type = TransactionType.Credito,
                Amount = request.Amount,
                Status = TransactionStatus.Aprobada,
                Description = "Depósito en efectivo por cajero",
                PerformedByUserId = cashierId,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();

            return BuildApproved(transaction, request.Amount);
        }

        // ============================================================
        // RETIRO
        // ============================================================
        public async Task<TransactionResponseDto> ProcessWithdrawalAsync(WithdrawRequestDto request, int cashierId)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(request.SourceAccountNumber);
            if (account == null)
                throw new KeyNotFoundException("La cuenta de origen no existe.");

            if (account.Status == AccountStatus.Cancelada)
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "La cuenta de origen se encuentra cancelada.", cashierId);

            if (account.Balance < request.Amount)
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "Fondos insuficientes en la cuenta de ahorros.", cashierId);

            account.Balance -= request.Amount;
            await _accountRepository.UpdateAsync(account);

            var transaction = new Transaction
            {
                AccountNumber = account.AccountNumber,
                Type = TransactionType.Debito,
                Amount = request.Amount,
                Status = TransactionStatus.Aprobada,
                Description = "Retiro en efectivo por cajero",
                PerformedByUserId = cashierId,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();

            return BuildApproved(transaction, request.Amount);
        }

        // ============================================================
        // PAGO A TARJETA DE CRÉDITO (con tope anti-sobrepago)
        // ============================================================
        public async Task<TransactionResponseDto> ProcessCreditCardPaymentAsync(PayCreditCardRequestDto request, int cashierId)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(request.SourceAccountNumber);
            if (account == null)
                throw new KeyNotFoundException("La cuenta de origen no existe.");

            var card = await _creditCardRepository.GetByCardNumberAsync(request.CardNumber);
            if (card == null)
                throw new KeyNotFoundException("La tarjeta de crédito especificada no existe.");

            if (account.Status == AccountStatus.Cancelada)
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "La cuenta de origen se encuentra cancelada.", cashierId);

            if (card.Status == "Cancelada")
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "La tarjeta de crédito se encuentra cancelada.", cashierId);

            var appliedAmount = Math.Min(request.Amount, card.CurrentDebt);

            if (appliedAmount <= 0)
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "La tarjeta no tiene deuda pendiente.", cashierId);

            if (account.Balance < appliedAmount)
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "Fondos insuficientes en la cuenta de ahorros.", cashierId);

            account.Balance -= appliedAmount;
            card.CurrentDebt -= appliedAmount;

            await _accountRepository.UpdateAsync(account);
            await _creditCardRepository.UpdateAsync(card);

            var transaction = new Transaction
            {
                AccountNumber = account.AccountNumber,
                Type = TransactionType.Debito,
                Amount = appliedAmount,
                Status = TransactionStatus.Aprobada,
                Description = $"Pago de tarjeta de crédito {request.CardNumber}",
                PerformedByUserId = cashierId,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();

            return BuildApproved(transaction, appliedAmount);
        }

        // ============================================================
        // PAGO A PRÉSTAMO (aplica cuotas en orden, persiste pagos parciales)
        // ============================================================
        public async Task<TransactionResponseDto> ProcessLoanPaymentAsync(PayLoanRequestDto request, int cashierId)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(request.SourceAccountNumber);
            if (account == null)
                throw new KeyNotFoundException("La cuenta de origen no existe.");

            var loan = await _loanRepository.GetByLoanNumberWithInstallmentsAsync(request.LoanNumber);
            if (loan == null)
                throw new KeyNotFoundException("El préstamo especificado no existe.");

            if (account.Status == AccountStatus.Cancelada)
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "La cuenta de origen se encuentra cancelada.", cashierId);

            if (loan.Status == "Completado")
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El préstamo ya se encuentra completado.", cashierId);

            var remainingDebt = loan.Installments.Sum(i => i.PendingInstallmentAmount);
            var appliedAmount = Math.Min(request.Amount, remainingDebt);

            if (appliedAmount <= 0)
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "El préstamo no tiene saldo pendiente.", cashierId);

            if (account.Balance < appliedAmount)
                return await RegisterRejectedAsync(account.AccountNumber, request.Amount,
                    "Fondos insuficientes en la cuenta de ahorros.", cashierId);

            var remainingPayment = appliedAmount;
            var pendingInstallments = loan.Installments
                .Where(i => i.PaymentStatus != "Pagada")
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

            var transaction = new Transaction
            {
                AccountNumber = account.AccountNumber,
                Type = TransactionType.Debito,
                Amount = appliedAmount,
                Status = TransactionStatus.Aprobada,
                Description = $"Pago de préstamo {request.LoanNumber}",
                PerformedByUserId = cashierId,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();

            return BuildApproved(transaction, appliedAmount);
        }

        // ============================================================
        // TRANSFERENCIA A TERCEROS
        // ============================================================
        public async Task<TransactionDto> ProcessThirdPartyTransferAsync(ThirdPartyTransferRequestDto request, int cashierId)
        {
            if (request.SourceAccountNumber == request.DestinationAccountNumber)
                throw new InvalidOperationException("La cuenta de origen y destino no pueden ser la misma.");

            var srcAcc = await _accountRepository.GetByAccountNumberAsync(request.SourceAccountNumber);
            if (srcAcc == null)
                throw new KeyNotFoundException("La cuenta de origen no existe.");

            var tgtAcc = await _accountRepository.GetByAccountNumberAsync(request.DestinationAccountNumber);
            if (tgtAcc == null)
                throw new KeyNotFoundException("La cuenta de destino no existe.");

            if (srcAcc.Status == AccountStatus.Cancelada)
                return await RegisterRejectedAsync(srcAcc.AccountNumber, request.Amount,
                    "La cuenta de origen se encuentra cancelada.", cashierId);

            if (tgtAcc.Status == AccountStatus.Cancelada)
                return await RegisterRejectedAsync(srcAcc.AccountNumber, request.Amount,
                    "La cuenta de destino se encuentra cancelada.", cashierId);

            if (srcAcc.Balance < request.Amount)
                return await RegisterRejectedAsync(srcAcc.AccountNumber, request.Amount,
                    "Fondos insuficientes en la cuenta de origen.", cashierId);

            srcAcc.Balance -= request.Amount;
            tgtAcc.Balance += request.Amount;
            await _accountRepository.UpdateAsync(srcAcc);
            await _accountRepository.UpdateAsync(tgtAcc);

            var debitTx = new Transaction
            {
                AccountNumber = srcAcc.AccountNumber,
                Type = TransactionType.Debito,
                Amount = request.Amount,
                Status = TransactionStatus.Aprobada,
                Description = $"Transferencia a terceros (Caja) hacia cuenta {tgtAcc.AccountNumber}",
                PerformedByUserId = cashierId,
                CreatedAt = DateTime.UtcNow
            };
            var creditTx = new Transaction
            {
                AccountNumber = tgtAcc.AccountNumber,
                Type = TransactionType.Credito,
                Amount = request.Amount,
                Status = TransactionStatus.Aprobada,
                Description = $"Transferencia recibida (Caja) desde cuenta {srcAcc.AccountNumber}",
                PerformedByUserId = cashierId,
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddCrossEntryAsync(debitTx, creditTx);
            await _accountRepository.SaveChangesAsync();

            return BuildApproved(debitTx, request.Amount);
        }

       
        // HOME DEL CAJERO
       
        public async Task<CajeroHomeIndicatorsResponseDto> GetHomeIndicatorsAsync(int cashierId)
        {
            var today = DateTime.UtcNow.Date;

            var todaysTransactions = await _transactionRepository.GetByPerformedUserAndDateAsync(cashierId, today);

            return new CajeroHomeIndicatorsResponseDto
            {
                DepositsToday = todaysTransactions.Count(t =>
                    t.Type == TransactionType.Credito && t.Description.Contains("Depósito")),
                WithdrawalsToday = todaysTransactions.Count(t =>
                    t.Type == TransactionType.Debito && t.Description.Contains("Retiro"))
            };
        }

        
        // PREVIEWS (confirmación antes de ejecutar)
       
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

        // Helpers privados
       
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