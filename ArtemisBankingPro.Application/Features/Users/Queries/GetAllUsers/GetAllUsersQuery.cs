using ArtemisBankingPro.Application.DTOs;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using System.Linq;

namespace ArtemisBankingPro.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<PagedUserResponseDto>
    {
        public string? RoleFilter { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class GetAllUsersQueryHandler(
        IGenericRepository<User> userRepository,
        IMapper mapper) : IRequestHandler<GetAllUsersQuery, PagedUserResponseDto>
    {
        public async Task<PagedUserResponseDto> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await userRepository.GetAllAsync();

            var query = users.Where(u => u.RoleId != (int)ArtemisBankingPro.Domain.Enums.Roles.Comercio).AsQueryable();

            if (!string.IsNullOrEmpty(request.RoleFilter))
            {
                var roleId = request.RoleFilter switch
                {
                    "Administrador" => 1,
                    "Cajero" => 2,
                    "Cliente" => 3,
                    _ => 0
                };
                if (roleId > 0)
                {
                    query = query.Where(u => u.RoleId == roleId);
                }
            }

            query = query.OrderByDescending(u => u.CreatedAt);

            var totalRecords = query.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);
            if (totalPages == 0) totalPages = 1;

            var pagedUsers = query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            return new PagedUserResponseDto
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                Data = mapper.Map<List<UserDto>>(pagedUsers)
            };
        }
    }
}
