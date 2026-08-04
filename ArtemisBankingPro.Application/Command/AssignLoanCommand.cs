using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtemisBankingPro.Application.DTOs.Loan;
using MediatR;


namespace ArtemisBankingPro.Application.Features.Loans.Commands
{
    public class AssignLoanCommand : IRequest<LoanResponseDto>
    {
        public string ClientId { get; set; }
        public decimal CapitalAmount { get; set; }
        public int TermInMonths { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public bool ConfirmHighRisk { get; set; }
        public string AdminId { get; set; } 
    }
}