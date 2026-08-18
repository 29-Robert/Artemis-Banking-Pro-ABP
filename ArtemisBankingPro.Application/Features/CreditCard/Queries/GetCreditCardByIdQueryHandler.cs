using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.CreditCard.Queries
{
    public class GetCreditCardByIdQueryHandler : IRequestHandler<GetCreditCardByIdQuery, CreditCardResponseDto>
    {
        private readonly ICreditCardService _creditCardService;
        public GetCreditCardByIdQueryHandler(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }
        public async Task<CreditCardResponseDto> Handle(GetCreditCardByIdQuery request, CancellationToken cancellationToken)
        {
            return await _creditCardService.GetCreditCardByIdAsync(request.Id);
        }
    }
}
