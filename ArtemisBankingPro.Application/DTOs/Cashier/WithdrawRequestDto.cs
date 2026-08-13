using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Cashier
{
    public class WithdrawRequestDto
    {
        public string SourceAccountNumber { get; set; }
        public decimal Amount { get; set; }
    }
}
