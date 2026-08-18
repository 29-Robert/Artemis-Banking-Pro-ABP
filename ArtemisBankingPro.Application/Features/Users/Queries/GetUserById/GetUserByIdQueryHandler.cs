using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Users.Queries
{
    public class GetUserByIdQueryHandler(IGenericRepository<User> userRepository) : IRequestHandler<GetUserByIdQuery, User?>
    {
        public async Task<User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await userRepository.GetByIdAsync(request.Id);
        }
    }
}
