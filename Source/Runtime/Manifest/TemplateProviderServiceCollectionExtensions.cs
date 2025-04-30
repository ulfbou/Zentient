using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Zentient.Runtime.Manifest
{
    /// <summary>
    /// Provides extension methods for registering template provider services.
    /// </summary>
    public static class TemplateProviderServiceCollectionExtensions
    {
        /// <summary>
        /// Adds template provider services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="rootConfigKey">The root configuration key for templates.</param>
        /// <param name="enforcePlaceholderCount">Whether to enforce placeholder count validation.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddTemplateProviderServices(this IServiceCollection services, string rootConfigKey = "Manifest", bool enforcePlaceholderCount = true)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            ArgumentNullException.ThrowIfNullOrEmpty(rootConfigKey, nameof(rootConfigKey));
            services.AddSingleton<ILocalizedValueProvider>(sp =>
                new ConfigurationTemplateProvider(sp.GetRequiredService<IConfiguration>(), sp.GetRequiredService<ILogger<ConfigurationTemplateProvider>>(), rootConfigKey, enforcePlaceholderCount));
            return services;
        }

        /// <summary>
        /// Adds template provider services to the service collection with custom options.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">The action to configure options.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddTemplateProviderServices(this IServiceCollection services, Action<ConfigurationTemplateProviderOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            var options = new ConfigurationTemplateProviderOptions();
            configureOptions?.Invoke(options);
            return services.AddTemplateProviderServices(options.RootConfigKey, options.EnforcePlaceholderCount);
        }
    }
}
