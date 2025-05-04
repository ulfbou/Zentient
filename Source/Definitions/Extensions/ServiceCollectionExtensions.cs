// (C) 2025 Ulf Bourelius. All rights reserved.
// MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zentient.Definitions.Core;
using Zentient.Definitions.Loader;
using System.Runtime.Loader;

namespace Zentient.Definitions.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring and registering definitions in the service collection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private static readonly Dictionary<Type, ServiceDescriptor> _registeredServices = new Dictionary<Type, ServiceDescriptor>();

        /// <summary>
        /// Adds definitions to the service collection.
        /// </summary>
        /// <param name="services">The service collection to which definitions will be added.</param>
        /// <param name="initialConfiguration">The initial configuration to use for definitions.</param>
        /// <param name="configureBuilder">An action to configure the <see cref="DefinitionsLibraryBuilder"/>.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddDefinitions(this IServiceCollection services, IConfiguration initialConfiguration, Action<DefinitionsLibraryBuilder> configureBuilder)
        {
            var builder = new DefinitionsLibraryBuilder();
            configureBuilder(builder);
            var options = builder.Options;

            options.OverrideSourceFactory = (overrides) => new MemoryConfigurationSource(overrides);

            services.AddSingleton(options);
            services.AddSingleton<DefinitionsLoadContext>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<DefinitionsLoadContext>();
                var loadContext = new DefinitionsLoadContext("DefinitionsLoadContext", options.AssemblyPaths, logger);
                options.PreLoadAction?.Invoke(loadContext);
                return loadContext;
            });

            services.AddSingleton<IConfiguration>(sp =>
            {
                var configBuilder = new ConfigurationBuilder()
                    .AddConfiguration(initialConfiguration)
                    .Add(options.OverrideSourceFactory(options.ConfigurationOverrides));

                return configBuilder.Build();
            });

            var serviceProvider = services.BuildServiceProvider();
            var loadContext = serviceProvider.GetRequiredService<DefinitionsLoadContext>();
            var logger = serviceProvider.GetRequiredService<ILogger<DefinitionsLoadContext>>();
            var criticalModules = options.CriticalModules ?? Array.Empty<string>();
            var currentConfiguration = serviceProvider.GetRequiredService<IConfiguration>();

            foreach (var assemblyPath in options.AssemblyPaths.Where(p => File.Exists(p) || Directory.Exists(p)))
            {
                Assembly assembly = null!;
                try
                {
                    if (File.Exists(assemblyPath))
                    {
                        assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
                    }
                    else if (Directory.Exists(assemblyPath))
                    {
                        foreach (var dllFile in Directory.GetFiles(assemblyPath, "*.dll"))
                        {
                            try
                            {
                                assembly = loadContext.LoadFromAssemblyPath(dllFile);
                                if (assembly != null)
                                {
                                    RegisterModules(services, currentConfiguration, assembly, logger, criticalModules, options.VersionCompatibility);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError($"Error loading or configuring modules from '{dllFile}': {ex.Message}");
                            }
                        }
                        continue;
                    }

                    if (assembly != null)
                    {
                        RegisterModules(services, currentConfiguration, assembly, logger, criticalModules, options.VersionCompatibility);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error loading assembly from '{assemblyPath}': {ex.Message}");
                }
            }

            return services;
        }

        /// <summary>
        /// Registers modules from the specified assembly into the service collection.
        /// </summary>
        /// <param name="services">The service collection to which modules will be registered.</param>
        /// <param name="configuration">The configuration to use for module registration.</param>
        /// <param name="assembly">The assembly containing the modules to register.</param>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <param name="criticalModules">An array of critical module names that must be loaded.</param>
        /// <param name="versionCompatibility">The strategy for version compatibility.</param>
        private static void RegisterModules(IServiceCollection services, IConfiguration configuration, Assembly assembly, ILogger logger, string[] criticalModules, DefinitionsOptions.VersionCompatibilityStrategy versionCompatibility)
        {
            try
            {
                var moduleTypes = assembly.GetTypes()
                    .Where(t => typeof(IDefinitionModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var moduleType in moduleTypes)
                {
                    if (Activator.CreateInstance(moduleType) is IDefinitionModule module)
                    {
                        var moduleName = module.ModuleName;
                        var moduleVersion = module.ModuleContractVersion;
                        var configSection = configuration.GetSection(moduleName);

                        logger.LogInformation($"Found definition module '{moduleName}' (v{moduleVersion}) in assembly '{assembly.GetName().Name}'.");
                        logger.LogDebug($"Module '{moduleName}' contract version: {moduleVersion}. Compatibility strategy: {versionCompatibility}");

                        try
                        {
                            module.ConfigureServices(services, configSection);
                        }
                        catch (Exception ex)
                        {
                            if (criticalModules.Contains(moduleName, StringComparer.OrdinalIgnoreCase))
                            {
                                logger.LogError($"Critical module '{moduleName}' failed during service configuration: {ex.Message}");
                                throw new DefinitionLoadException($"Critical module '{moduleName}' failed to configure services.", ex);
                            }
                            else
                            {
                                logger.LogError($"Module '{moduleName}' failed during service configuration: {ex.Message}");
                            }
                            continue;
                        }

                        var settingsType = assembly.GetTypes().FirstOrDefault(t => t.Name.EndsWith("Settings") && t.Namespace == moduleType.Namespace);
                        if (settingsType != null)
                        {
                            var configureMethod = typeof(OptionsServiceCollectionExtensions)
                                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                                .FirstOrDefault(m => m.Name == "Configure" && m.GetParameters().Length == 2)
                                ?.MakeGenericMethod(settingsType);

                            configureMethod?.Invoke(null, new object[] { services, configSection });
                            logger.LogDebug($"Bound configuration section '{moduleName}' to '{settingsType.FullName}'.");
                        }
                        else
                        {
                            logger.LogDebug($"No 'Settings' class found in namespace '{moduleType.Namespace}' for module '{moduleName}'.");
                        }

                        foreach (var serviceDescriptor in services.Where(sd => _registeredServices.ContainsKey(sd.ServiceType) && sd.Lifetime == _registeredServices[sd.ServiceType].Lifetime && sd.ImplementationType != _registeredServices[sd.ServiceType].ImplementationType && sd.ImplementationFactory == null && _registeredServices[sd.ServiceType].ImplementationFactory == null).ToList())
                        {
                            logger.LogError($"Dependency conflict detected for service type '{serviceDescriptor.ServiceType.FullName}'. Last registration from module '{moduleName}' will be used.");
                            services.Remove(_registeredServices[serviceDescriptor.ServiceType]);
                        }
                        foreach (var serviceDescriptor in services)
                        {
                            if (!_registeredServices.ContainsKey(serviceDescriptor.ServiceType))
                            {
                                _registeredServices.Add(serviceDescriptor.ServiceType, serviceDescriptor);
                            }
                        }

                        var serviceProvider = services.BuildServiceProvider();
                        var options = serviceProvider.GetService<DefinitionsOptions>();
                        options?.OnModuleLoaded?.Invoke(moduleType, serviceProvider);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var loaderException in ex.LoaderExceptions)
                {
                    logger.LogError($"Type load error in assembly '{assembly.GetName().Name}': {loaderException!.Message}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error during module registration in assembly '{assembly.GetName().Name}': {ex.Message}");
            }
        }
    }
}
