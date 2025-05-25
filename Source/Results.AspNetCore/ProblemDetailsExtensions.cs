using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure; // For ProblemDetailsFactory, DefaultProblemDetailsFactory
using Microsoft.AspNetCore.Mvc.ModelBinding; // For ModelStateDictionary
using Microsoft.Extensions.DependencyInjection; // For GetService
using Microsoft.Extensions.Options; // For IOptions
using Zentient.Results;
using System.Net; // For HttpStatusCode

namespace Zentient.Results.AspNetCore
{
    /// <summary>
    /// Provides extension methods for converting <see cref="Zentient.Results.IResult"/> instances
    /// into ASP.NET Core <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/> responses,
    /// adhering to RFC 7807.
    /// </summary>
    public static class ProblemDetailsExtensions
    {
        // Base URI for problem types. Customize this to reflect your API's documentation.
        private const string DefaultProblemTypeBaseUri = "https://yourdomain.com/errors/";

        /// <summary>
        /// Converts a failed <see cref="Zentient.Results.IResult"/> instance into an appropriate
        /// <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/> response.
        /// </summary>
        /// <param name="result">The <see cref="Zentient.Results.IResult"/> instance to convert.
        /// This method should only be called for failed results (<see cref="IResult.IsFailure"/> is true).</param>
        /// <param name="factory">The <see cref="ProblemDetailsFactory"/> instance, typically provided by the ASP.NET Core
        /// framework (e.g., injected into a filter or middleware).</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>, necessary for rich ProblemDetails generation
        /// (e.g., instance URI, trace ID, and custom problem details options).</param>
        /// <returns>A <see cref="ProblemDetails"/> instance representing the error.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the <paramref name="result"/> is a success result.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> or <paramref name="httpContext"/> is null.</exception>
        public static ProblemDetails ToProblemDetails(
                    this Zentient.Results.IResult result,
                    ProblemDetailsFactory factory,
                    HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(factory, nameof(factory));
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            if (result.IsSuccess)
            {
                throw new InvalidOperationException("Cannot convert a successful result to ProblemDetails.");
            }

            var firstError = result.Errors.FirstOrDefault();
            var statusCode = result.Status.ToHttpStatusCode();

            // Determine problem type, title, and detail based on the Zentient.Results.IResult properties
            string problemType = $"{DefaultProblemTypeBaseUri}{(firstError.Code ?? result.Status.Code.ToString())}";
            string problemTitle = result.Status.Description;
            string problemDetail = result.Error!;

            ProblemDetails problemDetails;

            if (statusCode == (int)HttpStatusCode.UnprocessableEntity || result.Errors.Any(e => e.Category == ErrorCategory.Validation))
            {
                var modelState = new ModelStateDictionary();
                foreach (var error in result.Errors.Where(e => e.Category == ErrorCategory.Validation))
                {
                    string key = error.Code;

                    if (string.IsNullOrWhiteSpace(key) && error.Data is string dataString && !string.IsNullOrWhiteSpace(dataString))
                    {
                        key = dataString;
                    }
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        key = "General";
                    }
                    modelState.AddModelError(key, error.Message);
                }

                problemDetails = factory.CreateValidationProblemDetails(
                    httpContext: httpContext,
                    modelStateDictionary: modelState,
                    statusCode: statusCode,
                    title: problemTitle,
                    type: problemType,
                    detail: problemDetail
                );
            }
            else
            {
                problemDetails = factory.CreateProblemDetails(
                                    httpContext: httpContext,
                                    statusCode: statusCode,
                                    title: problemTitle,
                                    type: problemType,
                                    detail: problemDetail
                                );
            }

            problemDetails.Status ??= statusCode;
            problemDetails.Title ??= problemTitle;
            problemDetails.Detail ??= problemDetail;
            problemDetails.Type ??= problemType;
            problemDetails.Instance ??= httpContext.Request.Path.Value;

            AddErrorInfoExtensions(problemDetails, result.Errors);

            return problemDetails;
        }

