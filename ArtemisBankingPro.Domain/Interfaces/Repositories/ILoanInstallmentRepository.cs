using ArtemisBankingPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Domain.Interfaces.Repositories
{
    public interface ILoanInstallmentRepository : IGenericRepository<LoanInstallment>
    {
        Task<IReadOnlyList<LoanInstallment>> GetPendingInstallmentsAsync(int loanId);
        Task AddRangeAsync(IEnumerable<LoanInstallment> installments);
    }
}
