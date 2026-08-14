using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using ArtemisBankingPro.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Persistence.Repositories
{
    public class CreditCardRepository : GenericRepository<CreditCard>, ICreditCardRepository
    {
        private const int MaxPageSize = 20;

        public CreditCardRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<CreditCard> GetByCardNumberAsync(string cardNumber)
        {
            return await _dbContext.Set<CreditCard>()
                .Include(c => c.Consumptions)
                .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
        }

        public async Task<IReadOnlyList<CreditCard>> GetCardsByClientAsync(int clientId)
        {
            return await _dbContext.Set<CreditCard>()
                .Where(c => c.ClientId == clientId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasActiveCreditCardAsync(int clientId)
        {
            return await _dbContext.Set<CreditCard>()
                .AnyAsync(c => c.ClientId == clientId && c.Status == "Activa");
        }

        public async Task<(IReadOnlyList<CreditCard> Items, int TotalCount)> SearchAsync(
            string? cedula, string? status, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = MaxPageSize;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;

            var query = _dbContext.Set<CreditCard>()
                .Include(c => c.Client)
                .AsQueryable();

            var hasCedula = !string.IsNullOrWhiteSpace(cedula);
            var hasStatus = !string.IsNullOrWhiteSpace(status) && status != "Todas";

            if (hasCedula)
            {
                query = query.Where(c => c.Client.Id.ToString() == cedula);
            }

            if (hasStatus)
            {
                query = query.Where(c => c.Status == status);
            }
            else if (!hasCedula)
            {
              
                query = query.Where(c => c.Status == "Activa");
            }

            
            query = hasCedula && !hasStatus
                ? query.OrderBy(c => c.Status == "Activa" ? 0 : 1)
                       .ThenByDescending(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<CreditCard?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbContext.Set<CreditCard>()
                .Include(c => c.Client)
                .Include(c => c.Consumptions)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public Task<CreditCard> GetByCardNumberWithClientAsync(string cardNumber)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<CreditCard>> GetCardsByClientAsync(string clientId)
        {
            throw new NotImplementedException();
        }
    }
}