using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Repositories
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetPagedByAccountAsync(string accountNumber, int page, int pageSize);
        Task AddCrossEntryAsync(Transaction debit, Transaction credit);
        Task<IEnumerable<Transaction>> GetByPerformedUserAndDateAsync(int cashierId,DateTime date);
        Task<IReadOnlyList<Transaction>> GetAllApprovedAsync();
    }
}
