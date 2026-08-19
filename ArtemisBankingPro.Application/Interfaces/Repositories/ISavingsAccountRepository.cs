using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using ArtemisBankingPro.Application.DTOs.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Repositories
{
    public interface ISavingsAccountRepository : IGenericRepository<SavingsAccount>
    {
        Task<SavingsAccount> GetByAccountNumberAsync(string accountNumber);
        Task<SavingsAccount> GetPrincipalByClientAsync(int clientId);
        Task<PagedAccountResponseDto> GetPagedAsync(int page, int pageSize, AccountStatus? status, AccountType? type, string cedula);
        Task<int> CountActiveAccountsByClientIdAsync(int clientId);
    }
}
