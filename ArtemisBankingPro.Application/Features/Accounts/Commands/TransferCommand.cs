using ArtemisBankingPro.Application.DTOs.Cashier;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class TransferCommand : IRequest<TransactionResponseDto>
    {
        public string SourceAccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? ClientId { get; set; }
        public int? CashierId { get; set; }
        public bool IsOwnAccount { get; set; }
        public bool IsThirdParty { get; set; }
        public bool IsBeneficiary { get; set; }
    }
}
