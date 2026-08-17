using ArtemisBankingPro.Application.DTOs.Cashier;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Queries
{
    public class GetHomeIndicatorsQueryHandler(ICashierService cashierService) : IRequestHandler<GetHomeIndicatorsQuery, CajeroHomeIndicatorsResponseDto>
    {
        public async Task<CajeroHomeIndicatorsResponseDto> Handle(GetHomeIndicatorsQuery request, CancellationToken cancellationToken)
        {
            return await cashierService.GetHomeIndicatorsAsync(request.CashierId);
        }
    }
}
