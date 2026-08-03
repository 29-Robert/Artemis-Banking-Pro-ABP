namespace ArtemisBankingPro.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendActivationEmailAsync(string to, string token);
        Task SendPasswordResetEmailAsync(string to, string token);
        Task SendNotificationEmailAsync(string to, string subject, string body);
    }
}