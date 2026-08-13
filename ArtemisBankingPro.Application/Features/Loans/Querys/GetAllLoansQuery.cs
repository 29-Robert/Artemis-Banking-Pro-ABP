using ArtemisBankingPro.Application.DTOs.Loan;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Loans.Querys
{
    public class GetAllLoansQuery : IRequest<List<LoanResponseDto>> { }
}
