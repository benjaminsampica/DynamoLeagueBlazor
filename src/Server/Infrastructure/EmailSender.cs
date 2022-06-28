using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace DynamoLeagueBlazor.Server.Infrastructure;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailSender(EmailSettings emailSettings)
    {
        _emailSettings = emailSettings;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        using var mimeMessage = new MimeMessage();

        mimeMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.Sender));
        mimeMessage.To.Add(new MailboxAddress(string.Empty, email));
        mimeMessage.Subject = subject;
        mimeMessage.Body = new TextPart("html")
        {
            Text = message
        };

        using var client = new SmtpClient();

        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

        await client.ConnectAsync(_emailSettings.MailServer);
        await client.AuthenticateAsync(_emailSettings.Sender, _emailSettings.Password);

        await client.SendAsync(mimeMessage);

        await client.DisconnectAsync(true);
    }
}

public class DevelopmentEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Task.CompletedTask;
    }
}

public class EmailSettings
{
    public const string Email = "Email";

    public string MailServer { get; set; } = null!;
    public int MailPort { get; set; }
    public string SenderName { get; set; } = null!;
    public string Sender { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string AdminEmail { get; set; }
}
