// (C) 2025 Ulf Bourelius. All rights reserved.
// MIT License. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;

namespace Zentient.Definitions.Core
{
    /// <summary>
    /// Represents a configuration source that stores settings in memory.
    /// </summary>
    internal class MemoryConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// A dictionary to store configuration settings in memory.
        /// </summary>
        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryConfigurationSource"/> class.
        /// </summary>
        /// <param name="overrides">
        /// A dictionary containing configuration overrides, where each key is a tuple of module name and key,
        /// and the value is the corresponding configuration value.
        /// </param>
        public MemoryConfigurationSource(Dictionary<(string ModuleName, string Key), string> overrides)
        {
            foreach (var ((moduleName, key), value) in overrides)
            {
                _settings[$"{moduleName}:{key}"] = value;
            }
        }

        /// <summary>
        /// Builds the <see cref="IConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <returns>A <see cref="MemoryConfigurationProvider"/> instance.</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new MemoryConfigurationProvider(_settings!);
        }
    }
}
