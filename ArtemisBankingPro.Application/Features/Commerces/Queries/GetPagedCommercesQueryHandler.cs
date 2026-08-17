using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Commerces.Queries
{
    public class GetPagedCommercesQueryHandler(ICommerceService commerceService) : IRequestHandler<GetPagedCommercesQuery, PagedCommerceResponseDto>
    {
        public async Task<PagedCommerceResponseDto> Handle(GetPagedCommercesQuery request, CancellationToken cancellationToken)
        {
            return await commerceService.GetPagedCommercesAsync(request.Page, request.Limit);
        }
    }
}
