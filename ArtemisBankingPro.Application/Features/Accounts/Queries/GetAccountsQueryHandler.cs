using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.DTOs.Account;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Queries
{
    public class GetAccountsQueryHandler(ISavingsAccountRepository accountRepository) : IRequestHandler<GetAccountsQuery, PagedAccountResponseDto>
    {
        public async Task<PagedAccountResponseDto> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
        {
            return await accountRepository.GetPagedAsync(request.Page, request.PageSize, request.Status, request.Type, request.Cedula);
        }
    }
}
