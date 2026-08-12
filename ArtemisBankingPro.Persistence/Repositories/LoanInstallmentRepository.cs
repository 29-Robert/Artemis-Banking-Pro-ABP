using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using ArtemisBankingPro.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;



namespace ArtemisBankingPro.Persistence.Repositories
{
    public class LoanInstallmentRepository : GenericRepository<LoanInstallment>, ILoanInstallmentRepository
    {
        public LoanInstallmentRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<LoanInstallment>> GetPendingInstallmentsAsync(int loanId)
        {
            return await _dbContext.Set<LoanInstallment>()
                .Where(i => i.LoanId == loanId && i.PaymentStatus != "Pagada")
                .OrderBy(i => i.DueDate) // Siempre ordenado por la cuota más antigua
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<LoanInstallment> installments)
        {
            await _dbContext.Set<LoanInstallment>().AddRangeAsync(installments);
        }
    }
}
