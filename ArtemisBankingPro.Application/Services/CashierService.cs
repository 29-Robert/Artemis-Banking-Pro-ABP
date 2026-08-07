using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Services
{
    public class CashierService : ICashierService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanInstallmentRepository _installmentRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;

        public CashierService(
            ILoanRepository loanRepository,
            ILoanInstallmentRepository installmentRepository,
            ICreditCardRepository creditCardRepository)
        {
            _loanRepository = loanRepository;
            _installmentRepository = installmentRepository;
            _creditCardRepository = creditCardRepository;
        }
        //DEPÓSITO
        public async Task ProcessDepositAsync(string targetAccountNumber, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto del depósito debe ser mayor a cero.");
           
            var account = await _accountRepository.GetByAccountNumberAsync(targetAccountNumber);
            if (account == null) throw new Exception("La cuenta destino no existe.");
            if (account.Status != AccountStatus.Activa) throw new Exception("La cuenta está inactiva.");
            account.Balance += amount;
            await _accountRepository.UpdateAsync(account);


 
            await Task.CompletedTask;
        }
        // RETIRO     
        public async Task ProcessWithdrawalAsync(string sourceAccountNumber, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto del retiro debe ser mayor a cero.");
           
             var account = await _accountRepository.GetByAccountNumberAsync(sourceAccountNumber);
             if (account == null) throw new Exception("La cuenta origen no existe.");
            if (account.Status != AccountStatus.Activa) throw new Exception("La cuenta está inactiva.");
            if (account.Balance < amount) throw new Exception("Fondos insuficientes.");
            account.Balance -= amount;
            await _accountRepository.UpdateAsync(account);

            await Task.CompletedTask;
        }
        // PAGO A TARJETA DE CRÉDITO
        public async Task ProcessCreditCardPaymentAsync(string sourceAccountNumber, string cardNumber, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto del pago debe ser mayor a cero.");
            var card = await _creditCardRepository.GetByCardNumberAsync(cardNumber);
            if (card == null || card.Status != "Activa") throw new Exception("La tarjeta de crédito no existe o está cancelada.");
            if (card.CurrentDebt <= 0) throw new Exception("La tarjeta no tiene deuda pendiente.");

            // Evitar sobrepago 
            decimal effectivePayment = Math.Min(amount, card.CurrentDebt);
           
            var account = await _accountRepository.GetByAccountNumberAsync(sourceAccountNumber);
            if (account.Balance < effectivePayment) throw new Exception("Fondos insuficientes.");
            account.Balance -= effectivePayment;
            await _accountRepository.UpdateAsync(account);
            card.CurrentDebt -= effectivePayment;
            await _creditCardRepository.UpdateAsync(card);

            
        }
        // PAGO A PRÉSTAMO
        public async Task ProcessLoanPaymentAsync(string sourceAccountNumber, string loanNumber, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto del pago debe ser mayor a cero.");
            var loan = await _loanRepository.GetByLoanNumberAsync(loanNumber);
            if (loan == null) throw new Exception("El número de préstamo no existe.");
            if (loan.Status == "Completado") throw new Exception("Este préstamo ya fue completado.");
            var pendingInstallments = await _installmentRepository.GetPendingInstallmentsAsync(loan.Id);
            decimal totalPending = pendingInstallments.Sum(i => i.PendingInstallmentAmount);
            if (totalPending == 0) throw new Exception("No hay cuotas pendientes en este préstamo.");
            

            decimal effectivePayment = Math.Min(amount, totalPending);
           
             var account = await _accountRepository.GetByAccountNumberAsync(sourceAccountNumber);
             if (account.Balance < effectivePayment) throw new Exception("Fondos insuficientes.");
             account.Balance -= effectivePayment;
             await _accountRepository.UpdateAsync(account);


            // Aplicar el pago de la cuota más antigua a la más nueva
            decimal remaining = effectivePayment;
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
           
            var stillPending = await _installmentRepository.GetPendingInstallmentsAsync(loan.Id);
            if (!stillPending.Any())
            {
                loan.Status = "Completado";
                await _loanRepository.UpdateAsync(loan);
            }
          
        }
        //TRANSFERENCIA A TERCEROS 
        public async Task ProcessThirdPartyTransferAsync(string sourceAccount, string targetAccount, decimal amount, string cashierId)
        {
            if (amount <= 0) throw new Exception("El monto de la transferencia debe ser mayor a cero.");
            if (sourceAccount == targetAccount) throw new Exception("La cuenta origen y destino no pueden ser la misma.");
           
             var source = await _accountRepository.GetByAccountNumberAsync(sourceAccount);
             var target = await _accountRepository.GetByAccountNumberAsync(targetAccount);
             if (source == null) throw new Exception("La cuenta origen no existe.");
             if (target == null) throw new Exception("La cuenta destino no existe.");
             if (source.Balance < amount) throw new Exception("Fondos insuficientes.");
             source.Balance -= amount;
             target.Balance += amount;
             await _accountRepository.UpdateAsync(source);
             await _accountRepository.UpdateAsync(target);

            
            await Task.CompletedTask;
        }
    }
}
