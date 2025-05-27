using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Zentient.Results.Tests.Helpers
{
    internal static class IResultExtensions
    {
        public static ProblemDetails ToProblemDetails(this IResult result, ProblemDetailsFactory factory, HttpContext context)
        {
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
