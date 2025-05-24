using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;
using System.Net;
using System.Net.Mime;

namespace Zentient.Results.AspNetCore
{
    /// <summary>
    /// Provides extension methods to convert <see cref="IResult"/> and <see cref="IResult{T}"/>
    /// to <see cref="IActionResult"/> for ASP.NET Core controllers.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Converts an <see cref="IResult"/> into an appropriate <see cref="IActionResult"/>.
        /// Success results will typically return <see cref="OkResult"/> for <see cref="StatusCodes.Status200OK"/>,
        /// <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status201Created"/>,
        /// <see cref="AcceptedResult"/> for <see cref="StatusCodes.Status202Accepted"/>, or
        /// <see cref="NoContentResult"/> for <see cref="StatusCodes.Status204NoContent"/>.
        /// Failure results will return <see cref="ObjectResult"/> with the corresponding HTTP status code
        /// and an <see cref="ApiErrorResponse"/> containing error details.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> to convert.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result's outcome.</returns>
        public static IActionResult ToActionResult(this IResult result)
        {
            if (result.IsSuccess)
            {
                return result.Status.Code switch
                {
                    StatusCodes.Status200OK => new OkResult(),
                    StatusCodes.Status201Created => new StatusCodeResult(StatusCodes.Status201Created),
                    StatusCodes.Status202Accepted => new AcceptedResult(),
                    StatusCodes.Status204NoContent => new NoContentResult(),
                    _ => new OkResult()
                };
            }

            // For failures, return an ObjectResult with the status code and error payload
            return new ObjectResult(new ApiErrorResponse(result.Errors, result.Messages, result.Status))
            {
                StatusCode = result.Status.ToHttpStatusCode(),
                ContentTypes = { MediaTypeNames.Application.Json }
            };
        }

        /// <summary>
        /// Converts an <see cref="IResult{T}"/> into an appropriate <see cref="IActionResult"/>.
        /// Success results will return <see cref="OkObjectResult"/> with the <see cref="IResult{T}.Value"/>.
        /// For <see cref="StatusCodes.Status201Created"/> and <see cref="StatusCodes.Status202Accepted"/>,
        /// it returns <see cref="CreatedResult"/> and <see cref="AcceptedResult"/> respectively, with the value.
        /// <see cref="StatusCodes.Status204NoContent"/> results in a <see cref="NoContentResult"/>.
        /// Failure results will return <see cref="ObjectResult"/> with the corresponding HTTP status code
        /// and an <see cref="ApiErrorResponse"/> containing error details.
        /// </summary>
        /// <typeparam name="T">The type of the value in the result.</typeparam>
        /// <param name="result">The <see cref="IResult{T}"/> to convert.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result's outcome.</returns>
        public static IActionResult ToActionResult<T>(this IResult<T> result)
        {
            if (result.IsSuccess)
            {
                return result.Status.Code switch
                {
                    StatusCodes.Status200OK => new OkObjectResult(result.Value),
                    StatusCodes.Status201Created => new CreatedResult(string.Empty, result.Value),
                    StatusCodes.Status202Accepted => new AcceptedResult(string.Empty, result.Value),
                    StatusCodes.Status204NoContent => new NoContentResult(),
                    _ => new OkObjectResult(result.Value)
                };
            }

            return new ObjectResult(new ApiErrorResponse(result.Errors, result.Messages, result.Status))
            {
                StatusCode = result.Status.ToHttpStatusCode(),
                ContentTypes = { MediaTypeNames.Application.Json }
            };
        }

        /// <summary>
        /// Converts an asynchronous <see cref="Task{TResult}"/> that resolves to an <see cref="IResult{T}"/>
        /// into an appropriate <see cref="IActionResult"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value in the result.</typeparam>
        /// <param name="taskResult">A <see cref="Task{TResult}"/> that resolves to an <see cref="IResult{T}"/>.</param>
        /// <returns>An asynchronous <see cref="Task{IActionResult}"/> representing the result's outcome.</returns>
        public static async Task<IActionResult> ToActionResultAsync<T>(this Task<IResult<T>> taskResult)
        {
            var result = await taskResult;
            return result.ToActionResult();
        }

        /// <summary>
        /// Converts an asynchronous <see cref="Task{TResult}"/> that resolves to an <see cref="IResult"/>
        /// into an appropriate <see cref="IActionResult"/>.
        /// </summary>
        /// <param name="taskResult">A <see cref="Task{TResult}"/> that resolves to an <see cref="IResult"/>.</param>
        /// <returns>An asynchronous <see cref="Task{IActionResult}"/> representing the result's outcome.</returns>
        public static async Task<IActionResult> ToActionResultAsync(this Task<IResult> taskResult)
        {
            var result = await taskResult;
            return result.ToActionResult();
        }

        /// <summary>
        /// A private DTO for formatting API error responses.
        /// </summary>
        private class ApiErrorResponse
        {
            /// <summary>
            /// Gets the HTTP status code of the error.
            /// </summary>
            public int StatusCode { get; }

            /// <summary>
            /// Gets the description of the status code.
            /// </summary>
            public string StatusDescription { get; }

            /// <summary>
            /// Gets the list of messages associated with the error.
            /// </summary>
            public IReadOnlyList<string> Messages { get; }

            /// <summary>
            /// Gets the list of detailed error information.
            /// </summary>
            public IReadOnlyList<ErrorInfo> Errors { get; }

            /// <summary>
            /// Gets the trace identifier for the current request.
            /// </summary>
            public string TraceId { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ApiErrorResponse"/> class.
            /// </summary>
            /// <param name="errors">The list of error information.</param>
            /// <param name="messages">The list of messages.</param>
            /// <param name="status">The status of the result.</param>
            public ApiErrorResponse(IReadOnlyList<ErrorInfo> errors, IReadOnlyList<string> messages, IResultStatus status)
            {
                StatusCode = status.ToHttpStatusCode();
                StatusDescription = status.Description;
                Messages = messages;
                Errors = errors;
                TraceId = Activity.Current?.Id ?? string.Empty;
            }
        }
        /// <summary>
        /// Gets the most appropriate HTTP status code based on the result's error categories.
        /// Defaults to 500 Internal Server Error if no specific category matches.
        /// </summary>
        /// <param name="result">The IResult instance.</param>
        /// <returns>An HttpStatusCode value.</returns>
        public static HttpStatusCode ToHttpStatusCode(this IResult result)
        {
            if (result.IsSuccess) return HttpStatusCode.OK;

            var firstErrorCategory = result.Errors?.FirstOrDefault().Category;

            return firstErrorCategory switch
            {
                ErrorCategory.NotFound => HttpStatusCode.NotFound,
                ErrorCategory.Validation => HttpStatusCode.BadRequest,
                ErrorCategory.Conflict => HttpStatusCode.Conflict,
                ErrorCategory.Unauthorized => HttpStatusCode.Unauthorized,
                ErrorCategory.Forbidden => HttpStatusCode.Forbidden,
                ErrorCategory.Authentication => HttpStatusCode.Unauthorized,
                ErrorCategory.Concurrency => HttpStatusCode.Conflict,
                ErrorCategory.TooManyRequests => (HttpStatusCode)429,
                ErrorCategory.ExternalService => HttpStatusCode.ServiceUnavailable,
                _ => HttpStatusCode.InternalServerError
            };
        }
    }
}
