using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Cashier
{
    public class CajeroHomeIndicatorsResponseDto
    {
        public int DepositsToday { get; set; }
        public int WithdrawalsToday { get; set; }
        public int TotalTransactionsToday { get; set; }
        public int PaymentsToday { get; set; }
    }
}
