// Zentient/Definitions/Core/DefinitionsOptions.cs
using System;
using System.Collections.Generic;
using System.Runtime.Loader;

using Microsoft.Extensions.Configuration;

using Polly.Registry;

namespace Zentient.Definitions.Core
{
    public class DefinitionsOptions
    {
        public string[] AssemblyPaths { get; set; } = Array.Empty<string>();
        public string[] CriticalModules { get; set; } = Array.Empty<string>();

        // Extension Points
        public Action<AssemblyLoadContext> PreLoadAction { get; set; } = _ => { };
        public Action<Type, IServiceProvider> OnModuleLoaded { get; set; } = (_, _) => { };

        // Runtime Configuration Overrides
        internal Dictionary<(string ModuleName, string Key), string> ConfigurationOverrides { get; set; } = new Dictionary<(string ModuleName, string Key), string>();
        internal MemoryConfigurationSource OverrideConfigurationSource { get; set; } = null!;
        public void OverrideSetting(string moduleName, string key, string value) => ConfigurationOverrides[(moduleName, key)] = value;

        // Resilience
        public IPolicyRegistry<string> PolicyRegistry { get; set; } = new PolicyRegistry();

        // Security (for future implementation)
        internal string AssembliesManifestPath { get; set; } = null!;
        internal List<string> TrustedRootCertificates { get; set; } = new List<string>();

        // Sandboxing (for future implementation)
        internal bool EnableSandboxing { get; set; } = false;
        internal Func<AssemblyLoadContext, AssemblyLoadContext> SandboxFactory { get; set; } = alc => alc;

        // Versioning
        internal VersionCompatibilityStrategy VersionCompatibility { get; set; } = VersionCompatibilityStrategy.Exact;

        internal Func<Dictionary<(string ModuleName, string Key), string>, IConfigurationSource> OverrideSourceFactory { get; set; } = null!;

        public enum VersionCompatibilityStrategy
        {
            Exact,
            Major
        }
    }
}
