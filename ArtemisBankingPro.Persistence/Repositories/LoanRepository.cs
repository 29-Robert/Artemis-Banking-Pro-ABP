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
            var trimmed = loanNumber?.Trim() ?? string.Empty;
            return await _dbContext.Set<Loan>()
                .Include(l => l.Installments)
                .FirstOrDefaultAsync(l => l.LoanNumber.Trim() == trimmed);
        }

        public async Task<Loan> GetByLoanNumberWithInstallmentsAsync(string loanNumber)
        {
            var trimmed = loanNumber?.Trim() ?? string.Empty;
            return await _dbContext.Set<Loan>()
                .Include(l => l.Client)
                .Include(l => l.Installments)
                .FirstOrDefaultAsync(l => l.LoanNumber.Trim() == trimmed); 
        }

        public async Task<bool> HasActiveLoanAsync(int clientId)
        {
            return await _dbContext.Set<Loan>()
                .AnyAsync(l => l.ClientId == clientId && l.Status == "Activo");
        }

        public async Task<IReadOnlyList<Loan>> GetLoansByClientAsync(int clientId)
        {
            return await _dbContext.Set<Loan>()
                .Where(l => l.ClientId == clientId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<Loan> GetByIdWithDetailsAsync(int id)
        {
            return await _dbContext.Set<Loan>()
                .Include(l => l.Client)
                .Include(l => l.Installments)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<(IReadOnlyList<Loan> Items, int TotalCount)> SearchAsync(string? cedula, string? status, int pageNumber, int pageSize)
        {
            const int MaxPageSize = 20;
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = MaxPageSize;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;

            var query = _dbContext.Set<Loan>()
                .Include(l => l.Client)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(cedula))
            {
                query = query.Where(l => l.Client.Cedula == cedula);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "Todos" && status != "Todas" && status != "Todos/Todas")
            {
                query = query.Where(l => l.Status == status);
            }

            query = query.OrderByDescending(l => l.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> LoanNumberExistsAsync(string loanNumber)
        {
            return await _dbContext.Set<Loan>()
                .AnyAsync(l => l.LoanNumber == loanNumber);
        }

        public async Task<decimal> GetTotalActiveDebtByClientAsync(int clientId)
        {
            return await _dbContext.Set<Loan>()
                .Where(l => l.ClientId == clientId && (l.Status == "Activo" || l.Status == "Aprobado"))
                .SelectMany(l => l.Installments)
                .Where(i => i.PaymentStatus == "Pendiente")
                .SumAsync(i => i.PendingInstallmentAmount);
        }
        public async Task<decimal> GetTotalActiveDebtSystemWideAsync()
        {
            return await _dbContext.Set<Loan>()
                .Where(l => l.Status == "Activo" || l.Status == "Aprobado")
                .SumAsync(l => l.CapitalAmount);
        }
    }
}
