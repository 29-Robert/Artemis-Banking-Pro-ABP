using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Account
{
    public class CreateSecondaryAccountDto
    {
        public string ClientCedula { get; set; } = null!;
        public decimal InitialBalance { get; set; }
        public int AdminId { get; set; }
    }
}
