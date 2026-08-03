namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string Role { get; }
        int? CommerceId { get; }
    }
}