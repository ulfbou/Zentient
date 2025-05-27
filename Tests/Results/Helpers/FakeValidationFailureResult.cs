using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Zentient.Results.Tests.Helpers
{
    internal class FakeValidationFailureResult : IResult
    {
        public bool IsSuccess => false;
        public bool IsFailure => true;
        public IReadOnlyList<ErrorInfo> Errors => new[] { new ErrorInfo(ErrorCategory.Validation, "VAL", "Validation failed") };
        public IReadOnlyList<string> Messages => new[] { "Validation failed" };
        public string? Error => "Validation failed";
        public IResultStatus Status { get; } = new FakeResultStatus(422, "Unprocessable Entity");

        public ProblemDetails ToProblemDetails(ProblemDetailsFactory factory, HttpContext context)
        {
            return new ValidationProblemDetails(new Dictionary<string, string[]> { { "Field", new[] { "Error" } } })
            {
                Status = 422,
                Title = "Validation failed"
            };
        }
    }
}
