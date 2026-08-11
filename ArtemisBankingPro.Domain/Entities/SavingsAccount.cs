using ArtemisBankingPro.Domain.Common;
using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Domain.Entities
{
    public class SavingsAccount : BaseEntity
    {
        public string AccountNumber { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public decimal Balance { get; set; }
        public AccountStatus Status { get; set; }
        public bool IsPrincipal { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
    }
}