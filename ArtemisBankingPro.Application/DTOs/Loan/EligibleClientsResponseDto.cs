using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Loan
{
    public class EligibleClientsResponseDto
    {
        public decimal SystemAverageDebt { get; set; }
        public PagedResult<EligibleClientDto> Clients { get; set; }
    }
}