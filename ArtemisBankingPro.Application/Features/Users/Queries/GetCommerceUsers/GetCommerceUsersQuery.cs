using ArtemisBankingPro.Application.DTOs;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using System.Linq;

namespace ArtemisBankingPro.Application.Features.Users.Queries.GetCommerceUsers
{
    public class GetCommerceUsersQuery : IRequest<PagedUserResponseDto>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class GetCommerceUsersQueryHandler(
        IGenericRepository<User> userRepository,
        IMapper mapper) : IRequestHandler<GetCommerceUsersQuery, PagedUserResponseDto>
    {
        public async Task<PagedUserResponseDto> Handle(GetCommerceUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await userRepository.GetAllAsync();

            // Filtrar solo usuarios con rol Comercio (RoleId = 4)
            var query = users.Where(u => u.RoleId == 4).AsQueryable();

            // Ordenar de más reciente a más antiguo
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
