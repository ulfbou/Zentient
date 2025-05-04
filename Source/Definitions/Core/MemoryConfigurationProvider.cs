// (C) 2025 Ulf Bourelius. All rights reserved.
// MIT License. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;

namespace Zentient.Definitions.Core
{
    /// <summary>
    /// Provides a memory-based implementation of the <see cref="ConfigurationProvider"/> class.
    /// </summary>
    internal class MemoryConfigurationProvider : ConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryConfigurationProvider"/> class.
        /// </summary>
        /// <param name="settings">
        /// A dictionary containing the configuration settings. Keys represent configuration keys, and values represent configuration values.
        /// If <paramref name="settings"/> is null, an empty dictionary will be used.
        /// </param>
        public MemoryConfigurationProvider(Dictionary<string, string?> settings)
        {
            Data = settings ?? new Dictionary<string, string?>();
        }
    }
}
