using ArtemisBankingPro.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Domain.Entities
{
    public class CreditCardConsumption : BaseEntity
    {
        public int CreditCardId { get; set; } 
        public CreditCard CreditCard { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string CommerceName { get; set; } 
        public string Status { get; set; } 
    }
}
