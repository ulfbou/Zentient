// (C) 2025 Ulf Bourelius. All rights reserved.
// MIT License. See LICENSE file in the project root for full license information.

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
