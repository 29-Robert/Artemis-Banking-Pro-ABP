using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Infrastructure.Services;
using ArtemisBankingPro.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBankingPro.Infrastructure
{
    public static class ServiceRegistration
    {
        public static void AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));

            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IJwtService, JwtService>();
        }
    }
}