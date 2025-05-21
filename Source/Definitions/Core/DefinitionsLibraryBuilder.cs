// (C) 2025 Ulf Bourelius. All rights reserved.
// MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Loader;

using Polly.Registry;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Zentient.Definitions.Core
{
    /// <summary>
    /// Provides a builder for configuring and creating a definitions library.
    /// </summary>
    public class DefinitionsLibraryBuilder
    {
        /// <summary>
        /// Gets the options used to configure the definitions library.
        /// </summary>
        public DefinitionsOptions Options { get; } = new DefinitionsOptions();

        /// <summary>
        /// Specifies the assembly paths to be used by the definitions library.
        /// </summary>
        /// <param name="assemblyPaths">An array of assembly paths.</param>
        /// <returns>The current <see cref="DefinitionsLibraryBuilder"/> instance.</returns>
        public DefinitionsLibraryBuilder WithAssemblyPaths(params string[] assemblyPaths)
        {
            Options.AssemblyPaths = assemblyPaths;
            return this;
        }

        /// <summary>
        /// Specifies the critical modules to be used by the definitions library.
        /// </summary>
        /// <param name="criticalModules">An array of critical module names.</param>
        /// <returns>The current <see cref="DefinitionsLibraryBuilder"/> instance.</returns>
        public DefinitionsLibraryBuilder WithCriticalModules(params string[] criticalModules)
        {
            Options.CriticalModules = criticalModules;
            return this;
        }

        /// <summary>
        /// Specifies an action to be executed before loading assemblies.
        /// </summary>
        /// <param name="preLoadAction">An action to execute with the <see cref="AssemblyLoadContext"/>.</param>
        /// <returns>The current <see cref="DefinitionsLibraryBuilder"/> instance.</returns>
        public DefinitionsLibraryBuilder WithPreLoadAction(Action<AssemblyLoadContext> preLoadAction)
        {
            Options.PreLoadAction = preLoadAction;
            return this;
        }

        /// <summary>
        /// Specifies an action to be executed when a module is loaded.
        /// </summary>
        /// <param name="onModuleLoaded">An action to execute with the module type and service provider.</param>
        /// <returns>The current <see cref="DefinitionsLibraryBuilder"/> instance.</returns>
        public DefinitionsLibraryBuilder WithOnModuleLoadedAction(Action<Type, IServiceProvider> onModuleLoaded)
        {
            Options.OnModuleLoaded = onModuleLoaded;
            return this;
        }

        /// <summary>
        /// Overrides a specific module setting with a given key-value pair.
        /// </summary>
        /// <param name="moduleName">The name of the module.</param>
        /// <param name="key">The key of the setting to override.</param>
        /// <param name="value">The value to set for the specified key.</param>
        /// <returns>The current <see cref="DefinitionsLibraryBuilder"/> instance.</returns>
        public DefinitionsLibraryBuilder OverrideModuleSetting(string moduleName, string key, string value)
        {
            Options.OverrideSetting(moduleName, key, value);
            return this;
        }

        /// <summary>
        /// Specifies a policy registry to be used for resilience policies.
        /// </summary>
        /// <param name="policyRegistry">The policy registry to use.</param>
        /// <returns>The current <see cref="DefinitionsLibraryBuilder"/> instance.</returns>
        public DefinitionsLibraryBuilder WithPolicyRegistry(IPolicyRegistry<string> policyRegistry)
        {
            Options.PolicyRegistry = policyRegistry;
            return this;
        }
    }
}
