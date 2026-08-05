using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Payments
{
    public class ProcessPaymentDto
    {
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string Cvc { get; set; } 
        public decimal Amount { get; set; }
    }
}
