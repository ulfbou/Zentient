// Zentient/Definitions/Core/IDefinitionModule.cs

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zentient.Definitions.Core
{
    public interface IDefinitionModule
    {
        string ModuleName { get; }
        string ModuleContractVersion { get; }
        void ConfigureServices(IServiceCollection services, IConfigurationSection config);
    }
}
