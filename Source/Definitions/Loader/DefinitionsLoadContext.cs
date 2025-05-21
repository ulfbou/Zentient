// (C) 2025 Ulf Bourelius. All rights reserved.
// MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using Microsoft.Extensions.Logging;

using Zentient.Definitions.Core;

namespace Zentient.Definitions.Loader
{
    /// <summary>
    /// Provides a custom <see cref="AssemblyLoadContext"/> for loading assemblies from specified paths.
    /// </summary>
    public class DefinitionsLoadContext : AssemblyLoadContext
    {
        private readonly string[] _assemblyPaths;
        private readonly ILogger<DefinitionsLoadContext> _logger;
        private readonly Dictionary<string, Assembly> _assemblyCache = new Dictionary<string, Assembly>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionsLoadContext"/> class.
        /// </summary>
        /// <param name="loadContextName">The name of the load context.</param>
        /// <param name="assemblyPaths">The paths to search for assemblies.</param>
        /// <param name="logger">The logger instance for logging operations.</param>
        public DefinitionsLoadContext(string loadContextName, string[] assemblyPaths, ILogger<DefinitionsLoadContext> logger)
            : base(loadContextName, isCollectible: true)
        {
            _assemblyPaths = assemblyPaths ?? Array.Empty<string>();
            _logger = logger;
        }

        /// <summary>
        /// Loads an assembly by its name.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to load.</param>
        /// <returns>The loaded assembly, or <c>null</c> if the assembly could not be found or loaded.</returns>
        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (_assemblyCache.TryGetValue(assemblyName.FullName, out var cachedAssembly))
            {
                _logger.LogDebug($"Found assembly '{assemblyName.FullName}' in cache.");
                return cachedAssembly;
            }

            foreach (var path in _assemblyPaths)
            {
                string assemblyPath = null!;

                if (File.Exists(path))
                {
                    if (Path.GetFileNameWithoutExtension(path).Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        assemblyPath = path;
                    }
                }
                else if (Directory.Exists(path))
                {
                    assemblyPath = Path.Combine(path, $"{assemblyName.Name}.dll");
                    if (!File.Exists(assemblyPath))
                    {
                        assemblyPath = Path.Combine(path, $"{assemblyName.Name}.exe");
                        if (!File.Exists(assemblyPath))
                        {
                            assemblyPath = null!;
                        }
                    }
                }

                if (assemblyPath != null)
                {
                    try
                    {
                        var assembly = LoadFromAssemblyPathInternal(assemblyPath, assemblyName);
                        if (assembly != null && assembly.GetName().FullName.Equals(assemblyName.FullName, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation($"Loaded assembly '{assemblyName.FullName}' from '{assemblyPath}'.");
                            _assemblyCache[assemblyName.FullName] = assembly;
                            return assembly;
                        }
                        else if (assembly != null)
                        {
                            _logger.LogWarning($"Loaded assembly at '{assemblyPath}' with name '{assembly.GetName().FullName}' does not match requested name '{assemblyName.FullName}'.");
                        }
                    }
                    catch (FileLoadException ex)
                    {
                        _logger.LogWarning($"Error loading assembly from '{assemblyPath}': {ex.Message}");
                    }
                    catch (BadImageFormatException ex)
                    {
                        _logger.LogWarning($"Invalid assembly format at '{assemblyPath}': {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An unexpected error occurred while loading assembly from '{assemblyPath}': {ex.Message}");
                    }
                }
            }

            _logger.LogDebug($"Could not find or load assembly '{assemblyName.FullName}' in configured paths.");
            return null!;
        }

        /// <summary>
        /// Loads an assembly from a specified file path.
        /// </summary>
        /// <param name="assemblyPath">The file path of the assembly to load.</param>
        /// <returns>The loaded assembly.</returns>
        public new Assembly LoadFromAssemblyPath(string assemblyPath)
        {
            var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
            return LoadFromAssemblyPathInternal(assemblyPath, assemblyName);
        }

        // Loads an assembly from a specified file path with an optional assembly name hint.
        private Assembly LoadFromAssemblyPathInternal(string assemblyPath, AssemblyName assemblyNameHint = null!)
        {
            if (assemblyNameHint != null && _assemblyCache.TryGetValue(assemblyNameHint.FullName, out var cachedAssembly))
            {
                _logger.LogDebug($"Found assembly '{assemblyNameHint.FullName}' in cache (via LoadFromAssemblyPath).");
                return cachedAssembly;
            }

            try
            {
                Assembly assembly = base.LoadFromAssemblyPath(assemblyPath);
                if (assemblyNameHint != null)
                {
                    _assemblyCache[assembly.FullName!] = assembly;
                }
                return assembly;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading assembly from path '{assemblyPath}': {ex.Message}");
                throw;
            }
        }
    }
}
