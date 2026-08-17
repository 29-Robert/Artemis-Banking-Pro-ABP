using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Commerces.Commands
{
    public class UpdateCommerceCommandHandler(ICommerceService commerceService) : IRequestHandler<UpdateCommerceCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateCommerceCommand request, CancellationToken cancellationToken)
        {
            var dto = new UpdateCommerceDto
            {
                BusinessName = request.BusinessName,
                RNC = request.RNC,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address
            };

            await commerceService.UpdateCommerceAsync(request.Id, dto);
            return Unit.Value;
        }
    }
}
