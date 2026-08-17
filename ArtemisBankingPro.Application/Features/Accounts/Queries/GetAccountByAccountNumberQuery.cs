using ArtemisBankingPro.Domain.Entities;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Accounts.Queries
{
    public class GetAccountByAccountNumberQuery : IRequest<SavingsAccount?>
    {
        public string AccountNumber { get; set; } = string.Empty;
    }
}
