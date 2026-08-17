using ArtemisBankingPro.Domain.Entities;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Accounts.Queries
{
    public class GetPrincipalAccountByClientIdQuery : IRequest<SavingsAccount?>
    {
        public int ClientId { get; set; }
    }
}
