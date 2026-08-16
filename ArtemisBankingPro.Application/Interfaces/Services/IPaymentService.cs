using ArtemisBankingPro.Application.DTOs.HermesPay;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<TransactionResponseDto> ProcessPaymentAsync(int commerceId, ProcessPaymentRequestDto request, int currentUserId);
        Task<PagedTransactionsDto> GetTransactionsAsync(int commerceId, int page, int limit);
    }
}
