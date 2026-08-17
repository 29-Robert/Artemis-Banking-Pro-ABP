using ArtemisBankingPro.Application.DTOs.HermesPay;
using MediatR;

namespace ArtemisBankingPro.Application.Features.HermesPay.Commands
{
    public class ProcessPaymentCommand : IRequest<TransactionResponseDto>
    {
        public int CommerceId { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string ExpirationMonth { get; set; } = string.Empty;
        public string ExpirationYear { get; set; } = string.Empty;
        public string Cvc { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
