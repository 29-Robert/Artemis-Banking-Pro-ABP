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
    }
}
