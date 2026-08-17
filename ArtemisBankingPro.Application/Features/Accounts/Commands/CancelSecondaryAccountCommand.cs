using MediatR;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class CancelSecondaryAccountCommand : IRequest<Unit>
    {
        public string AccountNumber { get; set; } = string.Empty;
    }
}
