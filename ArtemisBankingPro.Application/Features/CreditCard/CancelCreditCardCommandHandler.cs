using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.CreditCard
{
    public class CancelCreditCardCommandHandler : IRequestHandler<CancelCreditCardCommand, Unit>
    {
        private readonly ICreditCardService _creditCardService;
        public CancelCreditCardCommandHandler(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }
        public async Task<Unit> Handle(CancelCreditCardCommand request, CancellationToken cancellationToken)
        {
            await _creditCardService.CancelCreditCardAsync(request.CardId);
            return Unit.Value;
        }
    }
}
