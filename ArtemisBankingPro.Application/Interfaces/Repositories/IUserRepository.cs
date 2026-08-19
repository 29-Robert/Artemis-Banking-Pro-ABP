using ArtemisBankingPro.Domain.Entities;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByCedulaAsync(string cedula);
        Task<User> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
    }
}