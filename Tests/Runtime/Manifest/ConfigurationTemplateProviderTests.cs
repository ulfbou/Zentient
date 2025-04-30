using System.Globalization;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Moq;

namespace Zentient.Runtime.Manifest.Tests
{
    public class ConfigurationTemplateProviderTests
    {
        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();

            // Act
            var provider = new ConfigurationTemplateProvider(mockConfiguration.Object, mockLogger.Object);

            // Assert
            provider.Should().NotBeNull();
        }

        [Theory]
        [InlineData("Scope", "Key", "Hello, {0}!", "World", "Hello, World!")]
        [InlineData("Scope", "Key", "Value: {0:N2}", 1234.56, "Value: 1,234.56")]
        public void Resolve_ReturnsCorrectValue_WhenKeyExists(string scope, string key, string template, object arg, string expected)
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();
            mockConfiguration.Setup(c => c[$"Manifest:{scope}:{key}"]).Returns(template);
            var provider = new ConfigurationTemplateProvider(mockConfiguration.Object, mockLogger.Object);

            // Act
            var result = provider.Resolve(scope, key, CultureInfo.InvariantCulture, arg);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void Resolve_ThrowsKeyNotFoundException_WhenKeyDoesNotExist()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();
            var provider = new ConfigurationTemplateProvider(mockConfiguration.Object, mockLogger.Object);

            // Act
            var act = () => provider.Resolve("Scope", "NonExistentKey", CultureInfo.InvariantCulture);

            // Assert
            act.Should().Throw<KeyNotFoundException>();
        }

        [Theory]
        [InlineData("Scope", "Key", "Hello, {0}!", "World", "Hello, World!")]
        [InlineData("Scope", "Key", "Value: {0:N2}", 1234.56, "Value: 1,234.56")]
        public void TryResolve_ReturnsTrueAndCorrectValue_WhenKeyExists(string scope, string key, string template, object arg, string expected)
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();
            mockConfiguration.Setup(c => c[$"Manifest:{scope}:{key}"]).Returns(template);
            var provider = new ConfigurationTemplateProvider(mockConfiguration.Object, mockLogger.Object);

            // Act
            var result = provider.TryResolve(scope, key, out var value, CultureInfo.InvariantCulture, arg);

            // Assert
            result.Should().BeTrue();
            value.Should().Be(expected);
        }

        [Fact]
        public void TryResolve_ReturnsFalse_WhenKeyDoesNotExist()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();
            var provider = new ConfigurationTemplateProvider(mockConfiguration.Object, mockLogger.Object);

            // Act
            var result = provider.TryResolve("Scope", "NonExistentKey", out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().BeNull();
        }

        [Fact]
        public void Resolve_UsesCache_ForRepeatedCalls()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();
            mockConfiguration.Setup(c => c["Manifest:Scope:Key"]).Returns("Hello, {0}!");
            var provider = new ConfigurationTemplateProvider(mockConfiguration.Object, mockLogger.Object);

            // Act
            var firstCall = provider.Resolve("Scope", "Key", CultureInfo.InvariantCulture, "World");
            var secondCall = provider.Resolve("Scope", "Key", CultureInfo.InvariantCulture, "World");

            // Assert
            firstCall.Should().Be("Hello, World!");
            secondCall.Should().Be("Hello, World!");
            mockConfiguration.Verify(c => c["Manifest:Scope:Key"], Times.Once);
        }

        [Fact]
        public void Resolve_LogsWarning_WhenPlaceholdersExceedArguments()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();
            mockConfiguration.Setup(c => c["Manifest:Scope:Key"]).Returns("Hello, {0} {1}!");
            var provider = new ConfigurationTemplateProvider(mockConfiguration.Object, mockLogger.Object, enforcePlaceholderCount: true);

            // Act
            var result = provider.TryResolve("Scope", "Key", out var resolvedValue, CultureInfo.InvariantCulture, "World");

            // Assert
            result.Should().BeFalse();
            resolvedValue.Should().BeNull();
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("more placeholders")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("en-US", 1234.56, "Value: 1,234.56")]
        [InlineData("fr-FR", 1234.56, "Value: 1 234,56")]
        public void Resolve_FormatsValue_CorrectlyForDifferentCultures(string cultureName, double value, string expected)
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();
            mockConfiguration.Setup(c => c["Manifest:Scope:Key"]).Returns("Value: {0:N2}");
            var provider = new ConfigurationTemplateProvider(mockConfiguration.Object, mockLogger.Object);

            // Act
            var result = provider.Resolve("Scope", "Key", new CultureInfo(cultureName), value);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void Resolve_DoesNotLogWarning_WhenEnforcePlaceholderCountIsFalse()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();
            mockConfiguration.Setup(c => c["Manifest:Scope:Key"]).Returns("Hello, {0} {1}!");
            var provider = new ConfigurationTemplateProvider(mockConfiguration.Object, mockLogger.Object, enforcePlaceholderCount: false);

            // Act
            var result = provider.TryResolve("Scope", "Key", out var resolvedValue, CultureInfo.InvariantCulture, "World");

            // Assert
            result.Should().BeTrue();
            resolvedValue.Should().Be("Hello, World !");
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
