using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Commerces.Queries
{
    public class GetCommerceByIdQueryHandler(ICommerceService commerceService) : IRequestHandler<GetCommerceByIdQuery, CommerceDetailDto?>
    {
        public async Task<CommerceDetailDto?> Handle(GetCommerceByIdQuery request, CancellationToken cancellationToken)
        {
            return await commerceService.GetCommerceByIdAsync(request.Id);
        }
    }
}
