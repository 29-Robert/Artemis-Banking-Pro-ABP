using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.CreditCard.Commands
{
    public class UpdateCreditLimitCommandHandler : IRequestHandler<UpdateCreditLimitCommand, Unit>
    {
        private readonly ICreditCardService _creditCardService;
        public UpdateCreditLimitCommandHandler(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }
        public async Task<Unit> Handle(UpdateCreditLimitCommand request, CancellationToken cancellationToken)
        {
            await _creditCardService.UpdateCreditLimitAsync(request.CardId, request.NewCreditLimit);
            return Unit.Value;
        }
    }
}
