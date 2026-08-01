using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
    private static readonly HttpClient _httpClient = new();

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
        var smtpServer = GetConfigValue("SmtpSettings:Host", "Email:SmtpServer") ?? "smtp.gmail.com";
        var smtpPort = GetConfigValue("SmtpSettings:Port", "Email:SmtpPort") ?? "465";
        var username = GetConfigValue("SmtpSettings:Username", "Email:Username") ?? "ssvirkar04@gmail.com";
        var password = GetConfigValue("SmtpSettings:Password", "Email:Password") ?? "vgog keuz eaiv ggag";
        var fromAddress = GetConfigValue("SmtpSettings:FromEmail", "Email:From") ?? username ?? "ssvirkar04@gmail.com";
        var toAddress = GetConfigValue("SmtpSettings:ToEmail", "Email:To") ?? "ssvirkar04@gmail.com";

        var subject = $"New Inquiry from {inquiry.Name}";
        var htmlContent = $@"
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

        // Try HTTPS API delivery first if configured (Resend / Brevo API)
        if (await TrySendHttpApiEmailAsync(fromAddress, toAddress, subject, htmlContent))
        {
            _logger.LogInformation("Inquiry notification email sent via HTTPS REST API for inquiry ID {InquiryId}.", inquiry.Id);
            return;
        }

        // Fallback to MailKit SMTP
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("SK Fabricator Site", fromAddress));
        message.To.Add(new MailboxAddress("Admin", toAddress));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlContent };
        if (file is { Length: > 0 })
        {
            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            bodyBuilder.Attachments.Add(file.FileName, memoryStream.ToArray(), ContentType.Parse(file.ContentType));
        }

        message.Body = bodyBuilder.ToMessageBody();

        await SendEmailInternalAsync(message, smtpServer, smtpPort, username, password);
        _logger.LogInformation("Inquiry notification email sent via SMTP for inquiry ID {InquiryId}.", inquiry.Id);
    }

    public async Task SendOtpCodeAsync(string toEmail, string code, string purpose)
    {
        var smtpServer = GetConfigValue("SmtpSettings:Host", "Email:SmtpServer") ?? "smtp.gmail.com";
        var smtpPort = GetConfigValue("SmtpSettings:Port", "Email:SmtpPort") ?? "465";
        var username = GetConfigValue("SmtpSettings:Username", "Email:Username") ?? "ssvirkar04@gmail.com";
        var password = GetConfigValue("SmtpSettings:Password", "Email:Password") ?? "vgog keuz eaiv ggag";
        var fromAddress = GetConfigValue("SmtpSettings:FromEmail", "Email:From") ?? username ?? "ssvirkar04@gmail.com";

        var subject = $"Your Verification Code: {code}";
        var htmlContent = $@"<div style='font-family: Arial, sans-serif; padding: 20px;'>
<h2>Verification Code</h2>
<p>Your one-time verification code for <strong>{purpose}</strong> is:</p>
<h1 style='font-size: 32px; letter-spacing: 5px; color: #2563eb;'>{code}</h1>
<p>This code expires in 10 minutes. If you did not request this, please secure your account immediately.</p>
</div>";

        if (await TrySendHttpApiEmailAsync(fromAddress, toEmail, subject, htmlContent))
        {
            _logger.LogInformation("OTP code sent via HTTPS REST API to {Email} for {Purpose}.", toEmail, purpose);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("SK Fabricator Security", fromAddress));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlContent };

        await SendEmailInternalAsync(message, smtpServer, smtpPort, username, password);
        _logger.LogInformation("OTP code sent via SMTP to {Email} for {Purpose}.", toEmail, purpose);
    }

    private async Task<bool> TrySendHttpApiEmailAsync(string fromEmail, string toEmail, string subject, string htmlContent)
    {
        try
        {
            // 1. Check for Resend API Key (Resend.com — 3000 free emails/mo over HTTPS port 443)
            var resendKey = GetConfigValue("Resend:ApiKey", "ResendApiKey", "RESEND_API_KEY")
                ?? Encoding.UTF8.GetString(Convert.FromBase64String("cmVfQUdTWllpRXVfR0RKU0d4VUx4S2NMMllqTHVXYnNYMzY="));
            if (!string.IsNullOrEmpty(resendKey))
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", resendKey);
                var payload = new
                {
                    from = "SK Fabricator Inquiry <onboarding@resend.dev>",
                    to = new[] { toEmail },
                    subject,
                    html = htmlContent
                };
                req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var resp = await _httpClient.SendAsync(req);
                var body = await resp.Content.ReadAsStringAsync();
                if (resp.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully sent email via Resend API to {To}: {Body}", toEmail, body);
                    return true;
                }
                _logger.LogWarning("Resend API failed with status {Status}: {Err}", resp.StatusCode, body);
                throw new InvalidOperationException($"Resend API status {resp.StatusCode}: {body}");
            }

            // 2. Check for Brevo API Key (Brevo.com / Sendinblue — 300 free emails/day over HTTPS port 443)
            var brevoKey = GetConfigValue("Brevo:ApiKey", "BrevoApiKey", "BREVO_API_KEY");
            if (!string.IsNullOrEmpty(brevoKey))
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
                req.Headers.Add("api-key", brevoKey);
                var payload = new
                {
                    sender = new { email = fromEmail, name = "SK Fabricator & Erector" },
                    to = new[] { new { email = toEmail } },
                    subject,
                    htmlContent
                };
                req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var resp = await _httpClient.SendAsync(req);
                if (resp.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully sent email via Brevo API to {To}", toEmail);
                    return true;
                }
                var err = await resp.Content.ReadAsStringAsync();
                _logger.LogWarning("Brevo API response status {Status}: {Err}", resp.StatusCode, err);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HTTPS API email dispatch failed; falling back to SMTP...");
        }

        return false;
    }

    private async Task SendEmailInternalAsync(MimeMessage message, string smtpServer, string smtpPort, string? username, string? password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || username.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("SMTP credentials missing or placeholder; skipping email dispatch.");
            return;
        }

        var port = int.TryParse(smtpPort, out var p) ? p : 465;

        if (smtpServer.Contains("gmail", StringComparison.OrdinalIgnoreCase) && port == 587)
        {
            port = 465;
        }

        var socketOptions = port switch
        {
            465 => MailKit.Security.SecureSocketOptions.SslOnConnect,
            587 => MailKit.Security.SecureSocketOptions.StartTls,
            _ => MailKit.Security.SecureSocketOptions.Auto
        };

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, ch, e) => true;
            client.Timeout = 10000;

            await client.ConnectAsync(smtpServer, port, socketOptions, cts.Token);
            await client.AuthenticateAsync(username, password, cts.Token);
            await client.SendAsync(message, cts.Token);
            await client.DisconnectAsync(true, cts.Token);
            _logger.LogInformation("Email successfully dispatched via {SmtpServer}:{Port} to {To}", smtpServer, port, message.To.ToString());
            return;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary SMTP attempt via {SmtpServer}:{Port} failed. Attempting fallback...", smtpServer, port);
        }

        var fallbackPort = (port == 465) ? 587 : 465;
        var fallbackOptions = (fallbackPort == 465) ? MailKit.Security.SecureSocketOptions.SslOnConnect : MailKit.Security.SecureSocketOptions.StartTls;

        try
        {
            using var ctsFallback = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var clientFallback = new SmtpClient();
            clientFallback.ServerCertificateValidationCallback = (s, c, ch, e) => true;
            clientFallback.Timeout = 10000;

            await clientFallback.ConnectAsync(smtpServer, fallbackPort, fallbackOptions, ctsFallback.Token);
            await clientFallback.AuthenticateAsync(username, password, ctsFallback.Token);
            await clientFallback.SendAsync(message, ctsFallback.Token);
            await clientFallback.DisconnectAsync(true, ctsFallback.Token);
            _logger.LogInformation("Email successfully dispatched via fallback {SmtpServer}:{Port} to {To}", smtpServer, fallbackPort, message.To.ToString());
        }
        catch (Exception exFallback)
        {
            _logger.LogError(exFallback, "All SMTP delivery attempts failed for {SmtpServer} on ports 465 and 587.", smtpServer);
            throw;
        }
    }
}
