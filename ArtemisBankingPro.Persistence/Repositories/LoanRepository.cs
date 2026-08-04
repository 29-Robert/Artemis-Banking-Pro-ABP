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
    public class LoanRepository : GenericRepository<Loan>, ILoanRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public LoanRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Loan> GetByLoanNumberAsync(string loanNumber)
        {
            return await _dbContext.Set<Loan>()
                .Include(l => l.Installments)
                .FirstOrDefaultAsync(l => l.LoanNumber == loanNumber);
        }

        public async Task<bool> HasActiveLoanAsync(string clientId)
        {
            return await _dbContext.Set<Loan>()
                .AnyAsync(l => l.ClientId == clientId && l.Status == "Activo");
        }

        public async Task<IReadOnlyList<Loan>> GetLoansByClientAsync(string clientId)
        {
            return await _dbContext.Set<Loan>()
                .Where(l => l.ClientId == clientId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }
    }
}
