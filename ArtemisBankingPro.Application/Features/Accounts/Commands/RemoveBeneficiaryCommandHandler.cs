using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class RemoveBeneficiaryCommandHandler(IBeneficiaryService beneficiaryService) : IRequestHandler<RemoveBeneficiaryCommand, Unit>
    {
        public async Task<Unit> Handle(RemoveBeneficiaryCommand request, CancellationToken cancellationToken)
        {
            await beneficiaryService.RemoveBeneficiaryAsync(request.Id);
            return Unit.Value;
        }
    }
}
