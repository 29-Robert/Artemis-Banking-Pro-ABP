using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Cashier
{
    public class PayLoanRequestDto
    {
        public string SourceAccountNumber { get; set; }
        public string LoanNumber { get; set; }
        public decimal Amount { get; set; }
    }
}
