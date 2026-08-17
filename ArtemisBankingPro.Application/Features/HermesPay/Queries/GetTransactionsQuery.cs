using ArtemisBankingPro.Application.DTOs.HermesPay;
using MediatR;

namespace ArtemisBankingPro.Application.Features.HermesPay.Queries
{
    public class GetTransactionsQuery : IRequest<PagedTransactionsDto>
    {
        public int CommerceId { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }
}
