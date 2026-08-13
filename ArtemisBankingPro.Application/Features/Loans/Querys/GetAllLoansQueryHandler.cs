using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Loans.Querys
{
    public class GetAllLoansQueryHandler : IRequestHandler<GetAllLoansQuery, List<LoanResponseDto>>
    {
        private readonly ILoanService _loanService;
        public GetAllLoansQueryHandler(ILoanService loanService)
        {
            _loanService = loanService;
        }
        public async Task<List<LoanResponseDto>> Handle(GetAllLoansQuery request, CancellationToken cancellationToken)
        {
            return await _loanService.GetAllLoansAsync();
        }
    }
}