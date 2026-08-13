using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Cashier
{
    public class DepositRequestDto
    {
        public string DestinationAccountNumber { get; set; }
        public decimal Amount { get; set; }
    }

}
