using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.Loan;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Loans.Queries
{
    public class GetLoansQuery : IRequest<PagedResult<LoanResponseDto>>
    {
        public string? Cedula { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
