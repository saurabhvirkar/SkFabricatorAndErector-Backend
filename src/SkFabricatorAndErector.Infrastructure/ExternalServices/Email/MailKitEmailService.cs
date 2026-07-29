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

    public async Task SendInquiryNotificationEmailAsync(Inquiry inquiry)
    {
        var smtpServer = _config["SmtpSettings:Host"] ?? _config["Email:SmtpServer"];
        var smtpPort = _config["SmtpSettings:Port"] ?? _config["Email:SmtpPort"];
        var username = _config["SmtpSettings:Username"] ?? _config["Email:Username"];
        var password = _config["SmtpSettings:Password"] ?? _config["Email:Password"];
        var fromAddress = _config["SmtpSettings:FromEmail"] ?? _config["Email:From"] ?? username ?? "no-reply@skfabricator.com";
        var toAddress = _config["SmtpSettings:ToEmail"] ?? _config["Email:To"] ?? "admin@skfabricator.com";

        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPort) || smtpServer.Contains("REPLACE_WITH"))
        {
            _logger.LogWarning("SMTP settings not configured (Host/Port missing or placeholder); skipping inquiry notification email for inquiry ID {InquiryId}.", inquiry.Id);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("SK Fabricator Site", fromAddress));
        message.To.Add(new MailboxAddress("Admin", toAddress));
        message.Subject = $"New Inquiry from {inquiry.Name}";

        message.Body = new TextPart("plain")
        {
            Text = $@"📩 New Inquiry Received:

Name: {inquiry.Name}
Email: {inquiry.Email}
Phone: {inquiry.Phone ?? "N/A"}
Subject: {inquiry.Subject ?? "N/A"}
Category: {inquiry.Category ?? "N/A"}
Preferred Contact: {inquiry.PreferredContact ?? "N/A"}
Message:
{inquiry.Message}

Submitted At: {inquiry.SubmittedAt:yyyy-MM-dd HH:mm:ss}"
        };

        await SendEmailInternalAsync(message, smtpServer, smtpPort, username, password);
        _logger.LogInformation("Inquiry notification email sent for inquiry ID {InquiryId}.", inquiry.Id);
    }

    public async Task SendOtpCodeAsync(string toEmail, string code, string purpose)
    {
        var smtpServer = _config["SmtpSettings:Host"] ?? _config["Email:SmtpServer"];
        var smtpPort = _config["SmtpSettings:Port"] ?? _config["Email:SmtpPort"];
        var username = _config["SmtpSettings:Username"] ?? _config["Email:Username"];
        var password = _config["SmtpSettings:Password"] ?? _config["Email:Password"];
        var fromAddress = _config["SmtpSettings:FromEmail"] ?? _config["Email:From"] ?? "no-reply@skfabricator.com";

        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPort) || smtpServer.Contains("REPLACE_WITH"))
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
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || username.StartsWith("REPLACE_WITH_") || password.StartsWith("REPLACE_WITH_"))
            {
                _logger.LogWarning("SMTP credentials are unconfigured or placeholder; skipping email dispatch.");
                return;
            }

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, int.Parse(smtpPort), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SMTP Email delivery failed silently so user operation can proceed.");
        }
    }
}
