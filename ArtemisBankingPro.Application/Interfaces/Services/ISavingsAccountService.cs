using ArtemisBankingPro.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface ISavingsAccountService
    {
        Task<AccountResponseDto> CreateSecondaryAccountAsync(CreateSecondaryAccountDto dto);

        Task CancelSecondaryAccountAsync(string accountNumber);

        Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(string accountNumber, int page, int pageSize);
    }
}
