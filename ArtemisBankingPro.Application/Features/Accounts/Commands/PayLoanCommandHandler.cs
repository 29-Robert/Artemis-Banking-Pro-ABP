using ArtemisBankingPro.Application.DTOs.Cashier;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class PayLoanCommandHandler(ICashierService cashierService) : IRequestHandler<PayLoanCommand, TransactionResponseDto>
    {
        public async Task<TransactionResponseDto> Handle(PayLoanCommand request, CancellationToken cancellationToken)
        {
            var dto = new PayLoanRequestDto
            {
                SourceAccountNumber = request.SourceAccountNumber,
                LoanNumber = request.LoanNumber,
                Amount = request.Amount
            };
            return await cashierService.ProcessLoanPaymentAsync(dto, request.CashierId);
        }
    }
}
