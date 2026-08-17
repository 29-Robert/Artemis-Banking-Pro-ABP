using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class PayCreditCardOwnAccountCommandHandler(ISavingsAccountService accountService) : IRequestHandler<PayCreditCardOwnAccountCommand, Unit>
    {
        public async Task<Unit> Handle(PayCreditCardOwnAccountCommand request, CancellationToken cancellationToken)
        {
            await accountService.ProcessCreditCardPaymentOwnAccountAsync(request.SourceAccountNumber, request.CardNumber, request.Amount, request.UserId);
            return Unit.Value;
        }
    }
}
