using ArtemisBankingPro.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.CreditCard
{

    public class EligibleClientsResponseDto
    {
        public decimal SystemAverageDebt { get; set; }
        public PagedResult<EligibleClientDto> Clients { get; set; }
    }
}
