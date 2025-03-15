using EveryDaily.Core.Settings;
using MailKit.Net.Smtp;
using MassTransit.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EveryDaily.Application.Services.Email;

public interface IEmailService
{
    Task SendAsync(string email, string subject, string body);
    string GetMailTemplate(string formattedHtml);
}

public class EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    : IEmailService
{
    public async Task SendAsync(string email, string subject, string body)
    {
        var emailSettings = options.Value;
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(emailSettings.FromName, emailSettings.From));
        message.To.Add(new MailboxAddress(email, email));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        
        try
        {
            await client.ConnectAsync(emailSettings.Host, emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(emailSettings.From, emailSettings.Psw);

            await client.SendAsync(message);

            await client.DisconnectAsync(true);

            logger.LogInformation($"Email sending success. Email: {email}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Email sending failed: {ex.Message} Email: {email}");
        }
    }

    public string GetMailTemplate(string formattedHtml)
    {
        throw new NotImplementedException();
    }
}