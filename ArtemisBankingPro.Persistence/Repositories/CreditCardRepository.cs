using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using ArtemisBankingPro.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;



namespace ArtemisBankingPro.Persistence.Repositories
{
    public class CreditCardRepository : GenericRepository<CreditCard>, ICreditCardRepository
    {
        public CreditCardRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<CreditCard> GetByCardNumberAsync(string cardNumber)
        {
            return await _dbContext.Set<CreditCard>()
                .Include(c => c.Consumptions)
                .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
        }

        public async Task<bool> HasActiveCreditCardAsync(string clientId)
        {
            return await _dbContext.Set<CreditCard>()
                .AnyAsync(c => c.ClientId == clientId && c.Status == "Activa");
        }

        public async Task<IReadOnlyList<CreditCard>> GetCardsByClientAsync(string clientId)
        {
            return await _dbContext.Set<CreditCard>()
                .Where(c => c.ClientId == clientId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}