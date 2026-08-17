using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Queries
{
    public class GetPrincipalAccountByClientIdQueryHandler(ISavingsAccountRepository accountRepository) : IRequestHandler<GetPrincipalAccountByClientIdQuery, SavingsAccount?>
    {
        public async Task<SavingsAccount?> Handle(GetPrincipalAccountByClientIdQuery request, CancellationToken cancellationToken)
        {
            return await accountRepository.GetPrincipalByClientAsync(request.ClientId);
        }
    }
}
