using ArtemisBankingPro.Application.DTOs.Commerces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Commerces.Queries
{
    public class GetCommerceByIdQuery : IRequest<CommerceDetailDto?>
    {
        public int Id { get; set; }
    }
}
