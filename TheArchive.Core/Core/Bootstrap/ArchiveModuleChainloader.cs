using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TheArchive.Core.Attributes;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using Version = SemanticVersioning.Version;

namespace TheArchive.Core.Bootstrap;

public class ArchiveModuleChainloader
{
    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= Loader.LoaderWrapper.CreateLoggerInstance(nameof(ArchiveModuleChainloader), ConsoleColor.White);

    private static Regex allowedGuidRegex { get; } = new Regex("^[a-zA-Z0-9\\._\\-]+$");

    public static ModuleInfo ToModuleInfo(TypeDefinition type, string assemblyLocation)
    {
        if (type.IsInterface || type.IsAbstract)
        {
            return null;
        }
        try
        {
            if (!type.Interfaces.Any(i => i.InterfaceType.FullName == typeof(IArchiveModule).FullName))
            {
                return null;
            }
        }
        catch (AssemblyResolutionException ex)
        {
            Logger.Exception(ex);
            return null;
        }
        ArchiveModule metadata = ArchiveModule.FromCecilType(type);
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
        IEnumerable<ArchiveDependency> dependencies = ArchiveDependency.FromCecilType(type);
        IEnumerable<ArchiveIncompatibility> incompatibilities = ArchiveIncompatibility.FromCecilType(type);
        AssemblyNameReference assemblyNameReference = type.Module.AssemblyReferences.FirstOrDefault((reference) => reference.Name == CurrentAssemblyName);
        System.Version coreVersion = (assemblyNameReference != null ? assemblyNameReference.Version : null) ?? new();
        return new ModuleInfo
        {
            Metadata = metadata,
            Dependencies = dependencies,
            Incompatibilities = incompatibilities,
            TypeName = type.FullName,
            TargettedTheArchiveVersion = coreVersion,
            Location = assemblyLocation
        };
    }

    protected static bool HasArchiveModule(AssemblyDefinition ass)
    {
        if (!ass.MainModule.AssemblyReferences.Any((r) => r.Name == CurrentAssemblyName))
        {
            return false;
        }
        var res = ass.MainModule.GetTypes().Any((td) => td.Interfaces.Any(p => p.InterfaceType.FullName == typeof(IArchiveModule).FullName));
        return res;
    }

    protected static bool ModuleTargetsWrongTheArchive(ModuleInfo ModuleInfo)
    {
        System.Version moduleTarget = ModuleInfo.TargettedTheArchiveVersion;
        return moduleTarget.Major != CurrentAssemblyVersion.Major || moduleTarget.Minor > CurrentAssemblyVersion.Minor || moduleTarget.Minor >= CurrentAssemblyVersion.Minor && moduleTarget.Build > CurrentAssemblyVersion.Build;
    }

    public Dictionary<string, ModuleInfo> Modules { get; } = new Dictionary<string, ModuleInfo>();

    public List<string> DependencyErrors { get; } = new List<string>();


    public event Action<ModuleInfo, Assembly, IArchiveModule> ModuleLoad;

    public event Action<ModuleInfo> ModuleLoaded;

    public event Action Finished;

    public static void Initialize()
    {
        if (Instance != null)
        {
            throw new InvalidOperationException("Chainloader cannot be initialized multiple times");
        }
        Instance = new();
        Logger.Notice("Chainloader initialized");
        Instance.Execute();
    }

    protected IList<ModuleInfo> DiscoverModulesFrom(string path, string cacheName = "TheArchive_ModuleChainloader")
    {
        return TypeLoader.FindModuleTypes(path, new Func<TypeDefinition, string, ModuleInfo>(ToModuleInfo), new Func<AssemblyDefinition, bool>(HasArchiveModule), cacheName).SelectMany((p) => p.Value).ToList();
    }

    protected IList<ModuleInfo> DiscoverModules()
    {
#if R_BIE
        return DiscoverModulesFrom(BepInEx.Paths.PluginPath, "TheArchive_ModuleChainloader");
#endif
    }

