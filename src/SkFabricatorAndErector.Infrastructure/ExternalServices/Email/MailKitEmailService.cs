using Microsoft.AspNetCore.Http;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Infrastructure.ExternalServices.Email;

public class MailKitEmailService(IConfiguration config, ILogger<MailKitEmailService> logger) : IEmailService
{
    private readonly IConfiguration _config = config;
    private readonly ILogger<MailKitEmailService> _logger = logger;

    private string? GetConfigValue(params string[] keys)
    {
        foreach (var key in keys)
        {
            var val = _config[key];
            if (!string.IsNullOrWhiteSpace(val) && !val.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase))
            {
                return val;
            }
        }
        return null;
    }

    public async Task SendInquiryNotificationEmailAsync(Inquiry inquiry, IFormFile? file)
    {
        var smtpServer = GetConfigValue("SmtpSettings:Host", "Email:SmtpServer");
        var smtpPort = GetConfigValue("SmtpSettings:Port", "Email:SmtpPort");
        var username = GetConfigValue("SmtpSettings:Username", "Email:Username");
        var password = GetConfigValue("SmtpSettings:Password", "Email:Password");
        var fromAddress = GetConfigValue("SmtpSettings:FromEmail", "Email:From") ?? username ?? "no-reply@skfabricator.com";
        var toAddress = GetConfigValue("SmtpSettings:ToEmail", "Email:To") ?? "ssvirkar04@gmail.com";

        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPort))
        {
            _logger.LogWarning("SMTP settings unconfigured; skipping inquiry notification email for inquiry ID {InquiryId}.", inquiry.Id);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("SK Fabricator Site", fromAddress));
        message.To.Add(new MailboxAddress("Admin", toAddress));
        message.Subject = $"New Inquiry from {inquiry.Name}";

        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = $@"
<div style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;"">
        <div style=""text-align: center; padding-bottom: 20px; border-bottom: 1px solid #ddd;"">
            <h2 style=""color: #0B4C8C;"">New Inquiry Received</h2>
        </div>
        <div style=""padding: 20px 0;"">
            <p>You have received a new inquiry from the website. Details are as follows:</p>
            <table style=""width: 100%; border-collapse: collapse;"">
                <tr style=""border-bottom: 1px solid #eee;"">
                    <td style=""padding: 10px 0; font-weight: bold;"">Name:</td>
                    <td style=""padding: 10px 0;"">{inquiry.Name}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #eee;"">
                    <td style=""padding: 10px 0; font-weight: bold;"">Email:</td>
                    <td style=""padding: 10px 0;""><a href=""mailto:{inquiry.Email}"">{inquiry.Email}</a></td>
                </tr>
                <tr style=""border-bottom: 1px solid #eee;"">
                    <td style=""padding: 10px 0; font-weight: bold;"">Phone:</td>
                    <td style=""padding: 10px 0;"">{inquiry.Phone ?? "N/A"}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #eee;"">
                    <td style=""padding: 10px 0; font-weight: bold;"">Subject:</td>
                    <td style=""padding: 10px 0;"">{inquiry.Subject ?? "N/A"}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #eee;"">
                    <td style=""padding: 10px 0; font-weight: bold;"">Service of Interest:</td>
                    <td style=""padding: 10px 0;"">{inquiry.Category ?? "N/A"}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #eee;"">
                    <td style=""padding: 10px 0; font-weight: bold;"">Submitted At:</td>
                    <td style=""padding: 10px 0;"">{inquiry.SubmittedAt:yyyy-MM-dd HH:mm:ss} UTC</td>
                </tr>
                <tr>
                    <td style=""padding: 10px 0; font-weight: bold; vertical-align: top;"">Message:</td>
                    <td style=""padding: 10px 0; white-space: pre-wrap;"">{inquiry.Message}</td>
                </tr>
            </table>
        </div>
        <div style=""text-align: center; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #888;"">
            <p>This email was sent from the inquiry form on the SK Fabricator & Erector website.</p>
        </div>
    </div>
</div>";

        if (file is { Length: > 0 })
        {
            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            bodyBuilder.Attachments.Add(file.FileName, memoryStream.ToArray(), ContentType.Parse(file.ContentType));
        }

        message.Body = bodyBuilder.ToMessageBody();

        await SendEmailInternalAsync(message, smtpServer, smtpPort, username, password);
        _logger.LogInformation("Inquiry notification email sent for inquiry ID {InquiryId}.", inquiry.Id);
    }

    public async Task SendOtpCodeAsync(string toEmail, string code, string purpose)
    {
        var smtpServer = GetConfigValue("SmtpSettings:Host", "Email:SmtpServer");
        var smtpPort = GetConfigValue("SmtpSettings:Port", "Email:SmtpPort");
        var username = GetConfigValue("SmtpSettings:Username", "Email:Username");
        var password = GetConfigValue("SmtpSettings:Password", "Email:Password");
        var fromAddress = GetConfigValue("SmtpSettings:FromEmail", "Email:From") ?? username ?? "no-reply@skfabricator.com";

        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPort))
        {
            _logger.LogWarning("SMTP configuration missing. OTP code [{Code}] generated for {Email} ({Purpose}).", code, toEmail, purpose);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("SK Fabricator Security", fromAddress));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = $"Your Verification Code: {code}";

        message.Body = new TextPart("html")
        {
            Text = $@"<div style='font-family: Arial, sans-serif; padding: 20px;'>
<h2>Verification Code</h2>
<p>Your one-time verification code for <strong>{purpose}</strong> is:</p>
<h1 style='font-size: 32px; letter-spacing: 5px; color: #2563eb;'>{code}</h1>
<p>This code expires in 10 minutes. If you did not request this, please secure your account immediately.</p>
</div>"
        };

        await SendEmailInternalAsync(message, smtpServer, smtpPort, username, password);
        _logger.LogInformation("OTP code sent to {Email} for {Purpose}.", toEmail, purpose);
    }

    private async Task SendEmailInternalAsync(MimeMessage message, string smtpServer, string smtpPort, string? username, string? password)
    {
        try
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || username.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("SMTP credentials missing or placeholder; skipping email dispatch.");
                return;
            }

            var port = int.TryParse(smtpPort, out var p) ? p : 587;
            var socketOptions = port switch
            {
                465 => MailKit.Security.SecureSocketOptions.SslOnConnect,
                587 => MailKit.Security.SecureSocketOptions.StartTls,
                _ => MailKit.Security.SecureSocketOptions.Auto
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            using var client = new SmtpClient();
            client.Timeout = 15000;

            await client.ConnectAsync(smtpServer, port, socketOptions, cts.Token);
            await client.AuthenticateAsync(username, password, cts.Token);
            await client.SendAsync(message, cts.Token);
            await client.DisconnectAsync(true, cts.Token);
            _logger.LogInformation("Email successfully dispatched via {SmtpServer}:{Port} to {To}", smtpServer, port, message.To.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP Email delivery failed for server {SmtpServer}:{Port}.", smtpServer, smtpPort);
        }
    }
}
