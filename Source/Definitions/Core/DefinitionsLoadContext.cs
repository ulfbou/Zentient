// Zentient/Definitions/Loader/DefinitionsLoadContext.cs
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
    public class DefinitionsLoadContext : AssemblyLoadContext
    {
        private readonly string[] _assemblyPaths;
        private readonly ILogger<DefinitionsLoadContext> _logger;
        private readonly Dictionary<string, Assembly> _assemblyCache = new Dictionary<string, Assembly>();

        public DefinitionsLoadContext(string loadContextName, string[] assemblyPaths, ILogger<DefinitionsLoadContext> logger) : base(loadContextName, isCollectible: true)
        {
            _assemblyPaths = assemblyPaths ?? Array.Empty<string>();
            _logger = logger;
        }

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
                        assemblyPath = Path.Combine(path, $"{assemblyName.Name}.exe"); // Check for .exe as well
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
                            // TODO: Implement strong-name and Authenticode validation here (in a later refinement)
                            return assembly;
                        }
                        else if (assembly != null)
                        {
                            _logger.LogWarning($"Loaded assembly at '{assemblyPath}' with name '{assembly.GetName().FullName}' does not match requested name '{assemblyName.FullName}'.");
                            // Don't cache if the name doesn't match the request.
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

        public new Assembly LoadFromAssemblyPath(string assemblyPath)
        {
            var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
            return LoadFromAssemblyPathInternal(assemblyPath, assemblyName);
        }

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
                throw; // Re-throw to be caught by the caller
            }
        }
    }
}
