// (C) 2025 Ulf Bourelius. All rights reserved.
// MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.Loader;

using Microsoft.Extensions.Configuration;

using Polly.Registry;

namespace Zentient.Definitions.Core
{
    /// <summary>
    /// Represents configuration options for definitions in the Zentient framework.
    /// </summary>
    public class DefinitionsOptions
    {
        /// <summary>
        /// Gets or sets the paths to assemblies that should be loaded.
        /// </summary>
        public string[] AssemblyPaths { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the names of critical modules that must be loaded.
        /// </summary>
        public string[] CriticalModules { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets an action to be executed before an assembly is loaded.
        /// </summary>
        public Action<AssemblyLoadContext> PreLoadAction { get; set; } = _ => { };

        /// <summary>
        /// Gets or sets an action to be executed when a module is loaded.
        /// </summary>
        public Action<Type, IServiceProvider> OnModuleLoaded { get; set; } = (_, _) => { };

        /// <summary>
        /// Gets or sets a dictionary of configuration overrides for specific modules and keys.
        /// </summary>
        internal Dictionary<(string ModuleName, string Key), string> ConfigurationOverrides { get; set; } = new Dictionary<(string ModuleName, string Key), string>();

        /// <summary>
        /// Gets or sets the memory configuration source for overrides.
        /// </summary>
        internal MemoryConfigurationSource OverrideConfigurationSource { get; set; } = null!;

        /// <summary>
        /// Overrides a specific setting for a module.
        /// </summary>
        /// <param name="moduleName">The name of the module.</param>
        /// <param name="key">The key of the setting to override.</param>
        /// <param name="value">The value to set for the override.</param>
        public void OverrideSetting(string moduleName, string key, string value) => ConfigurationOverrides[(moduleName, key)] = value;

        /// <summary>
        /// Gets or sets the policy registry for managing resilience policies.
        /// </summary>
        public IPolicyRegistry<string> PolicyRegistry { get; set; } = new PolicyRegistry();

        /// <summary>
        /// Gets or sets the path to the assemblies manifest file.
        /// </summary>
        internal string AssembliesManifestPath { get; set; } = null!;

        /// <summary>
        /// Gets or sets the list of trusted root certificates.
        /// </summary>
        internal List<string> TrustedRootCertificates { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether sandboxing is enabled.
        /// </summary>
        internal bool EnableSandboxing { get; set; } = false;

        /// <summary>
        /// Gets or sets a factory function for creating sandboxed assembly load contexts.
        /// </summary>
        internal Func<AssemblyLoadContext, AssemblyLoadContext> SandboxFactory { get; set; } = alc => alc;

        /// <summary>
        /// Gets or sets the strategy for version compatibility.
        /// </summary>
        internal VersionCompatibilityStrategy VersionCompatibility { get; set; } = VersionCompatibilityStrategy.Exact;

        /// <summary>
        /// Gets or sets a factory function for creating override configuration sources.
        /// </summary>
        internal Func<Dictionary<(string ModuleName, string Key), string>, IConfigurationSource> OverrideSourceFactory { get; set; } = null!;

        /// <summary>
        /// Defines strategies for version compatibility.
        /// </summary>
        public enum VersionCompatibilityStrategy
        {
            /// <summary>
            /// Requires an exact version match.
            /// </summary>
            Exact,

            /// <summary>
            /// Allows compatibility with the same major version.
            /// </summary>
            Major
        }
    }
}
