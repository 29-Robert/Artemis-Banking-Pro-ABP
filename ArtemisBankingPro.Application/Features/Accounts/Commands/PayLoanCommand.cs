using ArtemisBankingPro.Application.DTOs.Cashier;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class PayLoanCommand : IRequest<TransactionResponseDto>
    {
        public string SourceAccountNumber { get; set; } = string.Empty;
        public string LoanNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int CashierId { get; set; }
    }
}
