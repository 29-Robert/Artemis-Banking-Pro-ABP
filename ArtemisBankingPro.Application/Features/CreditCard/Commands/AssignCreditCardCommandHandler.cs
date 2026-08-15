using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;

namespace ArtemisBankingPro.Application.Features.CreditCard.Commands
{
    public class AssignCreditCardCommandHandler : IRequestHandler<AssignCreditCardCommand, CreditCardCreatedResponseDto>
    {
        private readonly ICreditCardService _creditCardService;

        public AssignCreditCardCommandHandler(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        public async Task<CreditCardCreatedResponseDto> Handle(AssignCreditCardCommand request, CancellationToken cancellationToken)
        {
            var createRequest = new CreateCreditCardRequestDto
            {
                ClientId = request.ClientId,
                CreditLimit = request.CreditLimit
            };

            return await _creditCardService.AssignCreditCardAsync(createRequest, request.AdminId.ToString());
        }
    }
}