using ArtemisBankingPro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Account
{
    public class TransactionDto
    {
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public TransactionStatus Status { get; set; }
        public string Beneficiary { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
    }
}
