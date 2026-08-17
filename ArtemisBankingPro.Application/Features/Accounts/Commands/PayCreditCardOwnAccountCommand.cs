using MediatR;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class PayCreditCardOwnAccountCommand : IRequest<Unit>
    {
        public string SourceAccountNumber { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
