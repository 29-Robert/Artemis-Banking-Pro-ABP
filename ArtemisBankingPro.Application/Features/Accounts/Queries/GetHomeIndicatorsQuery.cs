using ArtemisBankingPro.Application.DTOs.Cashier;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Accounts.Queries
{
    public class GetHomeIndicatorsQuery : IRequest<CajeroHomeIndicatorsResponseDto>
    {
        public int CashierId { get; set; }
    }
}
