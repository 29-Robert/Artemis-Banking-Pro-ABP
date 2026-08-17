using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class CancelSecondaryAccountCommandHandler(ISavingsAccountService accountService) : IRequestHandler<CancelSecondaryAccountCommand, Unit>
    {
        public async Task<Unit> Handle(CancelSecondaryAccountCommand request, CancellationToken cancellationToken)
        {
            await accountService.CancelSecondaryAccountAsync(request.AccountNumber);
            return Unit.Value;
        }
    }
}
