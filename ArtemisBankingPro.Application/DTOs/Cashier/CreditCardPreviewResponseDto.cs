using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Cashier
{
    public class CreditCardPreviewResponseDto
    {
        public string MaskedCardNumber { get; set; }
        public string ClientFullName { get; set; }
        public decimal CurrentDebt { get; set; }
        public string Status { get; set; } 
    }
}
