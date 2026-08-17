using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class PayLoanOwnAccountCommandHandler(ISavingsAccountService accountService) : IRequestHandler<PayLoanOwnAccountCommand, Unit>
    {
        public async Task<Unit> Handle(PayLoanOwnAccountCommand request, CancellationToken cancellationToken)
        {
            await accountService.ProcessLoanPaymentOwnAccountAsync(request.SourceAccountNumber, request.LoanNumber, request.Amount, request.UserId);
            return Unit.Value;
        }
    }
}
