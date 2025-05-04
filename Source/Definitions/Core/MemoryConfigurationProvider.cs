// Zentient/Definitions/Core/MemoryConfigurationProvider.cs
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
