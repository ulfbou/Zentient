using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Net;

using Zentient.Results.AspNetCore;

using Zentient.Results;
using Zentient.Results.AspNetCore.Filters;

namespace Zentient.Results.AspNetCore
{
    public static class ZentientResultsAspNetCoreExtensions
    {
        /// <summary>
        /// Adds Zentient.Results ASP.NET Core integration services to the specified <see cref="IServiceCollection"/>.
        /// This includes:
        /// <list type="bullet">
        ///     <item>Configuring <see cref="ProblemDetailsFactory"/>.</item>
        ///     <item>Overriding default API behavior for model state validation to use <see cref="ProblemDetails"/>.</item>
        ///     <item>Adding a global <see cref="ProblemDetailsResultFilter"/> for MVC controllers.</item>
        ///     <item>Registering <see cref="ZentientResultEndpointFilter"/> for Minimal APIs (must be applied to endpoints).</item>
        ///     <item>Configuring global <see cref="Microsoft.AspNetCore.Mvc.ProblemDetailsOptions"/> (e.g., adding traceId).</item>
        /// </list>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configureProblemDetails">An optional action to configure <see cref="Microsoft.AspNetCore.Mvc.ProblemDetailsOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> with Zentient.Results services added.</returns>
        public static IServiceCollection AddZentientResultsAspNetCore(this IServiceCollection services,
            Action<ProblemDetailsOptions>? configureProblemDetails = null)
        {
            // Ensure MVC services are added if not already (they register ProblemDetailsFactory and ApiBehaviorOptions)
            // It's generally good practice to let the user call AddControllers/AddControllersWithViews etc.
            // If you want to ensure it, you can add a check or conditionally add it.
            // services.AddControllers(); // Or similar, depending on app type.

            // 1. Register ProblemDetailsFactory explicitly (it's a core dependency)
            services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();

            // 2. Register IHttpContextAccessor (for cases where HttpContext is needed outside of direct request context, though filters get it)
            services.AddHttpContextAccessor(); // Provided by Microsoft.AspNetCore.Http

            // 3. Configure ApiBehaviorOptions to use Zentient.Results for InvalidModelStateResponseFactory
            services.PostConfigure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Keys
                        .Where(key => context.ModelState[key] != null)
                        .SelectMany(key => context.ModelState[key]!.Errors.Select(x =>
                            new ErrorInfo(ErrorCategory.Validation, key, x.ErrorMessage, Data: key)))
                        .ToList();

                    var result = Result.Validation(errors);

                    // Resolve ProblemDetailsFactory from the current request scope
                    var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                    // Pass the factory and httpContext to ToProblemDetails
                    var problemDetails = result.ToProblemDetails(problemDetailsFactory, context.HttpContext);

                    return new ObjectResult(problemDetails)
                    {
                        StatusCode = problemDetails.Status ?? (int)HttpStatusCode.BadRequest,
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            // 4. Configure Microsoft.AspNetCore.Mvc.ProblemDetailsOptions for global customizations
            services.PostConfigure<ProblemDetailsOptions>(options =>
            {
                // Apply any custom configuration provided by the consumer
                configureProblemDetails?.Invoke(options);

                // Add a default CustomizeProblemDetails if none is already set or if it's not overriding existing logic
                var originalCustomize = options.CustomizeProblemDetails;
                options.CustomizeProblemDetails = context =>
                {
                    originalCustomize?.Invoke(context); // Call original customization
                    if (!context.ProblemDetails.Extensions.ContainsKey("traceId"))
                    {
                        context.ProblemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                    }
                };
            });

            // 5. Register and add the global ProblemDetailsResultFilter for MVC
            services.AddScoped<ProblemDetailsResultFilter>(); // Use Scoped for filters
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.AddService<ProblemDetailsResultFilter>(); // Tells MVC to resolve from DI
            });

            // 6. Register ZentientResultEndpointFilter for Minimal APIs (needs to be explicitly applied to endpoints)
            services.AddScoped<ZentientResultEndpointFilter>();

            return services;
        }
    }
}
