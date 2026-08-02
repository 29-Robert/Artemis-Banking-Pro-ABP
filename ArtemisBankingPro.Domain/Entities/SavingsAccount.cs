using ArtemisBankingPro.Domain.Common;
using ArtemisBankingPro.Domain.Enums;
using System.Transactions;

namespace ArtemisBankingPro.Domain.Entities
{
    public class SavingsAccount : BaseEntity
    {
        public string AccountNumber { get; set; }
        public int ClientId { get; set; }
        public AccountType Type { get; set; }
        public decimal Balance { get; set; }
        public AccountStatus Status { get; set; }

        public User Client { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}