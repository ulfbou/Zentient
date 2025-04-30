using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Zentient.Runtime.Manifest.Tests
{
    public class TemplateProviderServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTemplateProviderServices_WithDefaultParameters_RegistersServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();
            services.AddSingleton(mockConfiguration.Object);
            services.AddSingleton(mockLogger.Object);

            // Act
            services.AddTemplateProviderServices();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var provider = serviceProvider.GetService<ILocalizedValueProvider>();
            provider.Should().NotBeNull().And.BeOfType<ConfigurationTemplateProvider>();
        }

        [Fact]
        public void AddTemplateProviderServices_WithCustomParameters_RegistersServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();
            services.AddSingleton(mockConfiguration.Object);
            services.AddSingleton(mockLogger.Object);

            // Act
            services.AddTemplateProviderServices("CustomRootKey", false);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var provider = serviceProvider.GetService<ILocalizedValueProvider>();
            provider.Should().NotBeNull().And.BeOfType<ConfigurationTemplateProvider>();
        }

        [Fact]
        public void AddTemplateProviderServices_WithCustomOptions_RegistersServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();
            services.AddSingleton(mockConfiguration.Object);
            services.AddSingleton(mockLogger.Object);

            // Act
            services.AddTemplateProviderServices(options =>
            {
                options.RootConfigKey = "CustomRootKey";
                options.EnforcePlaceholderCount = false;
            });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var provider = serviceProvider.GetService<ILocalizedValueProvider>();
            provider.Should().NotBeNull().And.BeOfType<ConfigurationTemplateProvider>();
        }

        [Fact]
        public void AddTemplateProviderServices_RegistersILocalizedValueProviderAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();
            services.AddSingleton(mockConfiguration.Object);
            services.AddSingleton(mockLogger.Object);

            // Act
            services.AddTemplateProviderServices();

            // Assert
            var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(ILocalizedValueProvider));
            serviceDescriptor.Should().NotBeNull();
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
        public void AddTemplateProviderServices_NullServiceCollection_ThrowsArgumentNullException()
        {
            // Arrange
            IServiceCollection? services = null;

            // Act
            var act = () => services!.AddTemplateProviderServices();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddTemplateProviderServices_NullRootConfigKey_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<ConfigurationTemplateProvider>>();
            services.AddSingleton(mockConfiguration.Object);
            services.AddSingleton(mockLogger.Object);

            // Act
            var act = () => services.AddTemplateProviderServices(null!, true);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
