using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Zentient.Runtime.Manifest
{
    /// <summary>
    /// Resolves localized templates from configuration with caching and placeholder validation.
    /// </summary>
    public class ConfigurationTemplateProvider : ILocalizedValueProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationTemplateProvider> _logger;
        private readonly ConcurrentDictionary<(string scope, string key, string culture), string?> _cache = new();
        private readonly string _rootConfigKey;
        private readonly bool _enforcePlaceholderCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationTemplateProvider"/> class.
        /// </summary>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="rootConfigKey">The root configuration key for templates.</param>
        /// <param name="enforcePlaceholderCount">Whether to enforce placeholder count validation.</param>
        public ConfigurationTemplateProvider(IConfiguration configuration, ILogger<ConfigurationTemplateProvider> logger, string rootConfigKey = "Manifest", bool enforcePlaceholderCount = true)
        {
            _configuration = configuration;
            _logger = logger;
            _rootConfigKey = rootConfigKey;
            _enforcePlaceholderCount = enforcePlaceholderCount;
        }

        /// <inheritdoc />
        public string Resolve(string scope, string key, CultureInfo culture, params object[] args)
        {
            if (TryResolveInternal(scope, key, culture, out var value, args))
            {
                return value;
            }
            throw new KeyNotFoundException($"Template with key '{key}' in scope '{scope}' for culture '{culture.Name}' not found.");
        }

        /// <inheritdoc />
        public string Resolve(string scope, string key, params object[] args)
        {
            return Resolve(scope, key, CultureInfo.CurrentCulture, args);
        }

        /// <inheritdoc />
        public bool TryResolve(string scope, string key, out string value, CultureInfo? culture = null, params object[] args)
        {
            return TryResolveInternal(scope, key, culture ?? CultureInfo.CurrentCulture, out value, args);
        }

        private bool TryResolveInternal(string scope, string key, CultureInfo culture, out string resolvedValue, params object[] args)
        {
            var cacheKey = (scope, key, culture.Name);
            if (_cache.TryGetValue(cacheKey, out var cachedTemplate))
            {
                if (cachedTemplate is not null)
                {
                    resolvedValue = string.Format(culture, cachedTemplate, args);
                    return true;
                }
                resolvedValue = null!;
                return false;
            }

            var configPath = $"{_rootConfigKey}:{scope}:{key}";
            var template = _configuration[configPath];
            if (template is null)
            {
                _logger.LogDebug("Template with key '{Key}' in scope '{Scope}' for culture '{Culture}' not found in configuration at '{Path}'.", key, scope, culture.Name, configPath);
                resolvedValue = null!;
                return false;
            }

            var placeholders = Regex.Matches(template, @"\{(\w+)\}");
            if (_enforcePlaceholderCount && placeholders.Count > args.Length)
            {
                _logger.LogWarning("Template for key '{Key}' in scope '{Scope}' has more placeholders ({PlaceholderCount}) than provided arguments ({ArgumentCount}).", key, scope, placeholders.Count, args.Length);
                resolvedValue = null!;
                return false;
            }

            if (!_enforcePlaceholderCount && placeholders.Count > args.Length)
            {
                var extendedArgs = new object[placeholders.Count];
                Array.Copy(args, extendedArgs, args.Length);
                for (int i = args.Length; i < placeholders.Count; i++)
                {
                    extendedArgs[i] = string.Empty;
                }
                args = extendedArgs;
            }

            resolvedValue = string.Format(culture, template, args);
            _cache.TryAdd(cacheKey, template);
            return true;
        }

    }
}
