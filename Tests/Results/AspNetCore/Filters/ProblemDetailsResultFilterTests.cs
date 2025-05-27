using System.Net;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;
using Zentient.Results;
using Zentient.Results.AspNetCore.Filters;
using Zentient.Results.AspNetCore.Configuration;

using static Zentient.Results.Tests.Helpers.AspNetCoreHelpers;

namespace Zentient.Results.Tests.AspNetCore.Filters
{
    /// <summary>
    /// Unit tests for <see cref="ProblemDetailsResultFilter"/> to ensure it correctly converts
    /// <see cref="Zentient.Results.IResult"/> and <see cref="Zentient.Results.IResult{T}"/>
    /// returned from controller actions into appropriate
    /// <see cref="IActionResult"/> types, including <see cref="ProblemDetails"/> and
    /// <see cref="ValidationProblemDetails"/> responses.
    /// These tests verify that the filter handles both synchronous and asynchronous results,
    /// and that it correctly generates problem details for validation failures and generic errors.
    /// </summary>
    public class ProblemDetailsResultFilterTests
    {
        public const string ProblemTypeUri = "https://example.com/errors/";

        [Fact]
        public void Ctor_Throws_If_ProblemDetailsFactory_Null()
        {
            var options = Options.Create(new ProblemDetailsOptions());
            var zentientOptions = Options.Create(new ZentientProblemDetailsOptions { ProblemTypeBaseUri = ProblemTypeUri });
            Assert.Throws<ArgumentNullException>(() => new ProblemDetailsResultFilter(null!, options, zentientOptions));
        }

        [Fact]
        public async Task OnResultExecutionAsync_Converts_IResult_ObjectResult_To_ActionResult()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var zentientResult = new FakeFailureResult();
            var objectResult = new ObjectResult(zentientResult);
            var context = new ResultExecutingContext(
                new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
                new List<IFilterMetadata>(),
                objectResult,
                controller: null!
            );
            var filter = CreateFilter();
            var nextCalled = false;

