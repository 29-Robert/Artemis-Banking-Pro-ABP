using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Loans.Commands
{
    public class UpdateLoanRateCommand : IRequest<Unit>
    {
        public int LoanId { get; set; }
        public decimal NewAnnualInterestRate { get; set; }
    }
}
