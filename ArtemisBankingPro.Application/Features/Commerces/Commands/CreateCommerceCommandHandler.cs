using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Commerces.Commands
{
    public class CreateCommerceCommandHandler(ICommerceService commerceService) : IRequestHandler<CreateCommerceCommand, CommerceListItemDto>
    {
        public async Task<CommerceListItemDto> Handle(CreateCommerceCommand request, CancellationToken cancellationToken)
        {
            var dto = new CreateCommerceDto
            {
                BusinessName = request.BusinessName,
                RNC = request.RNC,
                Email = request.Email,
                Password = request.Password,
                Phone = request.Phone,
                Address = request.Address
            };

            return await commerceService.CreateCommerceAsync(dto);
        }
    }
}
