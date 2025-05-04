// (C) 2025 Ulf Bourelius. All rights reserved.
// MIT License. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;

namespace Zentient.Definitions.Core
{
    internal class MemoryConfigurationSource : IConfigurationSource
    {
        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>();

        public MemoryConfigurationSource(Dictionary<(string ModuleName, string Key), string> overrides)
        {
            foreach (var ((moduleName, key), value) in overrides)
            {
                _settings[$"{moduleName}:{key}"] = value;
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new MemoryConfigurationProvider(_settings!);
        }
    }
}