    protected IList<ModuleInfo> ModifyLoadOrder(IList<ModuleInfo> modules)
    {
        SortedDictionary<string, IEnumerable<string>> dependencyDict = new SortedDictionary<string, IEnumerable<string>>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, ModuleInfo> modulesByGuid = new Dictionary<string, ModuleInfo>();
        foreach (IGrouping<string, ModuleInfo> ModuleInfoGroup in from info in modules
                                                                  group info by info.Metadata.GUID)
        {
            if (Modules.TryGetValue(ModuleInfoGroup.Key, out var loadedModule))
            {
                Logger.Warning($"Skipping [{ModuleInfoGroup.Key}] because a module with a similar GUID ([{loadedModule}]) has been already loaded.");
            }
            else
            {
                ModuleInfo loadedVersion = null;
                foreach (ModuleInfo ModuleInfo in ModuleInfoGroup.OrderByDescending((x) => x.Metadata.Version))
                {
                    if (loadedVersion != null)
                    {
                        Logger.Warning($"Skip [{ModuleInfo}] because a newer version exists ({loadedVersion})");
                    }
                    else
                    {
                        loadedVersion = ModuleInfo;
                        dependencyDict[ModuleInfo.Metadata.GUID] = ModuleInfo.Dependencies.Select((d) => d.DependencyGUID);
                        modulesByGuid[ModuleInfo.Metadata.GUID] = ModuleInfo;
                    }
                }
            }
        }
        Func<ArchiveIncompatibility, bool> funcE = null;
        Func<string, bool> funcE2 = null;
        Func<string, bool> funcE3 = null;
        foreach (ModuleInfo ModuleInfo2 in modulesByGuid.Values.ToList())
        {
            IEnumerable<ArchiveIncompatibility> incompatibilities = ModuleInfo2.Incompatibilities;
            Func<ArchiveIncompatibility, bool> func = null;
            if ((func = funcE) == null)
            {
                func = (incompatibility) => modulesByGuid.ContainsKey(incompatibility.IncompatibilityGUID) || Modules.ContainsKey(incompatibility.IncompatibilityGUID)
#if R_BIE
                 || BepInEx.Unity.IL2CPP.IL2CPPChainloader.Instance.Plugins.ContainsKey(incompatibility.IncompatibilityGUID)
#endif
                ;
            }
            if (incompatibilities.Any(func))
            {
                modulesByGuid.Remove(ModuleInfo2.Metadata.GUID);
                dependencyDict.Remove(ModuleInfo2.Metadata.GUID);
                IEnumerable<string> enumerable = ModuleInfo2.Incompatibilities.Select((x) => x.IncompatibilityGUID);
                Func<string, bool> func2 = null;
                if ((func2 = funcE2) == null)
                {
                    func2 = funcE2 = (x) => modulesByGuid.ContainsKey(x);
                }
                IEnumerable<string> enumerable2 = enumerable.Where(func2);
                IEnumerable<string> enumerable3 = ModuleInfo2.Incompatibilities.Select((x) => x.IncompatibilityGUID);
                Func<string, bool> func3 = null;
                if ((func3 = funcE3) == null)
                {
                    func3 = funcE3 = (x) => Modules.ContainsKey(x);
                }
                IEnumerable<string> incompatibleModulesExisting = enumerable3.Where(func3);
                string[] incompatibleModules = enumerable2.Concat(incompatibleModulesExisting).ToArray();
                string message = string.Format("Could not load [{0}] because it is incompatible with: {1}", ModuleInfo2, string.Join(", ", incompatibleModules));
                DependencyErrors.Add(message);
                Logger.Error(message);
            }
            else if (ModuleTargetsWrongTheArchive(ModuleInfo2))
            {
                string message2 = string.Format("Module [{0}] targets a wrong version of TheArchive ({1}) and might not work until you update", ModuleInfo2, ModuleInfo2.TargettedTheArchiveVersion);
                DependencyErrors.Add(message2);
                Logger.Warning(message2);
            }
        }
        var emptyDependencies = Array.Empty<string>();
        return (from x in Utils.TopologicalSort(dependencyDict.Keys, delegate (string x)
        {
            if (!dependencyDict.TryGetValue(x, out var deps))
            {
                return emptyDependencies;
            }
            return deps;
        }).ToList().Where(new Func<string, bool>(modulesByGuid.ContainsKey))
                select modulesByGuid[x]).ToList();
    }

