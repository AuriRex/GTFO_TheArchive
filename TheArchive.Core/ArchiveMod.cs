using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TheArchive.Core;
using TheArchive.Core.Bootstrap;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Managers;
using TheArchive.Core.Models;
using TheArchive.Core.ModulesAPI;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

[assembly: AssemblyVersion(ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch)]

[assembly: AssemblyFileVersion(ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch)]

[assembly: AssemblyInformationalVersion(
    ThisAssembly.Git.SemVer.Major + "." +
    ThisAssembly.Git.SemVer.Minor + "." +
    ThisAssembly.Git.Commits + "-" +
    ThisAssembly.Git.Branch + "+" +
    ThisAssembly.Git.Commit)]

[assembly: InternalsVisibleTo("TheArchive.RichPresence")]
[assembly: InternalsVisibleTo("TheArchive.MONO")]
namespace TheArchive;

public static class ArchiveMod
{
    public const string GUID = "dev." + AUTHOR + ".gtfo." + MOD_NAME;
    public const string MOD_NAME = "TheArchive";
    public const string ABBREVIATION = "Ar";
    public const string AUTHOR = "AuriRex";
    public const string VERSION_STRING = ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch;
    public const string GITHUB_REPOSITORY_NAME = "GTFO_TheArchive";
    public const string GITHUB_OWNER_NAME = "AuriRex";
    public const string GITHUB_LINK = $"https://github.com/{GITHUB_OWNER_NAME}/{GITHUB_REPOSITORY_NAME}";
    public static readonly bool GIT_IS_DIRTY = ThisAssembly.Git.IsDirty;
    public const string GIT_COMMIT_SHORT_HASH = ThisAssembly.Git.Commit;
    public const string GIT_COMMIT_DATE = ThisAssembly.Git.CommitDate;
    public const string GIT_BASE_TAG = ThisAssembly.Git.BaseTag;
    public const uint GTFO_STEAM_APPID = 493520;

    public const string MTFO_GUID = "com.dak.MTFO";

    public const string ARCHIVE_CORE_FEATUREGROUP = "Archive Core";

    public static readonly string CORE_PATH = Assembly.GetAssembly(typeof(ArchiveMod)).Location;

    public static ArchiveSettings Settings { get; private set; } = new ArchiveSettings();

