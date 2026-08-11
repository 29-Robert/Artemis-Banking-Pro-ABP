using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;

namespace ArtemisBankingPro.Application.Features.CreditCard.Querys
{
    public class GetAllCreditCardsQueryHandler : IRequestHandler<GetAllCreditCardsQuery, PagedResult<CreditCardResponseDto>>
    {
        private readonly ICreditCardService _creditCardService;

        public GetAllCreditCardsQueryHandler(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        public async Task<PagedResult<CreditCardResponseDto>> Handle(GetAllCreditCardsQuery request, CancellationToken cancellationToken)
        {
            return await _creditCardService.GetCreditCardsAsync(
                request.Cedula,
                request.Status,
                request.PageNumber,
                request.PageSize);
        }
    }
}