using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Zentient.Results.AspNetCore.Filters
{
    /// <summary>
    /// An ASP.NET Core result filter that converts <see cref="Zentient.Results.IResult"/> and <see cref="Zentient.Results.IResult{T}"/>
    /// returned from controller actions into appropriate <see cref="IActionResult"/> types.
    /// On failure, it generates <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/> responses.
    /// </summary>
    public class ProblemDetailsResultFilter : IAsyncResultFilter
    {
        private readonly ProblemDetailsFactory _problemDetailsFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailsResultFilter"/> class.
        /// </summary>
        /// <param name="problemDetailsFactory">The factory used to create <see cref="ProblemDetails"/> instances.</param>
        public ProblemDetailsResultFilter(ProblemDetailsFactory problemDetailsFactory)
        {
            _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
        }

        /// <summary>
        /// Executes the result filter asynchronously. Converts <see cref="Zentient.Results.IResult"/> values
        /// in the action result to appropriate <see cref="IActionResult"/> types, including <see cref="ProblemDetails"/> for failures.
        /// </summary>
        /// <param name="context">The <see cref="ResultExecutingContext"/> for the current request.</param>
        /// <param name="next">The delegate to execute the next filter or result.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value is Zentient.Results.IResult zentientResult)
            {
                context.Result = ConvertZentientResultToActionResult(zentientResult, context.HttpContext);
            }
            // Handles Task<object> that resolves to IResult (e.g., async controller actions)
            else if (context.Result is ObjectResult taskObjectResult && taskObjectResult.Value is Task<object> taskValue && taskValue.Result is Zentient.Results.IResult zTaskResult)
            {
                context.Result = ConvertZentientResultToActionResult(zTaskResult, context.HttpContext);
            }

            await next();
        }

        /// <summary>
        /// Converts a <see cref="Zentient.Results.IResult"/> to an appropriate <see cref="IActionResult"/>.
        /// On success, returns an <see cref="OkObjectResult"/> or <see cref="StatusCodeResult"/>.
        /// On failure, returns an <see cref="ObjectResult"/> with <see cref="ProblemDetails"/> or <see cref="UnprocessableEntityObjectResult"/> for validation errors.
        /// </summary>
        /// <param name="zentientResult">The Zentient result to convert.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result.</returns>
        private IActionResult ConvertZentientResultToActionResult(Zentient.Results.IResult zentientResult, HttpContext httpContext)
        {
            if (zentientResult.IsSuccess)
            {
                if (zentientResult is Zentient.Results.IResult<object> successResultWithObject)
                {
                    return new OkObjectResult(successResultWithObject.Value) { StatusCode = zentientResult.Status.ToHttpStatusCode() };
                }

                return new StatusCodeResult(zentientResult.Status.ToHttpStatusCode());
            }
            else
            {
                var problemDetails = zentientResult.ToProblemDetails(_problemDetailsFactory, httpContext);

                if (problemDetails is ValidationProblemDetails validationProblemDetails)
                {
                    return new UnprocessableEntityObjectResult(validationProblemDetails);
                }

                return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError };
            }
        }
    }
}
