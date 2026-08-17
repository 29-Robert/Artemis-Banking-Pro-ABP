using ArtemisBankingPro.Domain.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Loans.Queries
{
    public class GetLoanByNumberQueryHandler(ILoanRepository loanRepository) : IRequestHandler<GetLoanByNumberQuery, Loan?>
    {
        public async Task<Loan?> Handle(GetLoanByNumberQuery request, CancellationToken cancellationToken)
        {
            return await loanRepository.GetByLoanNumberAsync(request.LoanNumber);
        }
    }
}
