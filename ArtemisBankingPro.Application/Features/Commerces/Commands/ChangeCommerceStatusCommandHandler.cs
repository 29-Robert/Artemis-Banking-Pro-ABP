using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Commerces.Commands
{
    public class ChangeCommerceStatusCommandHandler(ICommerceService commerceService) : IRequestHandler<ChangeCommerceStatusCommand, Unit>
    {
        public async Task<Unit> Handle(ChangeCommerceStatusCommand request, CancellationToken cancellationToken)
        {
            await commerceService.ChangeStatusAsync(request.Id, request.IsActive);
            return Unit.Value;
        }
    }
}
