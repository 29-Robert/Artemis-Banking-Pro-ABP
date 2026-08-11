using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Cashier
{
    public class PayCreditCardRequestDto
    {
        public string SourceAccountNumber { get; set; }
        public string CardNumber { get; set; }
        public decimal Amount { get; set; } 
    }
}
