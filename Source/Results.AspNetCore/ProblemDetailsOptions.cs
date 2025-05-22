using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zentient.Results.AspNetCore
{
    /// <summary>
    /// Configuration options for generating ProblemDetails responses for failed Zentient.Results.IResult instances in ASP.NET Core.
    /// </summary>
    public class ProblemDetailsOptions
    {
        /// <summary>
        /// Gets or sets the default problem type URI (<a href="https://datatracker.ietf.org/doc/html/rfc9457#section-3.1">RFC 9457, Section 3.1</a>)
        /// to include in generated <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/>.
        /// </summary>
        public string? DefaultProblemType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically include the <see cref="Microsoft.AspNetCore.Http.HttpContext.TraceIdentifier"/>
        /// in the <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails.Extensions"/> dictionary with the key "traceId".
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool IncludeTraceId { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to automatically map errors contained within a failed
        /// <see cref="Zentient.Results.IResult.Errors"/> collection to the <see cref="Microsoft.AspNetCore.Mvc.ValidationProblemDetails.Errors"/>
        /// dictionary when generating a <see cref="Microsoft.AspNetCore.Mvc.ValidationProblemDetails"/>.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool MapValidationErrors { get; set; } = false;

        /// <summary>
        /// Gets or sets the default <c>title</c> (<a href="https://datatracker.ietf.org/doc/html/rfc9457#section-3.1">RFC 9457, Section 3.1</a>)
        /// to use for <see cref="Microsoft.AspNetCore.Mvc.ValidationProblemDetails"/> when validation errors are present.
        /// Defaults to "One or more validation errors occurred."
        /// </summary>
        public string ValidationProblemTitle { get; set; } = "One or more validation errors occurred.";

        /// <summary>
        /// Gets the dictionary that maps HTTP status codes to default <c>title</c> values (<a href="https://datatracker.ietf.org/doc/html/rfc9457#section-3.1">RFC 9457, Section 3.1</a>)
        /// for <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/>. This allows overriding the default descriptions provided by <see cref="Microsoft.AspNetCore.Http.StatusCodes"/>.
        /// </summary>
        public Dictionary<int, string> StatusCodeTitleMap { get; set; } = new Dictionary<int, string>();

        /// <summary>
        /// Gets or sets a custom factory delegate that allows for complete control over the creation of
        /// <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> instances. If set, this delegate will be invoked
        /// instead of the default logic when a failed <see cref="Zentient.Results.IResult"/> is encountered
        /// (and <see cref="MapValidationErrors"/> is <c>false</c> or there are no validation errors).
        /// </summary>
        public Func<HttpContext?, int?, string?, string?, string?, IDictionary<string, object>?, ProblemDetails>? CustomProblemDetailsFactory { get; set; }

        /// <summary>
        /// Gets or sets a custom factory delegate that allows for complete control over the creation of
        /// <see cref="Microsoft.AspNetCore.Mvc.ValidationProblemDetails"/> instances when validation errors are present
        /// in a failed <see cref="Zentient.Results.IResult.Errors"/> collection and <see cref="MapValidationErrors"/> is <c>true</c>.
        /// </summary>
        public Func<HttpContext?, IDictionary<string, string[]>?, int?, string?, string?, string?, IDictionary<string, object>?, ValidationProblemDetails>? CustomValidationProblemDetailsFactory { get; set; }
    }
}
