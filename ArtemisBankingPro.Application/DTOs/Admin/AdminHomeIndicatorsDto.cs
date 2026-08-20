using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Admin
{
    public class AdminHomeIndicatorsDto
    {
        public int TotalTransactionsHistorical { get; set; }
        public int TotalTransactionsToday { get; set; }
        public int TotalPaymentsHistorical { get; set; }
        public int TotalPaymentsToday { get; set; }
        public int ActiveClients { get; set; }
        public int InactiveClients { get; set; }
        public int ActiveFinancialProducts { get; set; }
        public decimal AverageDebtPerClient { get; set; }
    }
}
