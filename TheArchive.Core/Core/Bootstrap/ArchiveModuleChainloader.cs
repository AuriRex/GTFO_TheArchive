// This file is licensed under the LGPL 2.1 LICENSE
// See LICENSE_BepInEx in the projects root folder
// Original code from https://github.com/BepInEx/BepInEx

using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TheArchive.Core.Attributes;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using Version = SemanticVersioning.Version;

namespace TheArchive.Core.Bootstrap;

/// <summary>
/// Custom chain-loader for loading archive module assemblies
/// </summary>
[SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class ArchiveModuleChainloader
{
    /// <summary> CurrentAssemblyName </summary>
    protected static readonly string CurrentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
    /// <summary> CurrentAssemblyVersion </summary>
    protected static readonly System.Version CurrentAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

    /// <summary> Instance </summary>
    public static ArchiveModuleChainloader Instance { get; private set; }
    
    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= Loader.LoaderWrapper.CreateLoggerInstance(nameof(ArchiveModuleChainloader), ConsoleColor.Magenta);

    private static Regex allowedGuidRegex { get; } = new(@"^[a-zA-Z0-9\._\-]+$");

    /// <summary>
    ///     Analyzes the given type definition and attempts to convert it to a valid <see cref="ModuleInfo" />
    /// </summary>
    /// <param name="type">Type definition to analyze.</param>
    /// <param name="assemblyLocation">The filepath of the assembly, to keep as metadata.</param>
    /// <returns>If the type represent a valid plugin, returns a <see cref="ModuleInfo" /> instance. Otherwise, return null.</returns>
    public static ModuleInfo ToModuleInfo(TypeDefinition type, string assemblyLocation)
    {
        if (type.IsInterface || type.IsAbstract)
            return null;
        
        try
        {
            if (type.Interfaces.All(i => i.InterfaceType.FullName != typeof(IArchiveModule).FullName))
                return null;
        }
        catch (AssemblyResolutionException ex)
        {
            // Can happen if this type inherits a type from an assembly that can't be found. Safe to assume it's not a module.
            Logger.Exception(ex);
            return null;
        }
        
        var metadata = ArchiveModule.FromCecilType(type);
        
        // Perform checks that will prevent the module from being loaded in ALL cases
        if (metadata == null)
        {
            Logger.Warning($"Skipping over type [{type.FullName}] as no metadata attribute is specified.");
            return null;
        }
        
        if (string.IsNullOrEmpty(metadata.GUID) || !allowedGuidRegex.IsMatch(metadata.GUID))
        {
            Logger.Warning($"Skipping type [{type.FullName}] because its GUID [{metadata.GUID}] is of an illegal format.");
            return null;
        }
        
        if (metadata.Version == null)
        {
            Logger.Warning($"Skipping type [{type.FullName}] because its version is invalid.");
            return null;
        }
        
        if (metadata.Name == null)
        {
            Logger.Warning($"Skipping type [{type.FullName}] because its name is null.");
            return null;
        }
        
        var dependencies = ArchiveDependency.FromCecilType(type);
        var incompatibilities = ArchiveIncompatibility.FromCecilType(type);
        var assemblyNameReference = type.Module.AssemblyReferences.FirstOrDefault(reference => reference.Name == CurrentAssemblyName);
        var coreVersion = assemblyNameReference?.Version ?? new System.Version();
        
        return new ModuleInfo
        {
            Metadata = metadata,
            Dependencies = dependencies,
            Incompatibilities = incompatibilities,
            TypeName = type.FullName,
            TargetedTheArchiveVersion = coreVersion,
            Location = assemblyLocation
        };
    }

    /// <summary>
    /// Check for any types implementing <c>IArchiveModule</c> in an assembly.
    /// </summary>
    /// <param name="ass">AssemblyDefinition to check</param>
    /// <returns><c>True</c> if a IArchiveModule was found</returns>
    protected static bool HasArchiveModule(AssemblyDefinition ass)
    {
        if (ass.MainModule.AssemblyReferences.All(r => r.Name != CurrentAssemblyName))
            return false;
        
        var res = ass.MainModule.GetTypes().Any(td => td.Interfaces.Any(p => p.InterfaceType.FullName == typeof(IArchiveModule).FullName));
        
        return res;
    }

    /// <summary>
    /// Check archive version requirements.
    /// </summary>
    /// <param name="ModuleInfo">Module info to check</param>
    /// <returns><c>True</c> if all version checks pass</returns>
    protected static bool ModuleTargetsWrongTheArchive(ModuleInfo ModuleInfo)
    {
        var moduleTarget = ModuleInfo.TargetedTheArchiveVersion;

        if (moduleTarget.Major != CurrentAssemblyVersion.Major) return true;
        if (moduleTarget.Minor > CurrentAssemblyVersion.Minor) return true;
        if (moduleTarget.Minor < CurrentAssemblyVersion.Minor) return false;
        return moduleTarget.Build > CurrentAssemblyVersion.Build;
    }

    /// <summary>
    ///     List of all <see cref="ModuleInfo" /> instances loaded via the chainloader.
    /// </summary>
    public Dictionary<string, ModuleInfo> Modules { get; } = new();

    /// <summary>
    ///     Collection of error chainloader messages that occured during module loading.
    ///     Contains information about what certain modules were not loaded.
    /// </summary>
    public List<string> DependencyErrors { get; } = new();

    /// <summary>
    ///     Occurs after a module is loaded.
    /// </summary>
    public event Action<ModuleInfo> ModuleLoaded;

    /// <summary>
    ///     Occurs after all modules are loaded.
    /// </summary>
    public event Action Finished;

    internal static void Initialize()
    {
        if (Instance != null)
        {
            throw new InvalidOperationException("Chainloader cannot be initialized multiple times");
        }
        
        Instance = new();
        Logger.Notice("Chainloader initialized");
        Instance.Execute();
    }

    /// <summary>
    /// Discovers all modules in the plugins directory without loading them.
    /// </summary>
    /// <remarks>
    /// This is useful for discovering BepInEx module metadata.
    /// </remarks>
    /// <param name="path">Path from which to search the modules.</param>
    /// <param name="cacheName">Cache name to use. If null, results are not cached.</param>
    /// <returns>List of discovered modules and their metadata.</returns>
    protected IList<ModuleInfo> DiscoverModulesFrom(string path, string cacheName = "TheArchive_ModuleChainloader")
    {
        var modulesToLoad = TypeLoader.FindModuleTypes(path, ToModuleInfo, HasArchiveModule, cacheName);
        return modulesToLoad.SelectMany(p => p.Value).ToList();
    }

    /// <summary>
    /// Discovers modules to load.
    /// </summary>
    /// <returns>List of modules to be loaded.</returns>
    protected IList<ModuleInfo> DiscoverModules()
    {
#if BepInEx
        return DiscoverModulesFrom(BepInEx.Paths.PluginPath);
#endif
    }

    /// <summary>
    /// Preprocess the modules and modify the load order.
    /// </summary>
    /// <remarks>Some modules may be skipped if they cannot be loaded (wrong metadata, etc).</remarks>
    /// <param name="modules">Modules to process.</param>
    /// <returns>List of modules to load in the correct load order.</returns>
    protected IList<ModuleInfo> ModifyLoadOrder(IList<ModuleInfo> modules)
    {
        var dependencyDict = new SortedDictionary<string, IEnumerable<string>>(StringComparer.InvariantCultureIgnoreCase);
        var modulesByGuid = new Dictionary<string, ModuleInfo>();
        
        foreach (var moduleInfoGroup in modules.GroupBy(info => info.Metadata.GUID))
        {
            if (Modules.TryGetValue(moduleInfoGroup.Key, out var loadedModule))
            {
                Logger.Warning($"Skipping [{moduleInfoGroup.Key}] because a module with a similar GUID ([{loadedModule}]) has been already loaded.");
                continue;
            }

            ModuleInfo loadedVersion = null;
            foreach (var ModuleInfo in moduleInfoGroup.OrderByDescending(x => x.Metadata.Version))
            {
                if (loadedVersion != null)
                {
                    Logger.Warning($"Skip [{ModuleInfo}] because a newer version exists ({loadedVersion})");
                    continue;
                }

                loadedVersion = ModuleInfo;
                dependencyDict[ModuleInfo.Metadata.GUID] = ModuleInfo.Dependencies.Select(d => d.DependencyGUID);
                modulesByGuid[ModuleInfo.Metadata.GUID] = ModuleInfo;
            }
        }
        
        foreach (var moduleInfo in modulesByGuid.Values.ToList())
        {
            var incompatibilities = moduleInfo.Incompatibilities;

            if (incompatibilities.Any(incompatibility => modulesByGuid.ContainsKey(incompatibility.IncompatibilityGUID)
                || Modules.ContainsKey(incompatibility.IncompatibilityGUID)
#if BepInEx
                || BepInEx.Unity.IL2CPP.IL2CPPChainloader.Instance.Plugins.ContainsKey(incompatibility.IncompatibilityGUID)
#endif
            ))
            {
                modulesByGuid.Remove(moduleInfo.Metadata.GUID);
                dependencyDict.Remove(moduleInfo.Metadata.GUID);
                
        
                var incompatibleModulesNew = moduleInfo.Incompatibilities.Select(x => x.IncompatibilityGUID)
                                                                .Where(x => modulesByGuid.ContainsKey(x));
                
                var incompatibleModulesExisting = moduleInfo.Incompatibilities.Select(x => x.IncompatibilityGUID)
                                                                    .Where(x => Modules.ContainsKey(x));
                
                var incompatibleModules = incompatibleModulesNew.Concat(incompatibleModulesExisting).ToArray();
                var message =
                    $"Could not load [{moduleInfo}] because it is incompatible with: {string.Join(", ", incompatibleModules)}";
                DependencyErrors.Add(message);
                Logger.Error(message);
            }
            else if (ModuleTargetsWrongTheArchive(moduleInfo))
            {
                var message =
                    $"Module [{moduleInfo}] targets a wrong version of TheArchive ({moduleInfo.TargetedTheArchiveVersion}) and might not work until you update";
                DependencyErrors.Add(message);
                Logger.Warning(message);
            }
        }
        
        // We don't add already loaded modules to the dependency graph as they are already loaded
        
        var emptyDependencies = Array.Empty<string>();
        
        // Sort modules by their dependencies.
        // Give missing dependencies no dependencies of its own, which will cause missing modules to be first in the resulting list.
        var sortedModules = Utils.TopologicalSort(dependencyDict.Keys,
                x => dependencyDict.GetValueOrDefault(x, emptyDependencies)).ToList();
        
        return sortedModules.Where(modulesByGuid.ContainsKey).Select(x => modulesByGuid[x]).ToList();
    }

    /// <summary>
    /// Run the chainloader and load all modules from the plugins folder.
    /// </summary>
    public void Execute()
    {
        try
        {
            var modules = DiscoverModules();
            Logger.Info($"{modules.Count} module{(modules.Count == 1 ? "" : "s")} to load");
            LoadModules(modules);
            Finished?.Invoke();
        }
        catch (Exception ex)
        {
            Logger.Error("Error occurred loading modules: ");
            Logger.Exception(ex);
        }
        
        Logger.Notice("Chainloader startup complete");
    }

    private IList<ModuleInfo> LoadModules(IList<ModuleInfo> modules)
    {
        var sortedModules = ModifyLoadOrder(modules);
        
        var invalidModules = new HashSet<string>();
        var processedModules = new Dictionary<string, Version>();
        var loadedAssemblies = new Dictionary<string, Assembly>();
        var loadedModules = new List<ModuleInfo>();
        
        foreach (var module in sortedModules)
        {
            var dependsOnInvalidModule = false;
            var missingDependencies = new List<ArchiveDependency>();
            foreach (var dependency in module.Dependencies)
            {
                // If the dependency wasn't already processed, it's missing altogether
                var dependencyExists = processedModules.TryGetValue(dependency.DependencyGUID, out var moduleVersion);
                // Alternatively, if the dependency hasn't been loaded before, it's missing too
                if (!dependencyExists)
                {
                    dependencyExists = Modules.TryGetValue(dependency.DependencyGUID, out var moduleInfo);
                    moduleVersion = moduleInfo?.Metadata.Version;
                    
                    if (!dependencyExists || moduleVersion == null)
                    {
#if BepInEx
                        dependencyExists = BepInEx.Unity.IL2CPP.IL2CPPChainloader.Instance.Plugins.TryGetValue(dependency.DependencyGUID, out var pluginInfo);
                        moduleVersion = pluginInfo?.Metadata.Version;
#endif
                    }
                }
                
                if (!dependencyExists || dependency.VersionRange != null && !dependency.VersionRange.IsSatisfied(moduleVersion))
                {
                    // If the dependency is hard, collect it into a list to show
                    if (IsHardDependency(dependency))
                        missingDependencies.Add(dependency);
                    continue;
                }

                // If the dependency is a hard and is invalid (e.g. has missing dependencies), report that to the user
                if (invalidModules.Contains(dependency.DependencyGUID) && IsHardDependency(dependency))
                {
                    dependsOnInvalidModule = true;
                    break;
                }
            }
            
            processedModules.Add(module.Metadata.GUID, module.Metadata.Version);
            
            if (dependsOnInvalidModule)
            {
                var message =
                    $"Skipping [{module}] because it has a dependency that was not loaded. See previous errors for details.";
                DependencyErrors.Add(message);
                Logger.Warning(message);
                continue;
            }

            if (missingDependencies.Count != 0)
            {
                var message = $@"Could not load [{module}] because it has missing dependencies: {
                    string.Join(", ", missingDependencies.Select(s => s.VersionRange == null ? s.DependencyGUID : $"{s.DependencyGUID} ({s.VersionRange})").ToArray())
                }";
                DependencyErrors.Add(message);
                Logger.Error(message);
                
                invalidModules.Add(module.Metadata.GUID);
                continue;
            }

            try
            {
                Logger.Info($"Loading [{module}]");
                
                if (!loadedAssemblies.TryGetValue(module.Location, out var ass))
                    loadedAssemblies[module.Location] = ass = Assembly.LoadFrom(module.Location);
                
                Modules[module.Metadata.GUID] = module;
                TryRunModuleCtor(module, ass);
                module.Instance = LoadModule(module, ass);
                loadedModules.Add(module);

                ModuleLoaded?.Invoke(module);
            }
            catch (Exception ex)
            {
                invalidModules.Add(module.Metadata.GUID);
                Modules.Remove(module.Metadata.GUID);

                Logger.Error($"Error loading [{module}]:");
                if (ex is ReflectionTypeLoadException re)
                {
                    Logger.Error(TypeLoader.TypeLoadExceptionToString(re));
                }
                else Logger.Exception(ex);
            }
        }
        
        return loadedModules;
    }

    /// <summary>
    /// Detects and loads all modules in the specified directories.
    /// </summary>
    /// <remarks>
    /// It is better to collect all paths at once and use a single call to LoadModules than multiple calls.
    /// This allows to run proper dependency resolving and to load all modules in one go.
    /// </remarks>
    /// <param name="modulesPaths">Directories to search the modules from.</param>
    /// <returns>List of loaded module infos.</returns>
    public IList<ModuleInfo> LoadModule(params string[] modulesPaths)
    {
        var modules = new List<ModuleInfo>();
        foreach (var modulesPath in modulesPaths)
        {
            modules.AddRange(DiscoverModulesFrom(modulesPath));
        }
        return LoadModules(modules);
    }

    private static void TryRunModuleCtor(ModuleInfo module, Assembly assembly)
    {
        try
        {
            RuntimeHelpers.RunModuleConstructor(assembly.GetType(module.TypeName)!.Module.ModuleHandle);
        }
        catch (Exception ex)
        {
            Logger.Error($"Couldn't run Module constructor for {assembly.FullName}::{module.TypeName}:");
            Logger.Exception(ex);
        }
    }

    private IArchiveModule LoadModule(ModuleInfo moduleInfo, Assembly moduleAssembly)
    {
        return ArchiveMod.CreateAndInitModule(moduleAssembly.GetType(moduleInfo.TypeName));
    }

    private static bool IsHardDependency(ArchiveDependency dep) => (dep.Flags & ArchiveDependency.DependencyFlags.HardDependency) > 0;
}