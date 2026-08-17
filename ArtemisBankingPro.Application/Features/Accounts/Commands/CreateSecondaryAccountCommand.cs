using ArtemisBankingPro.Application.DTOs.Account;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class CreateSecondaryAccountCommand : IRequest<SavingsAccountDetailDto>
    {
        public string ClientCedula { get; set; } = string.Empty;
        public decimal InitialBalance { get; set; }
    }
}
