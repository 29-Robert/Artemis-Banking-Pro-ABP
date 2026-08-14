using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtemisBankingPro.Application.DTOs.CreditCard;
using MediatR;

namespace ArtemisBankingPro.Application.Command
{
   
    public class AssignCreditCardCommand : IRequest<CreditCardResponseDto>
    {
        public string ClientId { get; set; }
        public decimal CreditLimit { get; set; }
        public string AdminId { get; set; } // Obtenido del JWT
    }
}
