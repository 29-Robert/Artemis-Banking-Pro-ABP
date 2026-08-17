using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Queries
{
    public class GetAccountByAccountNumberQueryHandler(ISavingsAccountRepository accountRepository) : IRequestHandler<GetAccountByAccountNumberQuery, SavingsAccount?>
    {
        public async Task<SavingsAccount?> Handle(GetAccountByAccountNumberQuery request, CancellationToken cancellationToken)
        {
            return await accountRepository.GetByAccountNumberAsync(request.AccountNumber);
        }
    }
}
