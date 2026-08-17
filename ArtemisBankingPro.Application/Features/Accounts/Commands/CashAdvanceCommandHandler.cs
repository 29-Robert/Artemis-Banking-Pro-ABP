using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class CashAdvanceCommandHandler(ISavingsAccountService accountService) : IRequestHandler<CashAdvanceCommand, Unit>
    {
        public async Task<Unit> Handle(CashAdvanceCommand request, CancellationToken cancellationToken)
        {
            await accountService.ProcessCashAdvanceAsync(request.SourceAccountNumber, request.CardNumber, request.Amount, request.UserId);
            return Unit.Value;
        }
    }
}
