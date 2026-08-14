using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.CreditCardQuerys
{
    public class GetAllCreditCardsQueryHandler : IRequestHandler<GetAllCreditCardsQuery, List<CreditCardResponseDto>>
    {
        private readonly ICreditCardService _creditCardService;
        public GetAllCreditCardsQueryHandler(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }
        public async Task<List<CreditCardResponseDto>> Handle(GetAllCreditCardsQuery request, CancellationToken cancellationToken)
        {
            return await _creditCardService.GetAllCreditCardsAsync();
        }
    }
}
