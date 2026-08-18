using ArtemisBankingPro.Domain.Entities;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Queries
{
    public class GetUserByIdQuery : IRequest<User?>
    {
        public int Id { get; set; }
    }
}
