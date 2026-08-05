using ArtemisBankingPro.Application.DTOs;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<List<UserDto>>
    {
    }

    public class GetAllUsersQueryHandler(
        IGenericRepository<User> userRepository,
        IMapper mapper) : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await userRepository.GetAllAsync();
            return mapper.Map<List<UserDto>>(users);
        }
    }
}