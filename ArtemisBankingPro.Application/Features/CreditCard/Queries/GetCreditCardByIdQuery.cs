using ArtemisBankingPro.Application.DTOs.CreditCard;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.CreditCard.Queries
{
    public class GetCreditCardByIdQuery : IRequest<CreditCardResponseDto>
    {
        public int Id { get; set; }
    }
}
