using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Domain.Entities;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Loans.Queries
{
    public class GetLoanByNumberQuery : IRequest<LoanResponseDto>
    {
        public string LoanNumber { get; set; } = string.Empty;
    }
}