        /// <summary>
        /// Converts a failed <see cref="Zentient.Results.IResult{T}"/> instance into an appropriate
        /// <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/> response.
        /// This method simply delegates to the non-generic <see cref="ToProblemDetails(IResult, ProblemDetailsFactory, HttpContext)"/>.
        /// </summary>
        /// <typeparam name="T">The type of the success value (ignored for failure conversion).</typeparam>
        /// <param name="result">The <see cref="Zentient.Results.IResult{T}"/> instance to convert.</param>
        /// <param name="factory">The <see cref="ProblemDetailsFactory"/> instance.</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
        /// <returns>A <see cref="ProblemDetails"/> instance representing the error.</returns>
        public static ProblemDetails ToProblemDetails<T>(
                    this Zentient.Results.IResult<T> result,
                    ProblemDetailsFactory factory,
                    HttpContext httpContext)
        {
            return (result as Zentient.Results.IResult).ToProblemDetails(factory, httpContext);
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
                //ErrorCategory.Unauthorized => HttpStatusCode.Unauthorized,
                //ErrorCategory.Forbidden => HttpStatusCode.Forbidden,
                ErrorCategory.Authentication => HttpStatusCode.Unauthorized,
                //ErrorCategory.Concurrency => HttpStatusCode.Conflict,
                //ErrorCategory.TooManyRequests => (HttpStatusCode)429,
                //ErrorCategory.ExternalService => HttpStatusCode.ServiceUnavailable,
                ErrorCategory.Network => HttpStatusCode.ServiceUnavailable,
                ErrorCategory.Timeout => HttpStatusCode.RequestTimeout,
                ErrorCategory.Security => HttpStatusCode.Forbidden,
                ErrorCategory.Request => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };
        }

        /// <summary>
        /// Adds a custom extension property "zentientErrors" to the <see cref="ProblemDetails.Extensions"/> dictionary.
        /// This extension contains a structured, hierarchical list of detailed error information from the Zentient.Results,
        /// including recursive handling of inner errors.
        /// </summary>
        /// <param name="problemDetails">The <see cref="ProblemDetails"/> instance to extend.</param>
        /// <param name="errors">The list of <see cref="ErrorInfo"/> objects to be added as an extension.
        /// If this list is null or empty, no "zentientErrors" extension will be added.</param>
        private static void AddErrorInfoExtensions(ProblemDetails problemDetails, IReadOnlyList<ErrorInfo> errors)
        {
            // Defensive null check and empty check for the main errors list
            if (errors == null || !errors.Any())
            {
                return; // No errors to add, exit early
            }

            // Convert each ErrorInfo into a serializable dictionary representation
            problemDetails.Extensions["zentientErrors"] = errors.Select(e => ToErrorObject(e)).ToList();

            // Local static function to convert a single ErrorInfo into a dictionary for JSON serialization.
            // This function is recursive to handle nested inner errors.
            static Dictionary<string, object?> ToErrorObject(ErrorInfo error)
            {
                var errorObject = new Dictionary<string, object?>
                {
                    { "category", error.Category.ToString() },
                    { "code", error.Code },
                    { "message", error.Message }
                };

                // Add the 'data' property if it's not null.
                // IMPORTANT CONSIDERATION ADDRESSED:
                // The serialization of 'error.Data' (which is object?) will be handled by the configured JSON serializer
                // (e.g., System.Text.Json by default in ASP.NET Core).
                // It is critical that 'error.Data' contains types that are naturally JSON-serializable (primitives, strings,
                // simple collections, or Data Transfer Objects (DTOs) with public properties).
                // If 'error.Data' holds complex objects that are not designed for JSON serialization (e.g., objects with
                // circular references, private fields, or non-public properties), it might lead to serialization errors
                // or unexpected output in the JSON response.
                if (error.Data != null)
                {
                    errorObject["data"] = error.Data;
                }

                if (error.InnerErrors != null && error.InnerErrors.Any())
                {
                    errorObject["innerErrors"] = error.InnerErrors.Select(ie => ToErrorObject(ie)).ToList();
                }

                return errorObject;
            }
        }

        /// <summary>
        /// Converts the <see cref="IResultStatus"/> to an HTTP status code.
        /// This is a helper method to extract the status code from the result status.
        /// </summary>
        /// <param name="status">The <see cref="IResultStatus"/> instance containing the status code.</param>
        /// <returns>The HTTP status code as an integer.</returns>
        private static int ToHttpStatusCode(this IResultStatus status) => status.Code;
    }
}
