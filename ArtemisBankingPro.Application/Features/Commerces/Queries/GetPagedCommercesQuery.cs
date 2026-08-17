using ArtemisBankingPro.Application.DTOs.Commerces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Commerces.Queries
{
    public class GetPagedCommercesQuery : IRequest<PagedCommerceResponseDto>
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }
}
