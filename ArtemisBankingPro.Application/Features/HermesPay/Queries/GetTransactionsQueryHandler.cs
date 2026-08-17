using ArtemisBankingPro.Application.DTOs.HermesPay;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.HermesPay.Queries
{
    public class GetTransactionsQueryHandler(IPaymentService paymentService) : IRequestHandler<GetTransactionsQuery, PagedTransactionsDto>
    {
        public async Task<PagedTransactionsDto> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
        {
            return await paymentService.GetTransactionsAsync(request.CommerceId, request.Page, request.Limit);
        }
    }
}
