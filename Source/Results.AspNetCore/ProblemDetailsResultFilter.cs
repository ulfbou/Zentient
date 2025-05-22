using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

using System.Net.Mime;

using Zentient.Results;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Zentient.Results.AspNetCore
{
    /// <summary>
    /// An ASP.NET Core <see cref="IResultFilter"/> that automatically transforms failed
    /// <see cref="Zentient.Results.IResult"/> instances returned from controller actions or Minimal API handlers
    /// into <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/> responses
    /// according to RFC 9457.
    /// </summary>
    public class ProblemDetailsResultFilter : IResultFilter
    {
        private readonly ProblemDetailsOptions _options;

        /// <summary>Initializes a new instance of the <see cref="ProblemDetailsResultFilter"/> class.</summary>
        /// <param name="options">The configuration options for ProblemDetails generation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is <c>null</c>.</exception>
        public ProblemDetailsResultFilter(IOptions<ProblemDetailsOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value is Zentient.Results.IResult result && result.IsFailure)
            {
                ProblemDetails problemDetails;
                int statusCode = result.Status.ToHttpStatusCode();
                string title = _options.StatusCodeTitleMap.TryGetValue(statusCode, out var customTitle) ? customTitle : result.Status.Description;
                string? type = _options.DefaultProblemType;
                string? instance = context.HttpContext.Request.Path;
                IDictionary<string, object>? extensions = null;
                string? detail = result.Error;

                if (_options.CustomProblemDetailsFactory != null)
                {
                    problemDetails = _options.CustomProblemDetailsFactory(context.HttpContext, statusCode, title, detail, type, extensions);
                }
                else if (_options.MapValidationErrors && result.Errors.Any())
                {
                    var errorsDictionary = new Dictionary<string, string[]>();
                    foreach (var errorInfo in result.Errors)
                    {
                        errorsDictionary.AddModelError(errorInfo.Code, errorInfo.Message);
                    }

                    if (_options.CustomValidationProblemDetailsFactory != null)
                    {
                        problemDetails = _options.CustomValidationProblemDetailsFactory(context.HttpContext, errorsDictionary, statusCode, title, detail, type, extensions);
                    }
                    else
                    {
                        problemDetails = new ValidationProblemDetails(errorsDictionary)
                        {
                            Type = type,
                            Title = _options.ValidationProblemTitle,
                            Status = statusCode,
                            Detail = detail,
                            Instance = instance
                        };
                        if (extensions != null)
                        {
                            foreach (var kvp in extensions)
                            {
                                problemDetails.Extensions[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }
                else
                {
                    problemDetails = new ProblemDetails
                    {
                        Type = type,
                        Title = title,
                        Status = statusCode,
                        Detail = detail,
                        Instance = instance
                    };
                    if (extensions != null)
                    {
                        foreach (var kvp in extensions)
                        {
                            problemDetails.Extensions[kvp.Key] = kvp.Value;
                        }
                    }
                }

                if (_options.IncludeTraceId)
                {
                    problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                }

                context.Result = new ObjectResult(problemDetails)
                {
                    StatusCode = statusCode,
                    ContentTypes = { "application/problem+json" }
                };

                context.HttpContext.Response.ContentType = "application/problem+json";
            }
        }

        /// <inheritdoc />
        public void OnResultExecuted(ResultExecutedContext context)
        {
            // No action needed after the result is executed.
        }
    }
}
