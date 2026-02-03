using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace PocMfa.Application;

public record EmailSettings
{
    public string ApiKey { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
};

public interface IEmailService
{
    Task SendPasswordResetAsync(string toEmail, string resetLink);
}

public class SendGridEmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public SendGridEmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendPasswordResetAsync(string toEmail, string resetLink)
    {
        var client = new SendGridClient(_settings.ApiKey);
        var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
        var to = new EmailAddress(toEmail);
        var subject = "Reset de senha";
        var html = $"<p>Para redefinir sua senha, clique no link abaixo:</p><p><a href='{resetLink}'>Resetar senha</a></p>";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: html);
        await client.SendEmailAsync(msg);
    }
}
