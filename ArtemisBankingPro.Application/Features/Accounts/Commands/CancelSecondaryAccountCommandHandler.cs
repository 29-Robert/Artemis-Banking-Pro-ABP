using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class CancelSecondaryAccountCommandHandler(ISavingsAccountService accountService, ArtemisBankingPro.Application.Interfaces.Repositories.IUnitOfWork unitOfWork) : IRequestHandler<CancelSecondaryAccountCommand, Unit>
    {
        public async Task<Unit> Handle(CancelSecondaryAccountCommand request, CancellationToken cancellationToken)
        {
            await unitOfWork.BeginTransactionAsync();
            try
            {
                await accountService.CancelSecondaryAccountAsync(request.AccountNumber);
                await unitOfWork.CommitAsync();
                return Unit.Value;
            }
            catch
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
