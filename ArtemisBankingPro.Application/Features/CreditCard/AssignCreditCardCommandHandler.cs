using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;

namespace ArtemisBankingPro.Application.Command
{
   

    public class AssignCreditCardCommandHandler : IRequestHandler<AssignCreditCardCommand, CreditCardResponseDto>
    {
        private readonly ICreditCardService _creditCardService;

        public AssignCreditCardCommandHandler(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        public async Task<CreditCardResponseDto> Handle(AssignCreditCardCommand request, CancellationToken cancellationToken)
        {

            // El Handler solo delega la operación al Servicio de Negocio
            var createRequest = new CreateCreditCardRequestDto
            {
                ClientId = request.ClientId,
                CreditLimit = request.CreditLimit
            };

            return await _creditCardService.AssignCreditCardAsync(createRequest, request.AdminId);
        }
    }
}
