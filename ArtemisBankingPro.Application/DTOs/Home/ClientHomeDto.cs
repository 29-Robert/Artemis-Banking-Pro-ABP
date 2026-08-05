using System.Collections.Generic;
using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.DTOs.CreditCard; 
using ArtemisBankingPro.Application.DTOs.Loan;       

namespace ArtemisBankingPro.Application.DTOs.Home
{
    public class ClientHomeDto
    {
        public List<SavingsAccountListItemDto> Accounts { get; set; } = new List<SavingsAccountListItemDto>();

        public List<LoanResponseDto> Loans { get; set; } = new List<LoanResponseDto>();

        public List<CreditCardResponseDto> CreditCards { get; set; } = new List<CreditCardResponseDto>();
    }
}