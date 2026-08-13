using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Cashier
{
    public class LoanPreviewResponseDto
    {
        public string LoanNumber { get; set; }
        public string ClientFullName { get; set; }
        public decimal RemainingBalance { get; set; }
        public string Status { get; set; } 
    }
}
