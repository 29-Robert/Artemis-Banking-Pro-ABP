using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Loans.Commands
{
    public class UpdateLoanRateCommandHandler : IRequestHandler<UpdateLoanRateCommand, Unit>
    {
        private readonly ILoanService _loanService;
        public UpdateLoanRateCommandHandler(ILoanService loanService)
        {
            _loanService = loanService;
        }
        public async Task<Unit> Handle(UpdateLoanRateCommand request, CancellationToken cancellationToken)
        {
            await _loanService.UpdateInterestRateAsync(request.LoanId, request.NewAnnualInterestRate);
            return Unit.Value;
        }
    }
}
