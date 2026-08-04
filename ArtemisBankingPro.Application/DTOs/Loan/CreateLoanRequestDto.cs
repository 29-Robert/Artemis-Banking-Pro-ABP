using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArtemisBankingPro.Application.DTOs.Loan
{
    public class CreateLoanRequestDto
    {
       
        public string ClientId { get; set; }
       
        public decimal CapitalAmount { get; set; }
        public int TermInMonths { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public bool ConfirmHighRisk { get; set; }
    }

}
