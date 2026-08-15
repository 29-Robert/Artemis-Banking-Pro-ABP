using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using ArtemisBankingPro.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Persistence.Repositories
{
    public class LoanRepository : GenericRepository<Loan>, ILoanRepository
    {
        public LoanRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Loan> GetByLoanNumberAsync(string loanNumber)
        {
            return await _dbContext.Set<Loan>()
                .Include(l => l.Installments)
                .FirstOrDefaultAsync(l => l.LoanNumber == loanNumber);
        }

        public async Task<Loan> GetByLoanNumberWithInstallmentsAsync(string loanNumber)
        {
            return await _dbContext.Set<Loan>()
                .Include(l => l.Installments)
                .FirstOrDefaultAsync(l => l.LoanNumber == loanNumber);
        }

        public async Task<bool> HasActiveLoanAsync(string clientId)
        {
            int cId = int.Parse(clientId);
            return await _dbContext.Set<Loan>()
                .AnyAsync(l => l.ClientId == cId && l.Status == "Activo");
        }

        public async Task<IReadOnlyList<Loan>> GetLoansByClientAsync(string clientId)
        {
            int cId = int.Parse(clientId);
            return await _dbContext.Set<Loan>()
                .Where(l => l.ClientId == cId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }
    }
}
