using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.CreditCard;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface ICreditCardService
    {
        Task<PagedResult<CreditCardResponseDto>> GetCreditCardsAsync(
            string? cedula, string? status, int pageNumber, int pageSize);

        Task<CreditCardResponseDto> GetCreditCardByIdAsync(int id);

        Task<CreditCardCreatedResponseDto> AssignCreditCardAsync(CreateCreditCardRequestDto request, int adminId);

        Task UpdateCreditLimitAsync(int cardId, decimal newLimit);

        Task CancelCreditCardAsync(int cardId);
    }
}