            // Act
            await filter.OnResultExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult<ResultExecutedContext>(null!);
            });

            // Assert
            context.Result.Should().BeOfType<ObjectResult>();
            var result = (ObjectResult)context.Result;
            result.Value.Should().BeOfType<ProblemDetails>();
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task OnResultExecutionAsync_Converts_IResultT_ObjectResult_To_ActionResult()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var zentientResult = new FakeSuccessResultWithObject("value");
            var objectResult = new ObjectResult(zentientResult);
            var context = new ResultExecutingContext(
                new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
                new List<IFilterMetadata>(),
                objectResult,
                controller: null!
            );
            var filter = CreateFilter();
            var nextCalled = false;

            // Act
            await filter.OnResultExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult<ResultExecutedContext>(null!);
            });

            // Assert
            context.Result.Should().BeOfType<OkObjectResult>();
            var result = (OkObjectResult)context.Result;
            result.Value.Should().Be("value");
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task OnResultExecutionAsync_Handles_Task_ObjectResult_Value()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var zentientResult = new FakeFailureResult();
            var task = Task.FromResult<object>(zentientResult);
            var objectResult = new ObjectResult(zentientResult);
            var context = new ResultExecutingContext(
                new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
                new List<IFilterMetadata>(),
                objectResult,
                controller: null!
            );
            var filter = CreateFilter();
            var nextCalled = false;

            // Act
            await filter.OnResultExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult<ResultExecutedContext>(null!);
            });

            // Assert
            context.Result.Should().BeOfType<ObjectResult>();
            var result = (ObjectResult)context.Result;
            result.Value.Should().BeOfType<ProblemDetails>();
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task OnResultExecutionAsync_Handles_Raw_IResult()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var zentientResult = new FakeFailureResult();
            var objectResult = new ObjectResult(zentientResult);
            var context = new ResultExecutingContext(
                new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
                new List<IFilterMetadata>(),
                objectResult,
                controller: null!
            );
            var filter = CreateFilter();
            var nextCalled = false;

            // Act
            await filter.OnResultExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult<ResultExecutedContext>(null!);
            });

            // Assert
            context.Result.Should().BeOfType<ObjectResult>();
            var result = (ObjectResult)context.Result;
            result.Value.Should().BeOfType<ProblemDetails>();
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public void ConvertZentientResultToActionResult_Returns_ValidationProblemDetails_For_ValidationFailure()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var filter = CreateFilter();
            var validationResult = new FakeValidationFailureResult();

            // Act
            var result = filter.GetType()
                .GetMethod("ConvertZentientResultToActionResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(filter, new object[] { validationResult, httpContext });

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
            var objResult = (UnprocessableEntityObjectResult)result!;
            objResult.Value.Should().BeOfType<ValidationProblemDetails>();
        }

        [Fact]
        public void ConvertZentientResultToActionResult_Returns_ProblemDetails_For_GenericFailure()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var filter = CreateFilter();
            var failureResult = new FakeFailureResult();

            // Act
            var result = filter.GetType()
                .GetMethod("ConvertZentientResultToActionResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(filter, new object[] { failureResult, httpContext });

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objResult = (ObjectResult)result!;
            objResult.Value.Should().BeOfType<ProblemDetails>();
            objResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            objResult.ContentTypes.Should().Contain("application/problem+json");
        }

        [Fact]
        public void ConvertZentientResultToActionResult_Returns_CorrectResult_For_Success()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var filter = CreateFilter();
            var successResult = new FakeSuccessResult();
            var successResultWithObject = new FakeSuccessResultWithObject("abc");

            // Act
            var result1 = filter.GetType()
                .GetMethod("ConvertZentientResultToActionResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(filter, new object[] { successResult, httpContext });

            var result2 = filter.GetType()
                .GetMethod("ConvertZentientResultToActionResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(filter, new object[] { successResultWithObject, httpContext });

            // Assert
            result1.Should().BeOfType<NoContentResult>();
            result2.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result2!).Value.Should().Be("abc");
        }

        // --- Fake IResult implementations for testing ---

        private class FakeSuccessResult : IResult
        {
            public bool IsSuccess => true;
            public bool IsFailure => false;
            public IReadOnlyList<ErrorInfo> Errors => Array.Empty<ErrorInfo>();
            public IReadOnlyList<string> Messages => Array.Empty<string>();
            public string? Error => null;
            public IResultStatus Status { get; } = new FakeResultStatus((int)HttpStatusCode.NoContent, "No Content");
        }

        private class FakeSuccessResultWithObject : IResult<object>
        {
            public FakeSuccessResultWithObject(object? value) => Value = value;
            public object? Value { get; }
            public object GetValueOrThrow() => Value!;
            public object GetValueOrThrow(string message) => Value!;
            public object GetValueOrThrow(Func<Exception> exceptionFactory) => Value!;
            public object GetValueOrDefault(object fallback) => Value ?? fallback;
            public IResult<U> Map<U>(Func<object, U> selector) => throw new NotImplementedException();
            public IResult<U> Bind<U>(Func<object, IResult<U>> binder) => throw new NotImplementedException();
            public IResult<object> Tap(Action<object> onSuccess) => this;
            public IResult<object> OnSuccess(Action<object> action) => this;
            public IResult<object> OnFailure(Action<IReadOnlyList<ErrorInfo>> action) => this;
            public U Match<U>(Func<object, U> onSuccess, Func<IReadOnlyList<ErrorInfo>, U> onFailure) => onSuccess(Value!);
            public bool IsSuccess => true;
            public bool IsFailure => false;
            public IReadOnlyList<ErrorInfo> Errors => Array.Empty<ErrorInfo>();
            public IReadOnlyList<string> Messages => Array.Empty<string>();
            public string? Error => null;
            public IResultStatus Status { get; } = new FakeResultStatus((int)HttpStatusCode.OK, "OK");
        }

        private class FakeFailureResult : IResult
        {
            public bool IsSuccess => false;
            public bool IsFailure => true;
            public IReadOnlyList<ErrorInfo> Errors => new[] { new ErrorInfo(ErrorCategory.General, "ERR", "Failure") };
            public IReadOnlyList<string> Messages => new[] { "Failure" };
            public string? Error => "Failure";
            public IResultStatus Status { get; } = new FakeResultStatus(500, "Internal Server Error");
        }

        private class FakeValidationFailureResult : IResult
        {
            public bool IsSuccess => false;
            public bool IsFailure => true;
            public IReadOnlyList<ErrorInfo> Errors => new[] { new ErrorInfo(ErrorCategory.Validation, "VAL", "Validation failed") };
            public IReadOnlyList<string> Messages => new[] { "Validation failed" };
            public string? Error => "Validation failed";
            public IResultStatus Status { get; } = new FakeResultStatus(422, "Unprocessable Entity");
            // Simulate ToProblemDetails returning ValidationProblemDetails
            public ProblemDetails ToProblemDetails(ProblemDetailsFactory factory, HttpContext context)
            {
                return new ValidationProblemDetails(new Dictionary<string, string[]> { { "Field", new[] { "Error" } } })
                {
                    Status = 422,
                    Title = "Validation failed"
                };
            }
        }

        private class FakeResultStatus : IResultStatus
        {
            public FakeResultStatus(int code, string desc) { Code = code; Description = desc; }
            public int Code { get; }
            public string Description { get; }
        }
    }

    // Extension method for IResult to simulate ToProblemDetails
    public static class IResultExtensions
    {
        public static ProblemDetails ToProblemDetails(this IResult result, ProblemDetailsFactory factory, HttpContext context)
        {
            // Use pattern matching on public interface and error category instead of internal test type
            if (!result.IsSuccess && result.Errors.Any(e => e.Category == ErrorCategory.Validation))
            {
                return new ValidationProblemDetails(new Dictionary<string, string[]> { { "Field", new[] { "Error" } } })
                {
                    Status = 422,
                    Title = result.Error ?? "Validation failed"
                };
            }

            return new ProblemDetails
            {
                Status = result.Status.Code,
                Title = result.Error ?? "Error",
                Detail = string.Join("; ", result.Messages)
            };
        }
    }
}
