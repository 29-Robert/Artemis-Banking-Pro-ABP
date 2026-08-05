using ArtemisBankingPro.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Domain.Entities
{
    public class CreditCard : BaseEntity
    {
        public string ClientId { get; set; } 
        public string CardNumber { get; set; } 
        public decimal CreditLimit { get; set; }
        public decimal CurrentDebt { get; set; } 
        public string ExpirationMonth { get; set; } 
        public string ExpirationYear { get; set; } 
        public string CvcHash { get; set; } 
        public string Status { get; set; } 
        public string AdminId { get; set; } 

        // Navegación
        public ICollection<CreditCardConsumption> Consumptions { get; set; }
    }
}
