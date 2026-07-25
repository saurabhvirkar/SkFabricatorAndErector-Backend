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
        var fromAddress = _config["Email:From"];
        var toAddress = _config["Email:To"];
        var smtpServer = _config["Email:SmtpServer"];
        var smtpPort = _config["Email:SmtpPort"];
        var username = _config["Email:Username"];
        var password = _config["Email:Password"];

        if (string.IsNullOrEmpty(fromAddress) || string.IsNullOrEmpty(toAddress) || string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPort))
        {
            _logger.LogWarning("Email settings incomplete; skipping inquiry notification email for inquiry ID {InquiryId}.", inquiry.Id);
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

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpServer, int.Parse(smtpPort), MailKit.Security.SecureSocketOptions.StartTls);
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            await client.AuthenticateAsync(username, password);
        }
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
        _logger.LogInformation("Inquiry notification email sent for inquiry ID {InquiryId}.", inquiry.Id);
    }
}
