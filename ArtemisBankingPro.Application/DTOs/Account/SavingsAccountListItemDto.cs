using ArtemisBankingPro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.Account
{
    public class SavingsAccountListItemDto
    {
        public string AccountNumber { get; set; }
        public string ClientFullName { get; set; }
        public AccountType Type { get; set; }
        public decimal Balance { get; set; }
        public AccountStatus Status { get; set; }
    }
}
