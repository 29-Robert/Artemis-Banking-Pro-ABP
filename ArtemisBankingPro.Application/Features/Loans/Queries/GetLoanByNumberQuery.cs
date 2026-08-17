using ArtemisBankingPro.Domain.Entities;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Loans.Queries
{
    public class GetLoanByNumberQuery : IRequest<Loan?>
    {
        public string LoanNumber { get; set; } = string.Empty;
    }
}
