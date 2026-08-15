using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.CreditCard
{
    public class CreditCardResponseDto
    {
        public int Id { get; set; }
        public string MaskedCardNumber { get; set; } 
        public string LastFourDigits { get; set; }
        public string ClientId { get; set; }
        public string ClientFullName { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal AvailableCredit { get; set; }
        public decimal CurrentDebt { get; set; }
        public string ExpirationDate { get; set; } 
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool EmailNotificationFailed { get; set; }


        public List<CreditCardConsumptionDto> Consumptions { get; set; }
    }
}
