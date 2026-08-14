using ArtemisBankingPro.Application.DTOs.CreditCard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtemisBankingPro.Application.Common;

namespace ArtemisBankingPro.Application.Interfaces.Services
{

  
        public interface ICreditCardService
        {
         
            Task<PagedResult<CreditCardResponseDto>> GetCreditCardsAsync(string? cedula, string? status, int pageNumber, int pageSize);

            Task<CreditCardResponseDto> GetCreditCardByIdAsync(int id);

            Task<CreditCardResponseDto> AssignCreditCardAsync(CreateCreditCardRequestDto request, string adminId);

            Task UpdateCreditLimitAsync(int cardId, decimal newLimit);

            Task CancelCreditCardAsync(int cardId);
        }
    

}
