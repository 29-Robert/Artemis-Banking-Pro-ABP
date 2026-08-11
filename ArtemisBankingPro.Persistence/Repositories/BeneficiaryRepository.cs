using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Persistence.Repositories
{
    public class BeneficiaryRepository : GenericRepository<Beneficiary>, IBeneficiaryRepository
    {
        public BeneficiaryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Beneficiary>> GetByClientAsync(int clientId)
        {
            return await _dbContext.Set<Beneficiary>()
                .Where(b => b.ClientId == clientId)
                .ToListAsync();
        }
    }
}
