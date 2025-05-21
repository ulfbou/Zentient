using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Zentient.Definitions.Core;
using Zentient.Definitions.Loader;
using Zentient.Definitions.Extensions;
using FluentAssertions;

namespace Zentient.Definitions.Tests
{
    public class LoadTests
    {
        [Fact]
        public void Load_ValidAssemblyFromFilePath_LoadsAndCachesAssembly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<DefinitionsLoadContext>>();
            var tempAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var context = new DefinitionsLoadContext("TestContext", new[] { Path.GetDirectoryName(tempAssemblyPath)! }, mockLogger.Object);

            // Act
            var assembly = context.LoadFromAssemblyName(new AssemblyName(Assembly.GetExecutingAssembly().GetName().Name!));

            // Assert
            assembly.Should().NotBeNull();
            assembly.FullName.Should().Be(Assembly.GetExecutingAssembly().FullName);
            mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Loaded assembly")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Load_AssemblyNotFound_ReturnsNullAndLogsDebug()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<DefinitionsLoadContext>>();
            var context = new DefinitionsLoadContext("TestContext", new[] { "nonexistent/path" }, mockLogger.Object);

            // Act
            var assembly = context.LoadFromAssemblyName(new AssemblyName("NonExistentAssembly"));

            // Assert
            assembly.Should().BeNull();
            mockLogger.Verify(l => l.LogDebug(It.Is<string>(s => s.Contains("Could not find or load assembly")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Load_IncorrectAssemblyName_LogsWarningAndDoesNotCache()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<DefinitionsLoadContext>>();
            var tempAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var context = new DefinitionsLoadContext("TestContext", new[] { Path.GetDirectoryName(tempAssemblyPath)! }, mockLogger.Object);

            // Act
            var assembly = context.LoadFromAssemblyName(new AssemblyName("IncorrectName"));

            // Assert
            assembly.Should().BeNull();
            mockLogger.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("does not match requested name")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Load_InvalidAssemblyFormat_LogsWarningAndReturnsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<DefinitionsLoadContext>>();
            var tempFile = Path.GetTempFileName(); // Create a temporary file to simulate an invalid assembly
            var context = new DefinitionsLoadContext("TestContext", new[] { Path.GetDirectoryName(tempFile)! }, mockLogger.Object);

            // Act
            var assembly = context.LoadFromAssemblyPath(Path.GetFileNameWithoutExtension(tempFile));

            // Assert
            assembly.Should().BeNull();
            mockLogger.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Invalid assembly format")), It.IsAny<object[]>()), Times.Once);

            // Cleanup
            File.Delete(tempFile);
        }

        [Fact]
        public void Load_CachesAssemblyOnFirstLoad()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<DefinitionsLoadContext>>();
            var tempAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var context = new DefinitionsLoadContext("TestContext", new[] { Path.GetDirectoryName(tempAssemblyPath)! }, mockLogger.Object);

            // Act
            var firstLoad = context.LoadFromAssemblyName(new AssemblyName(Assembly.GetExecutingAssembly().GetName().Name!));
            var secondLoad = context.LoadFromAssemblyName(new AssemblyName(Assembly.GetExecutingAssembly().GetName().Name!));

            // Assert
            firstLoad.Should().BeSameAs(secondLoad);
            mockLogger.Verify(l => l.LogDebug(It.Is<string>(s => s.Contains("Found assembly")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Load_EmptyAssemblyPaths_DoesNotThrow()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<DefinitionsLoadContext>>();
            var context = new DefinitionsLoadContext("TestContext", Array.Empty<string>(), mockLogger.Object);

            // Act
            var assembly = context.LoadFromAssemblyName(new AssemblyName("NonExistentAssembly"));

            // Assert
            assembly.Should().BeNull();
            mockLogger.Verify(l => l.LogDebug(It.Is<string>(s => s.Contains("Could not find or load assembly")), It.IsAny<object[]>()), Times.Once);
        }
    }
}
