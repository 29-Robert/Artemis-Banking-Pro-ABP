using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;



namespace ArtemisBankingPro.Application.Features.Loans.Commands
{
    public class AssignLoanCommandHandler : IRequestHandler<AssignLoanCommand, LoanResponseDto>
    {
        private readonly ILoanService _loanService;

        public AssignLoanCommandHandler(ILoanService loanService)
        {
            _loanService = loanService;
        }

        public async Task<LoanResponseDto> Handle(AssignLoanCommand request, CancellationToken cancellationToken)
        {
            var createDto = new CreateLoanRequestDto
            {
                ClientId = request.ClientId,
                CapitalAmount = request.CapitalAmount,
                TermInMonths = request.TermInMonths,
                AnnualInterestRate = request.AnnualInterestRate,
                ConfirmHighRisk = request.ConfirmHighRisk
            };

            return await _loanService.AssignLoanAsync(createDto,int.Parse(request.AdminId));
        }
    }
}
