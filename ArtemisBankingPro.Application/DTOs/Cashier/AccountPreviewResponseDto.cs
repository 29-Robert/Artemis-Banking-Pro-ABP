using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Cashier
{
    public class AccountPreviewResponseDto
    {
        public string AccountNumber { get; set; }
        public string AccountHolderFullName { get; set; }
        public string Status { get; set; }
    }
}
