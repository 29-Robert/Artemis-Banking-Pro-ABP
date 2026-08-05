using ArtemisBankingPro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Account
{
    public class SavingsAccountDetailDto
    {
        public string AccountNumber { get; set; }
        public AccountType Type { get; set; }
        public decimal Balance { get; set; }
        public AccountStatus Status { get; set; }
        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
    }
}
