using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.CreditCard
{
    public class CreditCardCreatedResponseDto
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
        public string Cvc { get; set; } 
        public bool EmailNotificationFailed { get; set; }
        public decimal CreditLimit { get; set; }
    }
}
