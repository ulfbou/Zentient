// (C) 2025 Ulf Bourelius. All rights reserved.
// MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Loader;

using Polly.Registry;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Zentient.Definitions.Core
{
    public class DefinitionsLibraryBuilder
    {
        public DefinitionsOptions Options { get; } = new DefinitionsOptions();

        public DefinitionsLibraryBuilder WithAssemblyPaths(params string[] assemblyPaths)
        {
            Options.AssemblyPaths = assemblyPaths;
            return this;
        }

        public DefinitionsLibraryBuilder WithCriticalModules(params string[] criticalModules)
        {
            Options.CriticalModules = criticalModules;
            return this;
        }

        public DefinitionsLibraryBuilder WithPreLoadAction(Action<AssemblyLoadContext> preLoadAction)
        {
            Options.PreLoadAction = preLoadAction;
            return this;
        }

        public DefinitionsLibraryBuilder WithOnModuleLoadedAction(Action<Type, IServiceProvider> onModuleLoaded)
        {
            Options.OnModuleLoaded = onModuleLoaded;
            return this;
        }

        public DefinitionsLibraryBuilder OverrideModuleSetting(string moduleName, string key, string value)
        {
            Options.OverrideSetting(moduleName, key, value);
            return this;
        }

        // Placeholder for Polly configuration (more details in a later refinement if needed)
        public DefinitionsLibraryBuilder WithPolicyRegistry(IPolicyRegistry<string> policyRegistry)
        {
            Options.PolicyRegistry = policyRegistry;
            return this;
        }
    }
}
