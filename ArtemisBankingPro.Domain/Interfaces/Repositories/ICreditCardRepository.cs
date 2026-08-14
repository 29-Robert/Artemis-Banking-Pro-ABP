
using ArtemisBankingPro.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Domain.Interfaces.Repositories
{
    public interface ICreditCardRepository : IGenericRepository<CreditCard>
    {
        Task<CreditCard> GetByCardNumberAsync(string cardNumber);
        Task<CreditCard> GetByCardNumberWithClientAsync(string cardNumber);
        Task<IReadOnlyList<CreditCard>> GetCardsByClientAsync(string clientId);
        Task<bool> HasActiveCreditCardAsync(int clientId);
        Task<CreditCard?> GetByIdWithDetailsAsync(int id);
        Task<(IReadOnlyList<CreditCard> Items, int TotalCount)> SearchAsync(
           string? cedula, string? status, int pageNumber, int pageSize);
    }
}