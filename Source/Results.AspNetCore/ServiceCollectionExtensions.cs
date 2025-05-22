using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;

using Zentient.Results.AspNetCore;

namespace Zentient.Results.AspNetCore
{
    /// <summary>
    /// Provides extension methods for configuring Zentient.Results ProblemDetails integration for ASP.NET Core MVC.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a global <see cref="ProblemDetailsResultFilter"/> to the MVC options pipeline.
        /// This filter automatically converts failed <see cref="Zentient.Results.IResult"/> instances
        /// returned from controller actions into <see cref="ProblemDetails"/> responses.
        /// </summary>
        /// <param name="options">The <see cref="MvcOptions"/> to configure.</param>
        /// <param name="configure">An optional action to configure the <see cref="ProblemDetailsOptions"/>.</param>
        /// <returns>The <see cref="MvcOptions"/> instance so that additional configuration can be chained.</returns>
        public static MvcOptions AddZentientResultProblemDetails(this MvcOptions options, Action<ProblemDetailsOptions>? configure = null)
        {
            var problemDetailsOptions = new ProblemDetailsOptions();
            configure?.Invoke(problemDetailsOptions);
            options.Filters.Add(new ProblemDetailsResultFilter(Options.Create(problemDetailsOptions)));
            return options;
        }
    }
}
