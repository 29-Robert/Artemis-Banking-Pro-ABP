using ArtemisBankingPro.Application.DTOs.Cashier;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class DepositCommand : IRequest<TransactionResponseDto>
    {
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int CashierId { get; set; }
    }
}
