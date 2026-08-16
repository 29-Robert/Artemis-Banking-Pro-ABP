using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Persistence.Repositories
{
    public class CommerceRepository(ApplicationDbContext dbContext) : GenericRepository<Commerce>(dbContext), ICommerceRepository
    {
        public async Task<Commerce> GetByEmailAsync(string email)
        {
            return await _dbContext.Commerces
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Commerce> GetByIdWithUserAsync(int id)
        {
            return await _dbContext.Commerces
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Commerce> GetByRncAsync(string rnc)
        {
            return await _dbContext.Commerces
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.RNC == rnc);
        }
    }
}
