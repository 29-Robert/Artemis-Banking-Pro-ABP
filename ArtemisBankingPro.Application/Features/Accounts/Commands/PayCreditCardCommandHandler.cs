using ArtemisBankingPro.Application.DTOs.Cashier;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class PayCreditCardCommandHandler(ICashierService cashierService) : IRequestHandler<PayCreditCardCommand, TransactionResponseDto>
    {
        public async Task<TransactionResponseDto> Handle(PayCreditCardCommand request, CancellationToken cancellationToken)
        {
            var dto = new PayCreditCardRequestDto
            {
                SourceAccountNumber = request.SourceAccountNumber,
                CardNumber = request.CardNumber,
                Amount = request.Amount
            };
            return await cashierService.ProcessCreditCardPaymentAsync(dto, request.CashierId);
        }
    }
}
