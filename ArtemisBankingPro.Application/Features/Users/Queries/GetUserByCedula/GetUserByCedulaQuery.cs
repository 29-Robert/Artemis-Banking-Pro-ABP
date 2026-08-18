using ArtemisBankingPro.Domain.Entities;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Queries
{
    public class GetUserByCedulaQuery : IRequest<User?>
    {
        public string Cedula { get; set; } = string.Empty;
    }
}
