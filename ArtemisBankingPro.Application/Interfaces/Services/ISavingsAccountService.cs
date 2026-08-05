using ArtemisBankingPro.Application.DTOs.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface ISavingsAccountService
    {
        Task<SavingsAccountDetailDto> CreateSecondaryAccountAsync(CreateSavingsAccountDto dto);

        Task CancelSecondaryAccountAsync(string accountNumber);

        Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(string accountNumber, int page, int pageSize);
    }
}
