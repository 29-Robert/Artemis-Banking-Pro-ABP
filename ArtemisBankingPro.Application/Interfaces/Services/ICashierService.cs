using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface ICashierService
    {
        Task ProcessDepositAsync(string targetAccountNumber, decimal amount, string cashierId);
        Task ProcessWithdrawalAsync(string sourceAccountNumber, decimal amount, string cashierId);
        Task ProcessCreditCardPaymentAsync(string sourceAccountNumber, string cardNumber, decimal amount, string cashierId);
        Task ProcessLoanPaymentAsync(string sourceAccountNumber, string loanNumber, decimal amount, string cashierId);
        Task ProcessThirdPartyTransferAsync(string sourceAccount, string targetAccount, decimal amount, string cashierId);
    }
}
