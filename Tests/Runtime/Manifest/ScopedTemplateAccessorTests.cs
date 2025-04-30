using System.Globalization;

using FluentAssertions;

using Moq;

using Xunit;

namespace Zentient.Runtime.Manifest.Tests
{
    public class ScopedTemplateAccessorTests
    {
        public enum TestScope
        {
            Key1,
            Key2
        }

        private class TestScopedTemplateAccessor : ScopedTemplateAccessor<TestScope>
        {
            public TestScopedTemplateAccessor(ILocalizedValueProvider localizedValueProvider)
                : base(localizedValueProvider)
            {
            }

            public string PublicGetValue(TestScope key, params object[] args) => GetValue(key, args);

            public string PublicGetValue(TestScope key, CultureInfo culture, params object[] args) => GetValue(key, culture, args);

            public bool PublicTryGetValue(TestScope key, out string value, CultureInfo? culture = null, params object[] args) =>
                TryGetValue(key, out value, culture, args);
        }

        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange
            var mockProvider = new Mock<ILocalizedValueProvider>();

            // Act
            var accessor = new TestScopedTemplateAccessor(mockProvider.Object);

            // Assert
            accessor.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_SetsScopeNameToEnumName()
        {
            // Arrange
            var mockProvider = new Mock<ILocalizedValueProvider>();

            // Act
            var accessor = new TestScopedTemplateAccessor(mockProvider.Object);

            // Assert
            var scopeNameField = typeof(ScopedTemplateAccessor<TestScope>)
                .GetField("_scopeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var scopeName = scopeNameField?.GetValue(accessor);
            scopeName.Should().Be("TestScope");
        }

        [Theory]
        [InlineData(TestScope.Key1, "Hello, {0}!", "World", "Hello, World!")]
        [InlineData(TestScope.Key2, "Value: {0:N2}", 1234.56, "Value: 1,234.56")]
        public void GetValue_ReturnsCorrectValue(TestScope key, string template, object arg, string expected)
        {
            // Arrange
            var mockProvider = new Mock<ILocalizedValueProvider>();
            mockProvider.Setup(p => p.Resolve("TestScope", key.ToString(), It.IsAny<object[]>()))
                .Returns((string scope, string key, object[] args) => string.Format(CultureInfo.InvariantCulture, template, args));
            var accessor = new TestScopedTemplateAccessor(mockProvider.Object);

            // Act
            var result = accessor.PublicGetValue(key, arg);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(TestScope.Key1, "Value: {0:N2}", "en-US", 1234.56, "Value: 1,234.56")]
        [InlineData(TestScope.Key2, "Value: {0:N2}", "fr-FR", 1234.56, "Value: 1 234,56")]
        public void GetValue_FormatsValueCorrectlyForDifferentCultures(TestScope key, string template, string cultureName, double value, string expected)
        {
            // Arrange
            var mockProvider = new Mock<ILocalizedValueProvider>();
            mockProvider.Setup(p => p.Resolve("TestScope", key.ToString(), It.IsAny<CultureInfo>(), It.IsAny<object[]>()))
                .Returns((string scope, string key, CultureInfo culture, object[] args) =>
                    string.Format(culture, template, args));
            var accessor = new TestScopedTemplateAccessor(mockProvider.Object);

            // Act
            var result = accessor.PublicGetValue(key, new CultureInfo(cultureName), value);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(TestScope.Key1, "Hello, {0}!", "World", true, "Hello, World!")]
        [InlineData(TestScope.Key2, null, null, false, "")]
        public void TryGetValue_ReturnsExpectedResult(TestScope key, string? template, object? arg, bool expectedResult, string expectedValue)
        {
            // Arrange
            var mockProvider = new Mock<ILocalizedValueProvider>();
            mockProvider.Setup(p => p.TryResolve("TestScope", key.ToString(), out It.Ref<string>.IsAny, It.IsAny<CultureInfo>(), It.IsAny<object[]>()))
                .Callback((string scope, string key, out string value, CultureInfo? culture, object[] args) =>
                {
                    value = template != null ? string.Format(culture ?? CultureInfo.CurrentCulture, template, args) : string.Empty;
                })
                .Returns(expectedResult);

            var accessor = new TestScopedTemplateAccessor(mockProvider.Object);

            // Act
            var result = accessor.PublicTryGetValue(key, out var value, null, arg ?? Array.Empty<object>());

            // Assert
            result.Should().Be(expectedResult);
            value.Should().Be(expectedValue);
        }

        [Fact]
        public void GetValue_ThrowsException_WhenKeyIsInvalid()
        {
            // Arrange
            var mockProvider = new Mock<ILocalizedValueProvider>();
            var accessor = new TestScopedTemplateAccessor(mockProvider.Object);

            // Act
            var act = () => accessor.PublicGetValue((TestScope)999);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TryGetValue_ThrowsException_WhenKeyIsInvalid()
        {
            // Arrange
            var mockProvider = new Mock<ILocalizedValueProvider>();
            var accessor = new TestScopedTemplateAccessor(mockProvider.Object);

            // Act
            var act = () => accessor.PublicTryGetValue((TestScope)999, out _);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("The key '999' is not a valid value of the enumeration 'TestScope'.*");
        }

        [Fact]
        public void GetValue_ThrowsException_WhenCultureIsNull()
        {
            // Arrange
            var mockProvider = new Mock<ILocalizedValueProvider>();
            var accessor = new TestScopedTemplateAccessor(mockProvider.Object);

            // Act
            var act = () => accessor.PublicGetValue(TestScope.Key1, null!, "World");

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
