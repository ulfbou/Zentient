// Zentient/Definitions/Core/MemoryConfigurationSource.cs
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
