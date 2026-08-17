using ArtemisBankingPro.Application.DTOs.Home;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Users.Queries
{
    public class GetClientHomeDataQueryHandler(ISavingsAccountService accountService) : IRequestHandler<GetClientHomeDataQuery, ClientHomeDto>
    {
        public async Task<ClientHomeDto> Handle(GetClientHomeDataQuery request, CancellationToken cancellationToken)
        {
            return await accountService.GetClientHomeDataAsync(request.ClientId);
        }
    }
}
