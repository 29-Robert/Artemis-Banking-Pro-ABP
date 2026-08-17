using ArtemisBankingPro.Application.DTOs.HermesPay;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.HermesPay.Commands
{
    public class ProcessPaymentCommandHandler(IPaymentService paymentService) : IRequestHandler<ProcessPaymentCommand, TransactionResponseDto>
    {
        public async Task<TransactionResponseDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            var dto = new ProcessPaymentRequestDto
            {
                CardNumber = request.CardNumber,
                ExpirationMonth = request.ExpirationMonth,
                ExpirationYear = request.ExpirationYear,
                Cvc = request.Cvc,
                Amount = request.Amount,
                Description = request.Description
            };

            return await paymentService.ProcessPaymentAsync(request.CommerceId, dto, request.UserId);
        }
    }
}
