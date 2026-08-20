namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendActivationEmailAsync(string to, string token, string? activationUrl = null);
        Task SendPasswordResetEmailAsync(string to, string token, string? resetUrl = null);
        Task SendNotificationEmailAsync(string to, string subject, string body);
        
    }
}