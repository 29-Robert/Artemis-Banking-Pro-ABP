using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArtemisBankingPro.Application.DTOs;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Loans.Queries
{
    public class GetEligibleClientsQuery : IRequest<PagedUserResponseDto>
    {
        public string? Cedula { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
