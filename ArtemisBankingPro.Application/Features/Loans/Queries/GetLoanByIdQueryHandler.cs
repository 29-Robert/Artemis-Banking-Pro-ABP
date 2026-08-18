using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Loans.Queries
{
    public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, LoanResponseDto>
    {
        private readonly ILoanService _loanService;
        public GetLoanByIdQueryHandler(ILoanService loanService)
        {
            _loanService = loanService;
        }
        public async Task<LoanResponseDto> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
        {
            return await _loanService.GetLoanByIdAsync(request.Id);
        }
    }
}
