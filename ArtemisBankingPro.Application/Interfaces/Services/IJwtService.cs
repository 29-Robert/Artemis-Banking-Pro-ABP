using ArtemisBankingPro.Domain.Entities;

namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
    }
}