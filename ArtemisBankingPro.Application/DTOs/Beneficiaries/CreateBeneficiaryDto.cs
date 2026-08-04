using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Beneficiaries
{
    public class CreateBeneficiaryDto
    {
        public string AccountNumber { get; set; }
        public string Alias { get; set; }
    }
}
