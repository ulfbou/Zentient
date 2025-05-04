// (C) 2025 Ulf Bourelius. All rights reserved.
// MIT License. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;

namespace Zentient.Definitions.Core
{
    internal class MemoryConfigurationProvider : ConfigurationProvider
    {
        public MemoryConfigurationProvider(Dictionary<string, string?> settings)
        {
            Data = settings ?? new Dictionary<string, string?>();
        }
    }
}
