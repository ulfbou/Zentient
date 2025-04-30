using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Zentient.Runtime.Manifest
{
    /// <summary>
    /// Represents options for configuring the <see cref="ConfigurationTemplateProvider"/>.
    /// </summary>
    public class ConfigurationTemplateProviderOptions
    {
        /// <summary>
        /// Gets or sets the root configuration key for templates.
        /// </summary>
        public string RootConfigKey { get; set; } = "Manifest";

        /// <summary>
        /// Gets or sets a value indicating whether to enforce placeholder count validation.
        /// </summary>
        public bool EnforcePlaceholderCount { get; set; } = true;
    }
}
