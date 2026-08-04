using System.Collections.Generic;
using ArtemisBankingPro.Application.DTOs.Account; 

namespace ArtemisBankingPro.Application.DTOs.Home
{
    public class ClientHomeDto
    {
        public List<SavingsAccountListItemDto> Accounts { get; set; } = new List<SavingsAccountListItemDto>();
        public List<LoanListItemDto> Loans { get; set; } = new List<LoanListItemDto>();
        public List<CreditCardListItemDto> CreditCards { get; set; } = new List<CreditCardListItemDto>();
    }
}