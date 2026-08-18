using ArtemisBankingPro.Application.DTOs.Home;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Queries
{
    public class GetClientHomeDataQuery : IRequest<ClientHomeDto>
    {
        public string ClientId { get; set; } = string.Empty;
    }
}
