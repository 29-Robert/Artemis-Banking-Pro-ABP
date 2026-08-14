using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.DTOs.Home;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface ISavingsAccountService
    {
        Task<SavingsAccountDetailDto> CreateSecondaryAccountAsync(CreateSecondaryAccountDto dto);
        Task CancelSecondaryAccountAsync(string accountNumber);
        Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(string accountNumber, int page, int pageSize);
        
        // Métodos agregados para el cliente:
        Task<ClientHomeDto> GetClientHomeDataAsync(string clientId);
        Task ProcessCreditCardPaymentOwnAccountAsync(string sourceAccountNumber, string cardNumber, decimal amount, string clientId);
        Task ProcessLoanPaymentOwnAccountAsync(string sourceAccountNumber, string loanNumber, decimal amount, string clientId);
        Task ProcessCashAdvanceAsync(string sourceAccountNumber, string cardNumber, decimal amount, string clientId);

        // Métodos administrativos para bloqueos y retenciones:
        Task BlockAccountAsync(string accountNumber);
        Task UnblockAccountAsync(string accountNumber);
        Task SetBlockedAmountAsync(string accountNumber, decimal amount);

        Task CreditToMainAsync(string clientId, decimal amount);
    }
}
