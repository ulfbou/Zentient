using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;


using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

using System;
using System.Linq;

using Zentient.Results;

namespace Zentient.Results.AspNetCore
{
    /// <summary>Provides extension methods to configuring Zentient.Results ProblemDetails integration for ASP.NET Core MVC.</summary>
    public static class MvcOptionsExtensions
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
            // Add the ProblemDetailsResultFilter to the MVC pipeline
            // Create new api behavior options if not already configured
            var apiBehaviorOptions = options.Filters.OfType<ApiBehaviorOptions>().FirstOrDefault()
                ?? new ApiBehaviorOptions();

            options.Filters.Add(new ProblemDetailsResultFilter(
                        new DefaultProblemDetailsFactory(
                            Options.Create(apiBehaviorOptions),
                            Options.Create(problemDetailsOptions)),
                        Options.Create(problemDetailsOptions)
                    ));
            return options;
        }
    }
}
