using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Cashier
{
    public class TransactionResponseDto
    {
            public int TransactionId { get; set; }
            public bool Approved { get; set; }
            public decimal AppliedAmount { get; set; }
            public string? WarningMessage { get; set; }
            public string RejectionReason { get; set; }
            public DateTime DateTime { get; set; }
    }
}

