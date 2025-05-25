using System.Net;
using FluentAssertions;
using Zentient.Results;
using Zentient.Results.AspNetCore;
using Xunit;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// using model binding and validation namespaces
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Zentient.Results.Tests.AspNetCore
{
    public class ProblemDetailsExtensionsTests
    {
        private static ProblemDetailsFactory CreateFactory()
        {
            var mock = new Mock<ProblemDetailsFactory>();
            mock.Setup(f => f.CreateProblemDetails(
                    It.IsAny<HttpContext>(),
                    It.IsAny<int?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns<HttpContext, int?, string, string, string, string>((ctx, status, title, type, detail, instance) =>
                    new ProblemDetails
                    {
                        Status = status,
                        Title = title,
                        Type = type,
                        Detail = detail,
                        Instance = instance
                    });

            mock.Setup(f => f.CreateValidationProblemDetails(
                    It.IsAny<HttpContext>(),
                    It.IsAny<ModelStateDictionary>(),
                    It.IsAny<int?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns((HttpContext ctx, ModelStateDictionary ms, int? status, string title, string type, string detail, string instance) =>
                    new ValidationProblemDetails(ms)
                    {
                        Status = status,
                        Title = title,
                        Type = type,
                        Detail = detail,
                        Instance = instance
                    });

            return mock.Object;
        }

        private static HttpContext CreateHttpContext(string path = "/test")
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            return context;
        }

        [Fact]
        public void ToProblemDetails_Throws_On_Success_Result()
        {
            // Arrange
            var result = new SuccessResultStub();
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            Action act = () => result.ToProblemDetails(factory, context);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ToProblemDetails_Creates_ProblemDetails_For_Failure()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Database, "DB-001", "Database error");
            var result = new FailureResultStub(new[] { error }, "Database error", new ResultStatusStub(500, "Internal Error"));
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context);

            // Assert
            pd.Should().NotBeNull();
            pd.Status.Should().Be(500);
            pd.Title.Should().Be("Internal Error");
            pd.Type.Should().Contain("DB-001");
            pd.Detail.Should().Be("Database error");
            pd.Instance.Should().Be("/test");
            pd.Extensions.Should().ContainKey("zentientErrors");
            var zentientErrors = pd.Extensions["zentientErrors"] as IEnumerable<object>;
            zentientErrors.Should().NotBeNull();
            zentientErrors.Should().ContainSingle();
        }

        [Fact]
        public void ToProblemDetails_Creates_ValidationProblemDetails_For_ValidationError()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Validation, "VAL-001", "Validation failed", "FieldA");
            var result = new FailureResultStub(new[] { error }, "Validation failed", new ResultStatusStub(400, "Bad Request"));
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context);

            // Assert
            pd.Should().BeOfType<ValidationProblemDetails>();
            pd.Status.Should().Be(400);
            pd.Title.Should().Be("Bad Request");
            pd.Type.Should().Contain("VAL-001");
            pd.Detail.Should().Be("Validation failed");
            pd.Instance.Should().Be("/test");
            pd.Extensions.Should().ContainKey("zentientErrors");
            var zentientErrors = pd.Extensions["zentientErrors"] as IEnumerable<object>;
            zentientErrors.Should().NotBeNull();
            zentientErrors.Should().ContainSingle();
        }

        [Fact]
        public void ToProblemDetails_Recursively_Handles_InnerErrors_And_Data()
        {
            // Arrange
            var leaf = new ErrorInfo(ErrorCategory.Request, "REQ-001", "Bad request", new { Field = "Leaf" });
            var mid = new ErrorInfo(ErrorCategory.Validation, "VAL-002", "Validation failed", new { Field = "Mid" }, new[] { leaf });
            var root = new ErrorInfo(ErrorCategory.Exception, "EX-001", "Exception occurred", new { Field = "Root" }, new[] { mid });
            var result = new FailureResultStub(new[] { root }, "Exception occurred", new ResultStatusStub(500, "Internal Error"));
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context);

            // Assert
            pd.Extensions.Should().ContainKey("zentientErrors");
            // Fix: Use IEnumerable<object> instead of List<object> for compatibility with the actual type
            var zentientErrors = pd.Extensions["zentientErrors"] as IEnumerable<object>;
            zentientErrors.Should().NotBeNull();
            zentientErrors.Should().HaveCount(1);

            var rootDict = zentientErrors!.First() as IDictionary<string, object?>;
            rootDict.Should().ContainKey("data");
            rootDict.Should().ContainKey("innerErrors");

            var midList = rootDict["innerErrors"] as IEnumerable<object>;
            midList.Should().NotBeNull();
            midList.Should().HaveCount(1);

            var midDict = midList!.First() as IDictionary<string, object?>;
            midDict.Should().ContainKey("data");
            midDict.Should().ContainKey("innerErrors");

            var leafList = midDict["innerErrors"] as IEnumerable<object>;
            leafList.Should().NotBeNull();
            leafList.Should().HaveCount(1);

            var leafDict = leafList!.First() as IDictionary<string, object?>;
            leafDict.Should().ContainKey("data");
            leafDict.Should().NotContainKey("innerErrors");
        }

        [Fact]
        public void ToProblemDetailsT_Delegates_To_NonGeneric()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Database, "DB-001", "Database error");
            var result = new FailureResultStubGeneric<string>(new[] { error }, "Database error", new ResultStatusStub(500, "Internal Error"));
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context);

            // Assert
            pd.Should().NotBeNull();
            pd.Status.Should().Be(500);
        }

        [Fact]
        public void ToHttpStatusCode_Maps_ErrorCategory()
        {
            // Arrange
            var categories = new Dictionary<ErrorCategory, HttpStatusCode>
            {
                { ErrorCategory.NotFound, HttpStatusCode.NotFound },
                { ErrorCategory.Validation, HttpStatusCode.BadRequest },
                { ErrorCategory.Conflict, HttpStatusCode.Conflict },
                { ErrorCategory.Authentication, HttpStatusCode.Unauthorized },
                { ErrorCategory.Network, HttpStatusCode.ServiceUnavailable },
                { ErrorCategory.Timeout, HttpStatusCode.RequestTimeout },
                { ErrorCategory.Security, HttpStatusCode.Forbidden },
                { ErrorCategory.Request, HttpStatusCode.BadRequest },
                { ErrorCategory.Database, HttpStatusCode.InternalServerError }
            };

            foreach (var kvp in categories)
            {
                var error = new ErrorInfo(kvp.Key, "CODE", "Message");
                var result = new FailureResultStub(new[] { error }, "Message", new ResultStatusStub(500, "Error"));
                var code = result.ToHttpStatusCode();
                code.Should().Be(kvp.Value);
            }
        }

        [Fact]
        public void ToHttpStatusCode_Returns_Ok_On_Success()
        {
            // Arrange
            var result = new SuccessResultStub();

            // Act
            var code = result.ToHttpStatusCode();

            // Assert
            code.Should().Be(HttpStatusCode.OK);
        }

        // --- Test Stubs ---

        private class ResultStatusStub : IResultStatus
        {
            public int Code { get; }
            public string Description { get; }
            public ResultStatusStub(int code, string description)
            {
                Code = code;
                Description = description;
            }
        }

        private class SuccessResultStub : IResult
        {
            public bool IsSuccess => true;
            public bool IsFailure => false;
            public IReadOnlyList<ErrorInfo> Errors => Array.Empty<ErrorInfo>();
            public IReadOnlyList<string> Messages => Array.Empty<string>();
            public string? Error => null;
            public IResultStatus Status { get; } = new ResultStatusStub(200, "OK");
        }

        private class FailureResultStub : IResult
        {
            public bool IsSuccess => false;
            public bool IsFailure => true;
            public IReadOnlyList<ErrorInfo> Errors { get; }
            public IReadOnlyList<string> Messages => Array.Empty<string>();
            public string? Error { get; }
            public IResultStatus Status { get; }
            public FailureResultStub(IReadOnlyList<ErrorInfo> errors, string? error, IResultStatus status)
            {
                Errors = errors;
                Error = error;
                Status = status;
            }
        }

        private class FailureResultStubGeneric<T> : IResult<T>
        {
            public bool IsSuccess => false;
            public bool IsFailure => true;
            public IReadOnlyList<ErrorInfo> Errors { get; }
            public IReadOnlyList<string> Messages => Array.Empty<string>();
            public string? Error { get; }
            public IResultStatus Status { get; }
            public T? Value => default;
            public FailureResultStubGeneric(IReadOnlyList<ErrorInfo> errors, string? error, IResultStatus status)
            {
                Errors = errors;
                Error = error;
                Status = status;
            }
            public T GetValueOrThrow() => throw new InvalidOperationException();
            public T GetValueOrThrow(string message) => throw new InvalidOperationException();
            public T GetValueOrThrow(Func<Exception> exceptionFactory) => throw new InvalidOperationException();
            public T GetValueOrDefault(T fallback) => fallback;
            public IResult<U> Map<U>(Func<T, U> selector) => throw new NotImplementedException();
            public IResult<U> Bind<U>(Func<T, IResult<U>> binder) => throw new NotImplementedException();
            public IResult<T> Tap(Action<T> onSuccess) => this;
            public IResult<T> OnSuccess(Action<T> action) => this;
            public IResult<T> OnFailure(Action<IReadOnlyList<ErrorInfo>> action) { action(Errors); return this; }
            public U Match<U>(Func<T, U> onSuccess, Func<IReadOnlyList<ErrorInfo>, U> onFailure) => onFailure(Errors);
        }
    }
}
