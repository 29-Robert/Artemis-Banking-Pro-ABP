using ArtemisBankingPro.Application.DTOs.Account;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface ISavingsAccountService
    {
        Task<SavingsAccountDetailDto> CreateSecondaryAccountAsync(CreateSecondaryAccountDto dto);

        Task CancelSecondaryAccountAsync(string accountNumber);

        Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(string accountNumber, int page, int pageSize);
    }
}
