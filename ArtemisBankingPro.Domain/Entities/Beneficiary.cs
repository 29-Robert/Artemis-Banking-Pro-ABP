using ArtemisBankingPro.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Domain.Entities
{
    public class Beneficiary : BaseEntity
    {
        public int ClientId { get; set; }
        public string Alias { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
    }
}
