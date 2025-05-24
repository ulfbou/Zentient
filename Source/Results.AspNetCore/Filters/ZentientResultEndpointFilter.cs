using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Zentient.Results;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Zentient.Results.AspNetCore.Filters
{
    /// <summary>
    /// An endpoint filter that automatically converts <see cref="Zentient.Results.IResult"/>
    /// and <see cref="Zentient.Results.IResult{T}"/> instances returned from Minimal API endpoints
    /// into appropriate <see cref="Microsoft.AspNetCore.Http.IResult"/> types, leveraging
    /// <see cref="ProblemDetails"/> for failure results.
    /// </summary>
    public class ZentientResultEndpointFilter : IEndpointFilter
    {
        /// <inheritdoc />
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var result = await next(context);

            if (result is not Zentient.Results.IResult zentientResult)
            {
                return result;
            }

            var httpContext = context.HttpContext;
            var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

            if (zentientResult.IsSuccess)
            {
                if (zentientResult is Zentient.Results.IResult<object> zResultGeneric)
                {
                    return zentientResult.Status.Code switch
                    {
                        (int)HttpStatusCode.OK => Microsoft.AspNetCore.Http.Results.Ok(zResultGeneric.Value),
                        (int)HttpStatusCode.Created => Microsoft.AspNetCore.Http.Results.StatusCode((int)HttpStatusCode.Created),
                        (int)HttpStatusCode.NoContent => Microsoft.AspNetCore.Http.Results.NoContent(),
                        _ => Microsoft.AspNetCore.Http.Results.StatusCode(zentientResult.Status.ToHttpStatusCode())
                    };
                }
                return zentientResult.Status.Code switch
                {
                    (int)HttpStatusCode.OK => Microsoft.AspNetCore.Http.Results.NoContent(), // Or Results.Ok() if you want a default object
                    (int)HttpStatusCode.Created => Microsoft.AspNetCore.Http.Results.StatusCode((int)HttpStatusCode.Created),
                    (int)HttpStatusCode.NoContent => Microsoft.AspNetCore.Http.Results.NoContent(),
                    _ => Microsoft.AspNetCore.Http.Results.StatusCode(zentientResult.Status.ToHttpStatusCode())
                };
            }
            // Here's where the factory and httpContext are passed to ToProblemDetails
            var problemDetails = zentientResult.ToProblemDetails(problemDetailsFactory, httpContext);
            return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
        }
    }
}
