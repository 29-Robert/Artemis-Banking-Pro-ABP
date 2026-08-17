using ArtemisBankingPro.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Queries
{
    public class GetAccountsQueryHandler(ISavingsAccountRepository accountRepository) : IRequestHandler<GetAccountsQuery, object>
    {
        public async Task<object> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
        {
            return await accountRepository.GetPagedAsync(request.Page, request.PageSize, request.Status, request.Type, request.Cedula);
        }
    }
}
