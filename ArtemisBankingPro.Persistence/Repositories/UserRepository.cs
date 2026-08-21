using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Persistence.Repositories
{
    public class UserRepository(ApplicationDbContext dbContext) : GenericRepository<User>(dbContext), IUserRepository
    {
        public async Task<User> GetByCedulaAsync(string cedula)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Cedula == cedula);
        }

        public override async Task<User> GetByIdAsync(int id)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByCommerceIdAsync(int commerceId)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.CommerceId == commerceId);
        }

        public new async Task<IReadOnlyList<User>> GetAllAsync()
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .ToListAsync();
        }

    }
}
