using ArtemisBankingPro.Application.DTOs.Account;
using MediatR;
using System.Collections.Generic;

namespace ArtemisBankingPro.Application.Features.Accounts.Queries
{
    public class GetStatementQuery : IRequest<IEnumerable<TransactionDto>>
    {
        public string AccountNumber { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
