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
    public class GetEligibleCreditCardClientsQueryHandler : IRequestHandler<GetEligibleCreditCardClientsQuery, EligibleClientsResponseDto>
    {
        private readonly ICreditCardService _creditCardService;

        public GetEligibleCreditCardClientsQueryHandler(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        public async Task<EligibleClientsResponseDto> Handle(GetEligibleCreditCardClientsQuery request, CancellationToken cancellationToken)
        {
            return await _creditCardService.GetEligibleClientsAsync(request.Cedula, request.PageNumber, request.PageSize);
        }
    }
}
