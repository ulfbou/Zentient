// (C) 2025 Ulf Bourelius. All rights reserved.
// MIT License. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zentient.Definitions.Core
{
    /// <summary>
    /// Represents a module definition that provides configuration and services for the application.
    /// </summary>
    public interface IDefinitionModule
    {
        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// Gets the version of the module's contract.
        /// </summary>
        string ModuleContractVersion { get; }

        /// <summary>
        /// Configures the services required by the module.
        /// </summary>
        /// <param name="services">The service collection to which the module's services will be added.</param>
        /// <param name="config">The configuration section specific to the module.</param>
        void ConfigureServices(IServiceCollection services, IConfigurationSection config);
    }
}
