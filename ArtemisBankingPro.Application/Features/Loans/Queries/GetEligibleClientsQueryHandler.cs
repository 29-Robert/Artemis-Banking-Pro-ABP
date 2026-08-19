using ArtemisBankingPro.Application.DTOs; 
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Loans.Queries
{
    public class GetEligibleClientsQueryHandler : IRequestHandler<GetEligibleClientsQuery, PagedUserResponseDto>
    {
        private readonly IGenericRepository<User> _userRepository;

        public GetEligibleClientsQueryHandler(IGenericRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<PagedUserResponseDto> Handle(GetEligibleClientsQuery request, CancellationToken cancellationToken)
        {
           
            var query = await _userRepository.GetAllAsync();

           
            if (!string.IsNullOrWhiteSpace(request.Cedula))
            {
                query = query.Where(u => u.Cedula != null && u.Cedula.Contains(request.Cedula)).ToList();
            }

            
            int totalRecords = query.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Cedula = u.Cedula,
                   
                })
                .ToList();

            
            return new PagedUserResponseDto
            {
                Page = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                Data = items
            };
        }
    }
}