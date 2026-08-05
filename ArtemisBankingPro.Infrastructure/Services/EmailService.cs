using System.Net;
using System.Net.Mail;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace ArtemisBankingPro.Infrastructure.Services
{
    public class EmailService(IOptions<MailSettings> mailSettings) : IEmailService
    {
        private readonly MailSettings _mailSettings = mailSettings.Value;

        public async Task SendActivationEmailAsync(string to, string token)
        {
            var subject = "Activación de cuenta";
            var body = $@"
                <p>Hola,</p>
                <p>Su cuenta ha sido creada correctamente en Artemis Banking.</p>
                <p>Utilice el siguiente token para activar su cuenta desde el endpoint correspondiente:</p>
                <h3>{token}</h3>
                <p>Si usted no esperaba la creación de esta cuenta, ignore este mensaje.</p>";

            await SendNotificationEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string to, string token)
        {
            var subject = "Token de restablecimiento de contraseña";
            var body = $@"
                <p>Hola,</p>
                <p>Se ha generado un token para restablecer la contraseña de su cuenta.</p>
                <p>Token de restablecimiento:</p>
                <h3>{token}</h3>
                <p>Utilice este token en el endpoint correspondiente para completar el cambio de contraseña.</p>
                <p>Si usted no solicitó este cambio, ignore este mensaje.</p>";

            await SendNotificationEmailAsync(to, subject, body);
        }

        public async Task SendNotificationEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_mailSettings.SmtpHost, _mailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_mailSettings.SmtpUser, _mailSettings.SmtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_mailSettings.EmailFrom, "Artemis Banking Pro"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);
            await client.SendMailAsync(mailMessage);
        }
    }
}