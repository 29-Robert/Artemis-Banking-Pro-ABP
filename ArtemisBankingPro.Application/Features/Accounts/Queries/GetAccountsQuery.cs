using ArtemisBankingPro.Domain.Enums;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Accounts.Queries
{
    public class GetAccountsQuery : IRequest<object>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public AccountStatus? Status { get; set; }
        public AccountType? Type { get; set; }
        public string Cedula { get; set; } = string.Empty;
    }
}
