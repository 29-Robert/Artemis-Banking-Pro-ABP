using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Commerces
{
    public class CommerceTransactionDto
    {
        public DateTime Date { get; set; }
        public string CardMasked { get; set; } 
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}
