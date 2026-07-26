using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;
using SkFabricatorAndErector.Infrastructure.Persistence;

namespace SkFabricatorAndErector.Infrastructure.Services;

public class OtpService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IEmailService emailService,
    ILogger<OtpService> logger) : IOtpService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<OtpService> _logger = logger;

    public async Task<Guid> GenerateAndSendAsync(string userId, string purpose, string channel = "Email")
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        // Invalidate active unconsumed OTPs for this user & purpose
        var oldCodes = await _dbContext.OtpCodes
            .Where(o => o.UserId == userId && o.Purpose == purpose && o.ConsumedAt == null)
            .ToListAsync();

        foreach (var old in oldCodes)
        {
            old.ConsumedAt = DateTime.UtcNow;
        }

        // Generate 6-digit random code
        var rawCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var codeHash = HashCode(rawCode);

        var otpEntity = new OtpCode
        {
            UserId = userId,
            CodeHash = codeHash,
            Purpose = purpose,
            DeliveryChannel = channel,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        _dbContext.OtpCodes.Add(otpEntity);
        await _dbContext.SaveChangesAsync();

        if (channel.Equals("Email", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(user.Email))
        {
            await _emailService.SendOtpCodeAsync(user.Email, rawCode, purpose);
        }
        else
        {
            _logger.LogWarning("OTP delivery channel {Channel} not implemented or missing contact info for User {UserId}.", channel, userId);
        }

        return otpEntity.Id;
    }

    public async Task<bool> VerifyAsync(string userId, string purpose, string submittedCode)
    {
        if (string.IsNullOrWhiteSpace(submittedCode)) return false;

        var codeHash = HashCode(submittedCode);
        var activeOtp = await _dbContext.OtpCodes
            .Where(o => o.UserId == userId && o.Purpose == purpose && o.ConsumedAt == null)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (activeOtp == null)
        {
            _logger.LogWarning("No active OTP found for User {UserId} and Purpose {Purpose}.", userId, purpose);
            return false;
        }

        if (activeOtp.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("OTP expired for User {UserId}.", userId);
            activeOtp.ConsumedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return false;
        }

        if (activeOtp.AttemptCount >= 5)
        {
            _logger.LogWarning("OTP attempt limit exceeded for User {UserId}.", userId);
            activeOtp.ConsumedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return false;
        }

        activeOtp.AttemptCount++;

        if (activeOtp.CodeHash != codeHash)
        {
            _logger.LogWarning("Invalid OTP code submitted for User {UserId}.", userId);
            await _dbContext.SaveChangesAsync();
            return false;
        }

        activeOtp.ConsumedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static string HashCode(string rawCode)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawCode));
        return Convert.ToBase64String(bytes);
    }
}
