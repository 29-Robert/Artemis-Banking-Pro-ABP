using ArtemisBankingPro.Application.DTOs.Cashier;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class WithdrawCommand : IRequest<TransactionResponseDto>
    {
        public string SourceAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int CashierId { get; set; }
    }
}
