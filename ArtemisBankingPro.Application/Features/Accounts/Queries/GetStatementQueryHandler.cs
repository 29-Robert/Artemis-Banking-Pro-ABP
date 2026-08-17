using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Queries
{
    public class GetStatementQueryHandler(ISavingsAccountService accountService) : IRequestHandler<GetStatementQuery, IEnumerable<TransactionDto>>
    {
        public async Task<IEnumerable<TransactionDto>> Handle(GetStatementQuery request, CancellationToken cancellationToken)
        {
            return await accountService.GetTransactionHistoryAsync(request.AccountNumber, request.Page, request.PageSize);
        }
    }
}
