using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Persistence.Repositories
{
    public class TransactionRepository(ApplicationDbContext dbContext) : GenericRepository<Transaction>(dbContext), ITransactionRepository
    {
        public async Task<IEnumerable<Transaction>> GetPagedByAccountAsync(string accountNumber, int page, int pageSize)
        {
            return await _dbContext.Transactions
                .Where(t => t.AccountNumber == accountNumber)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddCrossEntryAsync(Transaction debit, Transaction credit)
        {
            using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _dbContext.Transactions.AddAsync(debit);
                await _dbContext.Transactions.AddAsync(credit);
                await _dbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

            public async Task<IEnumerable<Transaction>> GetByPerformedUserAndDateAsync(int cashierId,DateTime date)
            
            {
            return await _dbContext.Transactions
                .Where(t =>
                    t.PerformedByUserId == cashierId &&
                    t.CreatedAt.Date == date.Date)
                .ToListAsync();
            }
        public async Task<IReadOnlyList<Transaction>> GetAllApprovedAsync()
        {
            return await _dbContext.Transactions
                .Where(t => t.Status == TransactionStatus.Aprobada)
                .ToListAsync();
        }

    }
    
}