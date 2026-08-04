using ArtemisBankingPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Domain.Interfaces.Repositories
{
    public interface ICreditCardRepository : IGenericRepository<CreditCard>
    {
        Task<CreditCard> GetByCardNumberAsync(string cardNumber);
        Task<IReadOnlyList<CreditCard>> GetCardsByClientAsync(string clientId);
        Task<bool> HasActiveCreditCardAsync(string clientId);
    }
}
