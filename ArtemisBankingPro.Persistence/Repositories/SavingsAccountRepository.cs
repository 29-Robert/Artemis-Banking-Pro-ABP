using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using ArtemisBankingPro.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Persistence.Repositories
{
    public class SavingsAccountRepository(ApplicationDbContext dbContext) : GenericRepository<SavingsAccount>(dbContext), ISavingsAccountRepository
    {
        public async Task<int> CountActiveAccountsByClientIdAsync(int clientId)
        {
            return await _dbContext.SavingsAccounts.CountAsync(s => s.UserId == clientId && s.Status == AccountStatus.Activa);
        }

        public async Task<SavingsAccount> GetByAccountNumberAsync(string accountNumber)
        {
            return await _dbContext.SavingsAccounts
       .Include(s => s.User)
       .FirstOrDefaultAsync(s => s.AccountNumber == accountNumber);
        }

        public async Task<object> GetPagedAsync(int page, int pageSize, AccountStatus? status, AccountType? type, string cedula)
        {
            var query = _dbContext.SavingsAccounts
                .Include(s => s.User)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }
            if (type.HasValue)
            {
                query = query.Where(s => s.Type == type.Value);
            }
            if (!string.IsNullOrWhiteSpace(cedula))
            {
                query = query.Where(s => s.User.Cedula.Contains(cedula));
            }
            var totalCount = await query.CountAsync();
            var data = await query
                .OrderBy(s => s.Status)
                .ThenByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new { Data = data, TotalCount = totalCount };
        }

        public async Task<SavingsAccount> GetPrincipalByClientAsync(int clientId)
        {
            return await _dbContext.SavingsAccounts.FirstOrDefaultAsync(s => s.UserId == clientId && s.IsPrincipal);
        }


    }
}
