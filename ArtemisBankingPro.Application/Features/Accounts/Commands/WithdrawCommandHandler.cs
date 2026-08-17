using ArtemisBankingPro.Application.DTOs.Cashier;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class WithdrawCommandHandler(ICashierService cashierService) : IRequestHandler<WithdrawCommand, TransactionResponseDto>
    {
        public async Task<TransactionResponseDto> Handle(WithdrawCommand request, CancellationToken cancellationToken)
        {
            var dto = new WithdrawRequestDto
            {
                SourceAccountNumber = request.SourceAccountNumber,
                Amount = request.Amount
            };
            return await cashierService.ProcessWithdrawalAsync(dto, request.CashierId);
        }
    }
}