    private static JsonSerializerSettings _jsonSerializerSettings = null;
    public static JsonSerializerSettings JsonSerializerSettings
    {
        get
        {
            if(_jsonSerializerSettings == null)
            {
                _jsonSerializerSettings = new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                };
                _jsonSerializerSettings.Converters.Add(new JsonLib.Converters.StringEnumConverter());
                _jsonSerializerSettings.ContractResolver = ArchiveContractResolver.Instance;
            }
            return _jsonSerializerSettings;
        }
    }

    public static bool IsPlayingModded { get; private set; } = false;
    public static RundownID CurrentRundown { get; private set; } = RundownID.RundownUnitialized;
    public static GameBuildInfo CurrentBuildInfo { get; private set; }
    public static int CurrentGameState { get; private set; }
    public static bool IsOnALTBuild { get; private set; }
    public static bool IsInitialized { get; private set; } = false;
    /// <summary>
    /// The currently selected rundown on the rundown screen.<br/>
    /// Is equal to <seealso cref="string.Empty"/> on the "Select Rundown" screen.<br/>
    /// </summary>
    public static string CurrentlySelectedRundownKey
    {
        get => _currentlySelectedRundownKey;
        internal set
        {
            _currentlySelectedRundownKey = value;

            ArchiveLogger.Debug($"Setting {nameof(CurrentlySelectedRundownKey)} to \"{_currentlySelectedRundownKey}\".");

            if(string.IsNullOrEmpty(_currentlySelectedRundownKey))
            {
                CurrentlySelectedRundownPersistentID = 0;
                return;
            }

            try
            {
                CurrentlySelectedRundownPersistentID = uint.Parse(_currentlySelectedRundownKey.Replace("Local_", ""));
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"Failed to parse selected rundown persistentId from {nameof(CurrentlySelectedRundownKey)} \"{CurrentlySelectedRundownKey}\"!");
                ArchiveLogger.Exception(ex);
            }
        }
    }
    private static string _currentlySelectedRundownKey = string.Empty;
    /// <summary>
    /// Persistent ID of the currently selected RundownDataBlock on the rundown screen.<br/>
    /// </summary>
    public static uint CurrentlySelectedRundownPersistentID { get; set; } = 0;


    public static event Action<RundownID> GameDataInitialized;
    public static event Action DataBlocksReady;
    public static event Action<int> GameStateChanged;
    public static event Action<bool> ApplicationFocusStateChanged;

    internal static event Action<IArchiveModule> OnNewModuleRegistered;

    private static IArchiveModule _mainModule;

    private static readonly HashSet<Assembly> _inspectedAssemblies = new HashSet<Assembly>();
    private static readonly HashSet<Type> _typesToInitOnDataBlocksReady = new HashSet<Type>();
    private static readonly HashSet<Type> _typesToInitOnGameDataInit = new HashSet<Type>();
    private static readonly HashSet<IInitializable> _iinitializablesToInjectOnGameDataInit = new HashSet<IInitializable>();

    private static readonly HashSet<Assembly> _moduleAssemblies = new HashSet<Assembly>();
    private static readonly List<Type> _moduleTypes = new List<Type>();

    public static HashSet<IArchiveModule> Modules { get; private set; } = new HashSet<IArchiveModule>();
    public static Type IL2CPP_BaseType { get; private set; } = null;

    private const string kArchiveSettingsFile = "TheArchive_Settings.json";

    private static HarmonyLib.Harmony _harmonyInstance;

    internal static void OnApplicationStart(IArchiveLogger logger, HarmonyLib.Harmony harmonyInstance)
    {
        ArchiveLogger.logger = logger;
        _harmonyInstance = harmonyInstance;

        if(GIT_IS_DIRTY)
        {
            ArchiveLogger.Warning("Git is dirty, this is a development build!");
        }

        try
        {
            if(LoaderWrapper.IsGameIL2CPP())
            {
                IL2CPP_BaseType = ImplementationManager.FindTypeInCurrentAppDomain("Il2CppSystem.Object", exactMatch: true);

                ArchiveLogger.Debug($"IL2CPP_BaseType: {IL2CPP_BaseType?.FullName}");

                if (IL2CPP_BaseType == null)
                    throw new Exception();
            }
        }
        catch (Exception ex)
        {
            ArchiveLogger.Error("IL2CPP base type \"Il2CppSystem.Object\" could not be resolved!");
            ArchiveLogger.Exception(ex);
        }

        LoadConfig();

        if(LoaderWrapper.IsModInstalled(MTFO_GUID))
        {
            IsPlayingModded = true;
        }

#if !BepInEx
            HarmonyLib.Tools.Logger.ChannelFilter |= HarmonyLib.Tools.Logger.LogChannel.Error;
            HarmonyLib.Tools.Logger.MessageReceived += (handler, eventArgs) => ArchiveLogger.Error(eventArgs.Message);
#endif

        GTFOLogger.Logger = LoaderWrapper.CreateLoggerInstance("GTFO-Internals", ConsoleColor.DarkGray);

        CurrentRundown = BuildDB.GetCurrentRundownID(BuildDB.BuildNumber);
        ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"Current game revision determined to be {BuildDB.BuildNumber}! ({CurrentRundown})");

        CurrentBuildInfo = new GameBuildInfo
        {
            BuildNumber = BuildDB.BuildNumber,
            Rundown = CurrentRundown
        };

        IsOnALTBuild = CurrentRundown.IsIncludedIn(RundownFlags.RundownAltOne.ToLatest());

        var steam_appidtxt = Path.Combine(LoaderWrapper.GameDirectory, "steam_appid.txt");
        if(!File.Exists(steam_appidtxt))
        {
            ArchiveLogger.Notice("Creating \"steam_appid.txt\" in GTFO folder ...");
            File.WriteAllText(steam_appidtxt, $"{GTFO_STEAM_APPID}");
        }

        FeatureManager.Internal_Init();

        try
        {
            _mainModule = CreateAndInitModule(typeof(MainArchiveModule));
        }
        catch(ReflectionTypeLoadException ex)
        {
            ArchiveLogger.Error("Failed loading main module!!");
            ArchiveLogger.Exception(ex);

            ArchiveLogger.Notice($"Loader Exceptions ({ex.LoaderExceptions.Length}):");
            foreach(var lex in ex.LoaderExceptions)
            {
                ArchiveLogger.Warning(lex.Message);
                ArchiveLogger.Debug(lex.StackTrace);
            }
            ArchiveLogger.Info("-------------");
        }

        InitializeArchiveModuleChainloader();
    }

    internal static void OnApplicationQuit()
    {
        FeatureManager.Instance.OnApplicationQuit();
        CustomSettingManager.OnApplicationQuit();
    }

    private static void LoadConfig()
    {
        var path = Path.Combine(LocalFiles.ModLocalLowPath, kArchiveSettingsFile);

        LoadConfig(path);
    }

    private static void LoadConfig(string path)
    {
        try
        {
            ArchiveLogger.Info($"Loading config file ... [{path}]");
                
            if (File.Exists(path))
            {
                Settings = JsonConvert.DeserializeObject<ArchiveSettings>(File.ReadAllText(path), JsonSerializerSettings);
            }
            SaveConfig(path);
        }
        catch (Exception ex)
        {
            ArchiveLogger.Exception(ex);
        }
    }

    private static void SaveConfig(string path)
    {
        ArchiveLogger.Debug($"Saving config file ... [{path}]");
        File.WriteAllText(path, JsonConvert.SerializeObject(Settings, JsonSerializerSettings));
    }

    public static bool RegisterArchiveModule(Assembly asm)
    {
        foreach (var type in asm.GetTypes().Where(t => typeof(IArchiveModule).IsAssignableFrom(t)))
        {
            if (RegisterArchiveModule(type))
                return true;
        }
        return false;
    }

    public static bool RegisterArchiveModule(Type moduleType)
    {
        if (moduleType == null) throw new ArgumentException("Module can't be null!");
        if (_moduleTypes.Contains(moduleType)) throw new ArgumentException($"Module \"{moduleType.Name}\" is already registered!");
        if (!typeof(IArchiveModule).IsAssignableFrom(moduleType)) throw new ArgumentException($"Type \"{moduleType.Name}\" does not implement {nameof(IArchiveModule)}!");

        var module = CreateAndInitModule(moduleType);
            
        OnNewModuleRegistered?.Invoke(module);

        if (CurrentRundown != RundownID.RundownUnitialized)
        {
            return true;
        }

        return false;
    }

    internal static void InitializeArchiveModuleChainloader()
    {
#if BepInEx
        BepInEx.Unity.IL2CPP.IL2CPPChainloader.Instance.Finished += ArchiveModuleChainloader.Initialize;
#endif
    }

    internal static void InvokeGameDataInitialized()
    {
        if (IsInitialized)
        {
            // Most likely a reload has been triggered via the MTFO `Reload Game Data` button
            ArchiveLogger.Info($"Reload triggered, skipping init.");
            return;
        }

        IsInitialized = true;

        ArchiveLogger.Info($"GameData has been initialized, invoking event.");

        foreach (var type in _typesToInitOnGameDataInit)
        {
            IInitializable instance = null;
            try
            {
                InitInitializables(type, out instance);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Trying to Init \"{type.FullName}\" threw an exception:");
                ArchiveLogger.Exception(ex);
            }
            try
            {
                InjectInstanceIntoModules(instance);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Trying to Inject \"{type.FullName}\" threw an exception:");
                ArchiveLogger.Exception(ex);
            }
        }

        foreach(var iinit in _iinitializablesToInjectOnGameDataInit)
        {
            try
            {
                InjectInstanceIntoModules(iinit);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Trying to Inject \"{iinit.GetType().FullName}\" threw an exception:");
                ArchiveLogger.Exception(ex);
            }
        }

        FeatureManager.Instance.OnGameDataInitialized();

        GameDataInitialized?.Invoke(CurrentRundown);
    }

    internal static void InvokeDataBlocksReady()
    {
        try
        {
            DataBlockManager.Setup();
        }
        catch (Exception ex)
        {
            ArchiveLogger.Exception(ex);
        }

        ArchiveLogger.Info($"DataBlocks should be ready to be interacted with, invoking event.");

        foreach(var type in _typesToInitOnDataBlocksReady)
        {
            IInitializable instance = null;
            try
            {
                InitInitializables(type, out instance);
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"Trying to Init \"{type.FullName}\" threw an exception:");
                ArchiveLogger.Exception(ex);
            }
            try
            {
                InjectInstanceIntoModules(instance);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Trying to Inject \"{type.FullName}\" threw an exception:");
                ArchiveLogger.Exception(ex);
            }
        }

        FeatureManager.Instance.OnDatablocksReady();
        CustomSettingManager.OnGameDataInited();

        DataBlocksReady?.Invoke();
    }

    internal static void InvokeGameStateChanged(int eGameState_state)
    {
        CurrentGameState = eGameState_state;
        GameStateChanged?.Invoke(eGameState_state);
    }

    internal static void InvokeApplicationFocusChanged(bool focus)
    {
        ApplicationFocusStateChanged?.Invoke(focus);
    }


    public static void InjectInstanceIntoModules(object instance)
    {
        if (instance == null) return;

        foreach (var module in Modules)
        {
            InjectInstanceIntoModules(instance, module);
        }
    }

    private static void InjectInstanceIntoModules(object instance, IArchiveModule module)
    {
        foreach (var prop in module.GetType().GetProperties().Where(p => p.SetMethod != null && !p.SetMethod.IsStatic && p.PropertyType.IsAssignableFrom(instance.GetType())))
        {
            prop.SetValue(module, instance);
        }
    }

    private static void InitInitializables(Type type, out IInitializable initializable)
    {
        ArchiveLogger.Debug($"Creating instance of: \"{type.FullName}\".");
        var instance = (IInitializable) Activator.CreateInstance(type);
        initializable = instance;

        bool init = true;

        var initSingletonbase_Type = typeof(InitSingletonBase<>).MakeGenericType(type);
        var isInitSingleton = initSingletonbase_Type.IsAssignableFrom(type);
        if (isInitSingleton)
        {
            initSingletonbase_Type.GetProperty(nameof(InitSingletonBase<String>.Instance), AnyBindingFlagss).SetValue(null, instance);
        }

        if (typeof(IInjectLogger).IsAssignableFrom(type))
        {
            var injectLoggerable = (IInjectLogger)instance;

            injectLoggerable.Logger = LoaderWrapper.CreateArSubLoggerInstance(type.Name, ConsoleColor.Green);
        }

        if (typeof(IInitCondition).IsAssignableFrom(type))
        {
            var conditional = (IInitCondition)instance;

            try
            {
                init = conditional.InitCondition();
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"{nameof(IInitCondition.InitCondition)} method on Type \"{type.FullName}\" failed!");
                ArchiveLogger.Warning($"This {nameof(IInitializable)} won't be initialized.");
                ArchiveLogger.Exception(ex);
                init = false;
            }
        }

        if (init)
        {
            try
            {
                instance.Init();
                if(isInitSingleton)
                {
                    initSingletonbase_Type.GetProperty(nameof(InitSingletonBase<String>.HasBeenInitialized), AnyBindingFlagss).SetValue(null, true);
                }
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"{nameof(IInitializable.Init)} method on Type \"{type.FullName}\" failed!");
                ArchiveLogger.Exception(ex);
            }
        }
    }

    private static void InspectType(Type type, IArchiveModule module)
    {
        if(typeof(Feature).IsAssignableFrom(type) && type != typeof(Feature))
        {
            FeatureManager.Instance.InitFeature(type, module);
            return;
        }

        if (typeof(IInitImmediately).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
        {
            IInitializable instance = null;
            try
            {
                InitInitializables(type, out instance);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Trying to Init \"{type.FullName}\" (immediately) threw an exception:");
                ArchiveLogger.Exception(ex);
            }
            if(instance != null)
                _iinitializablesToInjectOnGameDataInit.Add(instance);
            return;
        }

        if (typeof(IInitAfterGameDataInitialized).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
        {
            _typesToInitOnGameDataInit.Add(type);
            return;
        }

        if (typeof(IInitAfterDataBlocksReady).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
        {
            _typesToInitOnDataBlocksReady.Add(type);
            return;
        }
    }

    internal static IArchiveModule CreateAndInitModule(Type moduleType)
    {
        if (moduleType == null) throw new ArgumentException($"Parameter {nameof(moduleType)} can not be null!");

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _moduleTypes.Add(moduleType);
        ArchiveLogger.Info($"Initializing module \"{moduleType.FullName}\" ...");
        var module = (IArchiveModule) Activator.CreateInstance(moduleType)!;

        if (string.IsNullOrWhiteSpace(module.ModuleGroup))
            throw new Exception($"ArchiveModule: {module.GetType().FullName}, {nameof(IArchiveModule.ModuleGroup)} can not be null!");

        FeatureGroups.GetOrCreateModuleGroup(module.ModuleGroup);

        foreach(var type in moduleType.Assembly.GetTypes())
        {
            InspectType(type, module);
        }

        _moduleAssemblies.Add(moduleType.Assembly);

        try
        {
            module.Init();

            if (module.ApplyHarmonyPatches)
            {
                ArchiveLogger.Warning($"Applying regular Harmony patches on module \"{moduleType.FullName}\" ...");
                _harmonyInstance.PatchAll(moduleType.Assembly);
            }
        }
        catch(Exception ex)
        {
            ArchiveLogger.Error($"Error while trying to init \"{moduleType.FullName}\"!");
            ArchiveLogger.Exception(ex);
        }

        Modules.Add(module);

        stopwatch.Stop();
        ArchiveLogger.Debug($"Creation of \"{moduleType.FullName}\" took {stopwatch.Elapsed:ss\\.fff} seconds.");

        return module;
    }

    public static void OnUpdate()
    {
        FeatureManager.Instance.OnUpdate();
    }

    public static void OnLateUpdate()
    {
        foreach (var module in Modules)
        {
            try
            {
                module?.OnLateUpdate();
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Error while trying to run {nameof(IArchiveModule.OnLateUpdate)} in module \"{module?.GetType()?.FullName ?? "Unknown"}\"!");
                ArchiveLogger.Exception(ex);
            }
        }

        FeatureManager.Instance.OnLateUpdate();
    }
}