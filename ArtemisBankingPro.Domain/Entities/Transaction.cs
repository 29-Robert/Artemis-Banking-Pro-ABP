using ArtemisBankingPro.Domain.Common;
using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public string AccountNumber { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string RelatedEntity { get; set; }
        public int? PerformedByUserId { get; set; }
        public TransactionStatus Status { get; set; }

        public SavingsAccount SavingsAccount { get; set; }
    }
}