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
    public class CreditCardConsumptionRepository : GenericRepository<CreditCardConsumption>, ICreditCardConsumptionRepository
    {
        public CreditCardConsumptionRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<CreditCardConsumption>> GetConsumptionsByCardAsync(int creditCardId)
        {
            return await _dbContext.Set<CreditCardConsumption>()
                .Where(c => c.CreditCardId == creditCardId)
                .OrderByDescending(c => c.TransactionDate)
                .ToListAsync();
        }
    }
}