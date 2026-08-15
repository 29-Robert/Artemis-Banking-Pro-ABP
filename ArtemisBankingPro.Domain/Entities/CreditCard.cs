using ArtemisBankingPro.Domain.Common;
using System.Collections.Generic;

namespace ArtemisBankingPro.Domain.Entities
{
    public class CreditCard : BaseEntity
    {
        public decimal AvailableCredit;
        public int UserId;
        public User? User;

        public int ClientId { get; set; }
        public User? Client { get; set; }

        public string CardNumber { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentDebt { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public string CvcHash { get; set; }
        public string Status { get; set; } 

        public int AdminId { get; set; }
        public User? Admin { get; set; }

        public ICollection<CreditCardConsumption> Consumptions { get; set; } = new List<CreditCardConsumption>();
    }
}