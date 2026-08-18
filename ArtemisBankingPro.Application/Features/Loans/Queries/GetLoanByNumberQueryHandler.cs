using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Loans.Queries
{
    public class GetLoanByNumberQueryHandler(ILoanService loanService)
        : IRequestHandler<GetLoanByNumberQuery, LoanResponseDto>
    {
        public async Task<LoanResponseDto> Handle(GetLoanByNumberQuery request, CancellationToken cancellationToken)
        {
            return await loanService.GetLoanByNumberAsync(request.LoanNumber);
        }
    }
}