    public void Execute()
    {
        try
        {
            IList<ModuleInfo> modules = DiscoverModules();
            Logger.Info($"{modules.Count} module{(modules.Count == 1 ? "" : "s")} to load");
            LoadModules(modules);
            Action finished = Finished;
            if (finished != null)
            {
                finished();
            }
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
        IEnumerable<ModuleInfo> enumerable = ModifyLoadOrder(modules);
        HashSet<string> invalidModules = new HashSet<string>();
        Dictionary<string, Version> processedModules = new Dictionary<string, Version>();
        Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();
        List<ModuleInfo> loadedModules = new List<ModuleInfo>();
        foreach (ModuleInfo module in enumerable)
        {
            bool dependsOnInvalidModule = false;
            List<ArchiveDependency> missingDependencies = new List<ArchiveDependency>();
            foreach (ArchiveDependency dependency in module.Dependencies)
            {
                bool dependencyExists = processedModules.TryGetValue(dependency.DependencyGUID, out var moduleVersion);
                if (!dependencyExists)
                {
                    dependencyExists = Modules.TryGetValue(dependency.DependencyGUID, out var ModuleInfo);
                    moduleVersion = ModuleInfo != null ? ModuleInfo.Metadata.Version : null;
                    if (!dependencyExists || moduleVersion == null)
                    {
#if R_BIE
                        dependencyExists = BepInEx.Unity.IL2CPP.IL2CPPChainloader.Instance.Plugins.TryGetValue(dependency.DependencyGUID, out var pluginInfo);
                        moduleVersion = pluginInfo != null ? pluginInfo.Metadata.Version : null;
#endif
                    }
                }
                if (!dependencyExists || dependency.VersionRange != null && !dependency.VersionRange.IsSatisfied(moduleVersion, false))
                {
                    if (IsHardDependency(dependency))
                    {
                        missingDependencies.Add(dependency);
                    }
                }
                else if (invalidModules.Contains(dependency.DependencyGUID) && IsHardDependency(dependency))
                {
                    dependsOnInvalidModule = true;
                    break;
                }
            }
            processedModules.Add(module.Metadata.GUID, module.Metadata.Version);
            if (dependsOnInvalidModule)
            {
                string message = string.Format("Skipping [{0}] because it has a dependency that was not loaded. See previous errors for details.", module);
                DependencyErrors.Add(message);
                Logger.Warning(message);
            }
            else if (missingDependencies.Count != 0)
            {
                string message2 = string.Format("Could not load [{0}] because it has missing dependencies: {1}", module, string.Join(", ", missingDependencies.Select(delegate (ArchiveDependency s)
                {
                    if (!(s.VersionRange == null))
                    {
                        return string.Format("{0} ({1})", s.DependencyGUID, s.VersionRange);
                    }
                    return s.DependencyGUID;
                }).ToArray()));
                DependencyErrors.Add(message2);
                Logger.Error(message2);
                invalidModules.Add(module.Metadata.GUID);
            }
            else
            {
                try
                {
                    if (!loadedAssemblies.TryGetValue(module.Location, out var ass))
                    {
                        ass = loadedAssemblies[module.Location] = Assembly.LoadFrom(module.Location);
                    }
                    Modules[module.Metadata.GUID] = module;
                    TryRunModuleCtor(module, ass);
                    module.Instance = LoadModule(module, ass);
                    loadedModules.Add(module);
                    Action<ModuleInfo> moduleLoaded = ModuleLoaded;
                    if (moduleLoaded != null)
                    {
                        moduleLoaded(module);
                    }
                }
                catch (Exception ex)
                {
                    invalidModules.Add(module.Metadata.GUID);
                    Modules.Remove(module.Metadata.GUID);

                    Logger.Error($"Error loading [{module}]:");
                    Logger.Exception(ex);
                }
            }
        }
        return loadedModules;
    }

    public IList<ModuleInfo> LoadModule(params string[] modulesPaths)
    {
        List<ModuleInfo> modules = new List<ModuleInfo>();
        foreach (string modulesPath in modulesPaths)
        {
            modules.AddRange(DiscoverModulesFrom(modulesPath, "TheArchive_ModuleChainloader"));
        }
        return LoadModules(modules);
    }

    private static void TryRunModuleCtor(ModuleInfo module, Assembly assembly)
    {
        try
        {
            RuntimeHelpers.RunModuleConstructor(assembly.GetType(module.TypeName).Module.ModuleHandle);
        }
        catch (Exception ex)
        {
            Logger.Error($"Couldn't run Module constructor for {assembly.FullName}::{module.TypeName}:");
            Logger.Exception(ex);
        }
    }

    public IArchiveModule LoadModule(ModuleInfo moduleInfo, Assembly moduleAssembly)
    {
        return ArchiveMod.CreateAndInitModule(moduleAssembly.GetType(moduleInfo.TypeName));
    }

    internal static bool IsHardDependency(ArchiveDependency dep) => (dep.Flags & ArchiveDependency.DependencyFlags.HardDependency) > 0;

    protected static readonly string CurrentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    protected static readonly System.Version CurrentAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

    public static ArchiveModuleChainloader Instance { get; private set; }
}