using ArtemisBankingPro.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Domain.Interfaces.Repositories
{
    public interface ILoanRepository : IGenericRepository<Loan>
    {
        Task<Loan> GetByLoanNumberAsync(string loanNumber);
        Task<Loan> GetByLoanNumberWithInstallmentsAsync(string loanNumber);
        Task<bool> HasActiveLoanAsync(int clientId);              
        Task<IReadOnlyList<Loan>> GetLoansByClientAsync(int clientId); 

        Task<Loan> GetByIdWithDetailsAsync(int id); 
        Task<(IReadOnlyList<Loan> Items, int TotalCount)> SearchAsync(string? cedula, string? status, int pageNumber, int pageSize);
        Task<bool> LoanNumberExistsAsync(string loanNumber);
        Task<decimal> GetTotalActiveDebtByClientAsync(int clientId);
        Task<decimal> GetTotalActiveDebtSystemWideAsync();
    }
}