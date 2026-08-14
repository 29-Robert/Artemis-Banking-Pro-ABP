using ArtemisBankingPro.Application.DTOs.Cashier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ArtemisBankingPro.Application.Interfaces.Services
    {
        public interface ICashierService
        {
            Task<TransactionResponseDto> ProcessDepositAsync(DepositRequestDto request, int cashierId);

            Task<TransactionResponseDto> ProcessWithdrawalAsync(WithdrawRequestDto request, int cashierId);

            Task<TransactionResponseDto> ProcessCreditCardPaymentAsync(PayCreditCardRequestDto request, int cashierId);

            Task<TransactionResponseDto> ProcessLoanPaymentAsync(PayLoanRequestDto request, int cashierId);

            Task<TransactionResponseDto> ProcessThirdPartyTransferAsync(ThirdPartyTransferRequestDto request, int cashierId);

            Task<CajeroHomeIndicatorsResponseDto> GetHomeIndicatorsAsync(int cashierId);

            Task<AccountPreviewResponseDto> GetAccountPreviewAsync(string accountNumber);

            Task<CreditCardPreviewResponseDto> GetCreditCardPreviewAsync(string cardNumber);

            Task<LoanPreviewResponseDto> GetLoanPreviewAsync(string loanNumber);
        }
    

}

