using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using SkFabricatorAndErector.Application.Extensions;
using Xunit;

namespace SkFabricatorAndErector.UnitTests.Extensions;

public class SecurityConfigurationExtensionsTests
{
    [Fact]
    public void ValidateStartupSecurity_ShouldThrowInvalidOperationException_WhenProductionConfigHasPlaceholderJwtKey()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?> {
            {"Jwt:Key", "REPLACE_WITH_STRONG_SECRET_KEY_MIN_32_CHARS"},
            {"ConnectionStrings:DefaultConnection", "Data Source=skfabricator.db"},
            {"CloudinarySettings:CloudName", "real-name"},
            {"CloudinarySettings:ApiKey", "real-key"},
            {"CloudinarySettings:ApiSecret", "real-secret"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Production");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => config.ValidateStartupSecurity(envMock.Object));
    }

    [Fact]
    public void ValidateStartupSecurity_ShouldNotThrow_InDevelopmentEnvironment()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?> {
            {"Jwt:Key", "REPLACE_WITH_STRONG_SECRET_KEY_MIN_32_CHARS"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Act & Assert
        config.ValidateStartupSecurity(envMock.Object); // Should not throw
    }
}
