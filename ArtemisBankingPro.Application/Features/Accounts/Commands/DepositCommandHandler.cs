using ArtemisBankingPro.Application.DTOs.Cashier;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class DepositCommandHandler(ICashierService cashierService) : IRequestHandler<DepositCommand, TransactionResponseDto>
    {
        public async Task<TransactionResponseDto> Handle(DepositCommand request, CancellationToken cancellationToken)
        {
            var dto = new DepositRequestDto
            {
                DestinationAccountNumber = request.DestinationAccountNumber,
                Amount = request.Amount
            };
            return await cashierService.ProcessDepositAsync(dto, request.CashierId);
        }
    }
}
