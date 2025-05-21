using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zentient.Definitions.Core;
using Zentient.Definitions.Extensions;
using Zentient.Definitions.Loader;

namespace Zentient.Definitions.Tests
{
    public class ServiceCollectionTests
    {
        [Fact]
        public void AddDefinitions_ConfiguresAssemblyPathsCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c["AssemblyPaths"]).Returns("path1;path2");

            // Act
            services.AddDefinitions(mockConfiguration.Object, builder =>
            {
                builder.WithAssemblyPaths("path1", "path2");
            });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<DefinitionsOptions>();
            options.AssemblyPaths.Should().Contain(new[] { "path1", "path2" });
        }

        [Fact]
        public void AddDefinitions_InvokesPreLoadAndOnModuleLoadedActions()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var preLoadInvoked = false;
            var onModuleLoadedInvoked = false;

            // Act
            services.AddDefinitions(mockConfiguration.Object, builder =>
            {
                builder.WithPreLoadAction(_ => preLoadInvoked = true);
                builder.WithOnModuleLoadedAction((_, _) => onModuleLoadedInvoked = true);
            });

            // Assert
            preLoadInvoked.Should().BeTrue();
            onModuleLoadedInvoked.Should().BeTrue();
        }

        [Fact]
        public void AddDefinitions_HandlesCriticalModuleFailure()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<DefinitionsLoadContext>>();

            services.AddSingleton(mockLogger.Object);

            // Act
            Action act = () => services.AddDefinitions(mockConfiguration.Object, builder =>
            {
                builder.WithCriticalModules("CriticalModule");
            });

            // Assert
            act.Should().Throw<DefinitionLoadException>();
        }

        [Fact]
        public void AddDefinitions_HandlesNonCriticalModuleFailure()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<DefinitionsLoadContext>>();

            services.AddSingleton(mockLogger.Object);

            // Act
            services.AddDefinitions(mockConfiguration.Object, builder =>
            {
                builder.WithCriticalModules();
            });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<DefinitionsOptions>();
            options.Should().NotBeNull();
        }
    }
}
