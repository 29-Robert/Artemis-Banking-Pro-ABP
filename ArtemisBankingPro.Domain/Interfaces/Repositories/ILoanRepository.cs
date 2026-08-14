
using ArtemisBankingPro.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Domain.Interfaces.Repositories
{
    public interface ILoanRepository : IGenericRepository<Loan>
    {
        Task<Loan> GetByLoanNumberAsync(string loanNumber);
        Task<Loan> GetByLoanNumberWithInstallmentsAsync(string loanNumber);
        Task<bool> HasActiveLoanAsync(string clientId);
        Task<IReadOnlyList<Loan>> GetLoansByClientAsync(string clientId);
    }
}