using ArtemisBankingPro.Application.DTOs.CreditCard;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.CreditCard.Querys
{
    public class GetAllCreditCardsQuery : IRequest<List<CreditCardResponseDto>> { }
}
