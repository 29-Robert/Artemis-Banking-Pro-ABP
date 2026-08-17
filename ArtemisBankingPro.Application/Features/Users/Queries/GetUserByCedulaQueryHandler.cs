using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Users.Queries
{
    public class GetUserByCedulaQueryHandler(IUserRepository userRepository) : IRequestHandler<GetUserByCedulaQuery, User?>
    {
        public async Task<User?> Handle(GetUserByCedulaQuery request, CancellationToken cancellationToken)
        {
            return await userRepository.GetByCedulaAsync(request.Cedula);
        }
    }
}
