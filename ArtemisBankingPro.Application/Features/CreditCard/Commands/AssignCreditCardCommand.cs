using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtemisBankingPro.Application.DTOs.CreditCard;
using MediatR;

namespace ArtemisBankingPro.Application.Features.CreditCard.Commands
{
    public class AssignCreditCardCommand : IRequest<CreditCardCreatedResponseDto>
    {
        public string ClientId { get; set; }
        public decimal CreditLimit { get; set; }
        public int AdminId { get; set; } 
    }
}