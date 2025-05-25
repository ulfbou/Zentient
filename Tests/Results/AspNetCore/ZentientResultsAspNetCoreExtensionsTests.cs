using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Zentient.Results.AspNetCore;
using Zentient.Results.AspNetCore.Filters;
using Xunit;

namespace Zentient.Results.Tests.AspNetCore
{
    public class ZentientResultsAspNetCoreExtensionsTests
    {
        [Fact]
        public void AddZentientResultsAspNetCore_Registers_ProblemDetailsFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddZentientResultsAspNetCore();
            var provider = services.BuildServiceProvider();

            // Assert
            provider.GetRequiredService<ProblemDetailsFactory>().Should().BeOfType<DefaultProblemDetailsFactory>();
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Registers_HttpContextAccessor()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddZentientResultsAspNetCore();
            var provider = services.BuildServiceProvider();

            // Assert
            provider.GetRequiredService<IHttpContextAccessor>().Should().NotBeNull();
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Registers_ProblemDetailsResultFilter()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddZentientResultsAspNetCore();
            var provider = services.BuildServiceProvider();

            // Assert
            provider.GetRequiredService<ProblemDetailsResultFilter>().Should().NotBeNull();
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Registers_ZentientResultEndpointFilter()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddZentientResultsAspNetCore();
            var provider = services.BuildServiceProvider();

            // Assert
            provider.GetRequiredService<ZentientResultEndpointFilter>().Should().NotBeNull();
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Configures_ProblemDetailsOptions_Customization()
        {
            // Arrange
            var services = new ServiceCollection();
            string? customKey = null;
            services.AddZentientResultsAspNetCore(options =>
            {
                options.CustomizeProblemDetails = ctx =>
                {
                    ctx.ProblemDetails.Extensions["custom"] = "value";
                    customKey = "value";
                };
            });

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<ProblemDetailsOptions>>().Value;

            // Act
            var httpContext = new DefaultHttpContext();
            var pd = new ProblemDetails();
            var ctx = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = pd
            };
            options.CustomizeProblemDetails!(ctx);

            // Assert
            pd.Extensions.Should().ContainKey("custom");
            pd.Extensions["custom"].Should().Be("value");
            customKey.Should().Be("value");
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Configures_ProblemDetailsOptions_TraceId()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddZentientResultsAspNetCore();

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<ProblemDetailsOptions>>().Value;

            // Act
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "trace-123";
            var pd = new ProblemDetails();
            var ctx = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = pd
            };
            options.CustomizeProblemDetails!(ctx);

            // Assert
            pd.Extensions.Should().ContainKey("traceId");
            pd.Extensions["traceId"].Should().Be("trace-123");
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Configures_ApiBehaviorOptions_InvalidModelStateResponseFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddZentientResultsAspNetCore();
            var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("FieldA", "ErrorA");
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = provider;
            var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(), modelState);

            // Act
            var result = options.InvalidModelStateResponseFactory(actionContext);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = (ObjectResult)result;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
            objectResult.ContentTypes.Should().Contain("application/problem+json");
            objectResult.Value.Should().BeOfType<ValidationProblemDetails>();
            var pd = (ValidationProblemDetails)objectResult.Value!;
            pd.Extensions.Should().ContainKey("zentientErrors");
        }

        [Fact]
        public void AddZentientResultsAspNetCore_DoesNot_Override_Controllers()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddControllers();
            services.AddZentientResultsAspNetCore();

            // Act
            var provider = services.BuildServiceProvider();

            // Assert
            provider.GetRequiredService<ProblemDetailsFactory>().Should().NotBeNull();
        }
    }
}
