using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.DTOs.CreditCard
{
    public class CreateCreditCardRequestDto
    {
        public string ClientId { get; set; }
        public decimal CreditLimit { get; set; }
    }
   
}
