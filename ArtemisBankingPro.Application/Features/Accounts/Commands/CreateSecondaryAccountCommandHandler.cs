using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class CreateSecondaryAccountCommandHandler(ISavingsAccountService accountService) : IRequestHandler<CreateSecondaryAccountCommand, SavingsAccountDetailDto>
    {
        public async Task<SavingsAccountDetailDto> Handle(CreateSecondaryAccountCommand request, CancellationToken cancellationToken)
        {
            var dto = new CreateSecondaryAccountDto
            {
                ClientCedula = request.ClientCedula,
                InitialBalance = request.InitialBalance
            };
            return await accountService.CreateSecondaryAccountAsync(dto);
        }
    }
}
