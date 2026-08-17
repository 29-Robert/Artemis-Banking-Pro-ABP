using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.DTOs.Cashier;
using ArtemisBankingPro.Application.DTOs.Transactions;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class TransferCommandHandler(
        ITransactionService transactionService,
        ICashierService cashierService,
        IBeneficiaryService beneficiaryService) : IRequestHandler<TransferCommand, TransactionResponseDto>
    {
        public async Task<TransactionResponseDto> Handle(TransferCommand request, CancellationToken cancellationToken)
        {
            if (request.IsOwnAccount)
            {
                if (string.IsNullOrEmpty(request.ClientId))
                    throw new ArgumentException("El ID del cliente es requerido para transferencias entre cuentas propias.");

                var dto = new OwnAccountTransferDto
                {
                    SourceAccountNumber = request.SourceAccountNumber,
                    DestinationAccountNumber = request.DestinationAccountNumber,
                    Amount = request.Amount
                };

                var ownResult = await transactionService.OwnAccountTransferAsync(dto, request.ClientId);
                return new TransactionResponseDto
                {
                    Approved = ownResult.IsSuccess,
                    AppliedAmount = ownResult.IsSuccess ? request.Amount : 0,
                    RejectionReason = ownResult.IsSuccess ? null : ownResult.Message,
                    DateTime = DateTime.UtcNow
                };
            }

            if (request.IsThirdParty && request.CashierId.HasValue)
            {
                var dto = new ThirdPartyTransferRequestDto
                {
                    SourceAccountNumber = request.SourceAccountNumber,
                    DestinationAccountNumber = request.DestinationAccountNumber,
                    Amount = request.Amount
                };
                return await cashierService.ProcessThirdPartyTransferAsync(dto, request.CashierId.Value);
            }

            var expressDto = new ExpressTransactionDto
            {
                SourceAccountNumber = request.SourceAccountNumber,
                DestinationAccountNumber = request.DestinationAccountNumber,
                Amount = request.Amount
            };

            if (request.IsBeneficiary)
            {
                await beneficiaryService.TransferToBeneficiaryAsync(expressDto);
            }
            else
            {
                await transactionService.ExpressTransactionAsync(expressDto);
            }

            return new TransactionResponseDto
            {
                Approved = true,
                AppliedAmount = request.Amount,
                DateTime = DateTime.UtcNow
            };
        }
    }
}
