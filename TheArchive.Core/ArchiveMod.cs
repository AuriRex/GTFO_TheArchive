using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using TheArchive.Core;
using TheArchive.Core.Bootstrap;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Interop;
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

namespace TheArchive;

/// <summary>
/// Archive core mod behaviour
/// </summary>
public static class ArchiveMod
{
    /// <summary> Mod GUID </summary>
    public const string GUID = "dev." + AUTHOR + ".gtfo." + MOD_NAME;
    /// <summary> Mode name </summary>
    public const string MOD_NAME = "TheArchive";
    /// <summary> Shortform for 'Archive' </summary>
    public const string ABBREVIATION = "Ar";
    /// <summary> Author </summary>
    public const string AUTHOR = "AuriRex";
    /// <summary> Git version string </summary>
    public const string VERSION_STRING = ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch;
    /// <summary> GitHub repository name </summary>
    public const string GITHUB_REPOSITORY_NAME = "GTFO_TheArchive";
    /// <summary> GitHub repository owner </summary>
    public const string GITHUB_OWNER_NAME = "AuriRex";
    /// <summary> GitHub repository link </summary>
    public const string GITHUB_LINK = $"https://github.com/{GITHUB_OWNER_NAME}/{GITHUB_REPOSITORY_NAME}";
    /// <summary> Git is dirty </summary>
    public static readonly bool GIT_IS_DIRTY = ThisAssembly.Git.IsDirty;
    /// <summary> Git commit hash short </summary>
    public const string GIT_COMMIT_SHORT_HASH = ThisAssembly.Git.Commit;
    /// <summary> Git commit date </summary>
    public const string GIT_COMMIT_DATE = ThisAssembly.Git.CommitDate;
    /// <summary> Git base tag </summary>
    public const string GIT_BASE_TAG = ThisAssembly.Git.BaseTag;
    /// <summary> The games Steam App-ID </summary>
    public const uint GTFO_STEAM_APPID = 493520;

    /// <summary> MTFO GUID </summary>
    public const string MTFO_GUID = "com.dak.MTFO";

    /// <summary> Main feature group name </summary>
    public const string ARCHIVE_CORE_FEATUREGROUP = "Archive Core";

    /// <summary> Path to the main TheArchive.Core.dll assembly </summary>
    public static readonly string CORE_PATH = Assembly.GetAssembly(typeof(ArchiveMod))!.Location;

    internal static ArchiveSettings Settings { get; private set; } = new ArchiveSettings();

    private static JsonSerializerSettings _jsonSerializerSettings = null;
    internal static JsonSerializerSettings JsonSerializerSettings
    {
        get
        {
            if (_jsonSerializerSettings != null)
                return _jsonSerializerSettings;
            
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };
            _jsonSerializerSettings.Converters.Add(new JsonLib.Converters.StringEnumConverter());
            _jsonSerializerSettings.ContractResolver = ArchiveContractResolver.Instance;
            return _jsonSerializerSettings;
        }
    }

    /// <summary> If we are playing a modded rundown (MTFO is installed). </summary>
    public static bool IsPlayingModded { get; private set; } = false;
    
    /// <summary> The currently running game version </summary> <seealso cref="RundownID"/> <seealso cref="BuildDB"/>
    public static RundownID CurrentRundown { get; private set; } = RundownID.RundownUnitialized;
    
    /// <summary> The currently running game version </summary> <seealso cref="RundownID"/> <seealso cref="BuildDB"/>
    public static GameBuildInfo CurrentBuildInfo { get; private set; }
    
    /// <summary> Current game state as an int, cast to <c>eGameStateName</c> </summary>
    public static int CurrentGameState { get; private set; }
    
    /// <summary> Are we running on a game version after OG R7? </summary>
    public static bool IsOnALTBuild { get; private set; }
    
    private static bool IsInitialized { get; set; }
    
    /// <summary>
    /// The currently selected rundown on the rundown screen.
    /// </summary>
    /// <remarks>
    /// Is equal to <c>string.Empty</c> on the "Select Rundown" screen.
    /// </remarks>
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
    /// <remarks>
    /// Is equal to <c>0</c> on the "Select Rundown" screen.
    /// </remarks>
    public static uint CurrentlySelectedRundownPersistentID { get; set; } = 0;


    /// <summary>
    /// Called once game data has been initialized.<br/>
    /// Right after <c>GameDataInit.Initialize</c> has finished running.
    /// </summary>
    public static event Action<RundownID> GameDataInitialized;
    
    /// <summary>
    /// Called once all datablocks (and specifically localization) have been loaded.
    /// </summary>
    public static event Action DataBlocksReady;
    
    /// <summary>
    /// Called any time the game state changes.
    /// </summary>
    /// <remarks>
    /// Cast the received int value to <c>eGameStateName</c>.
    /// </remarks>
    public static event Action<int> GameStateChanged;
    
    /// <summary>
    /// Called any time the games focus state changes.
    /// </summary>
    public static event Action<bool> ApplicationFocusStateChanged;

    internal static event Action<IArchiveModule> OnNewModuleRegistered;

    private static IArchiveModule _mainModule;
    
    private static readonly HashSet<Type> _typesToInitOnDataBlocksReady = new HashSet<Type>();
    private static readonly HashSet<Type> _typesToInitOnGameDataInit = new HashSet<Type>();

    private static readonly HashSet<Assembly> _moduleAssemblies = new HashSet<Assembly>();
    private static readonly List<Type> _moduleTypes = new List<Type>();

    /// <summary> All registered Archive modules </summary>
    public static HashSet<IArchiveModule> Modules { get; } = new HashSet<IArchiveModule>();
    
    /// <summary> <c>typeof(Il2CppSystem.Object)</c> </summary>
    public static Type IL2CPP_BaseType { get; private set; } = null;

    private const string ARCHIVE_SETTINGS_FILE = "TheArchive_Settings.json";

    private static HarmonyLib.Harmony _harmonyInstance;

    internal static void OnApplicationStart(IArchiveLogger logger, HarmonyLib.Harmony harmonyInstance)
    {
        ArchiveLogger.Logger = logger;
        _harmonyInstance = harmonyInstance;

        if(GIT_IS_DIRTY)
        {
            ArchiveLogger.Warning("Git is dirty, this is a development build!");
        }

        
        if(LoaderWrapper.IsGameIL2CPP())
        {
            IL2CPP_BaseType = ImplementationManager.FindTypeInCurrentAppDomain("Il2CppSystem.Object", exactMatch: true);

            ArchiveLogger.Debug($"IL2CPP_BaseType: {IL2CPP_BaseType?.FullName}");

            if (IL2CPP_BaseType == null)
            {
                ArchiveLogger.Error("IL2CPP base type \"Il2CppSystem.Object\" could not be resolved!");
            }
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

        var steamAppidTxtFilePath = Path.Combine(LoaderWrapper.GameDirectory, "steam_appid.txt");
        if(!File.Exists(steamAppidTxtFilePath))
        {
            ArchiveLogger.Notice("Creating \"steam_appid.txt\" in GTFO folder ...");
            File.WriteAllText(steamAppidTxtFilePath, $"{GTFO_STEAM_APPID}");
        }

        AddInternalAttribution();
        
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
                if (lex == null)
                    continue;
                
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
        var path = Path.Combine(LocalFiles.ModLocalLowPath, ARCHIVE_SETTINGS_FILE);

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

    /// <summary>
    /// Finds the first type that inherits <c>IArchiveModule</c> in the given assembly and initializes it.
    /// </summary>
    /// <param name="asm">The assembly to search.</param>
    /// <returns><c>True</c> if archive mod setup has already run.</returns>
    /// <seealso cref="RegisterArchiveModule(System.Type)"/>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool RegisterArchiveModule(Assembly asm)
    {
        var moduleType = asm.GetTypes().FirstOrDefault(t => typeof(IArchiveModule).IsAssignableFrom(t));
        return RegisterArchiveModule(moduleType);
    }

    /// <summary>
    /// Registers and initializes an <c>IArchiveModule</c> type.
    /// </summary>
    /// <param name="moduleType">The <c>IArchiveModule</c> type.</param>
    /// <returns><c>True</c> if archive mod setup has already run.</returns>
    /// <remarks>
    /// <list>
    /// <item>A module type can only be initialized once.</item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentException"></exception>
    public static bool RegisterArchiveModule(Type moduleType)
    {
        if (moduleType == null) throw new ArgumentException("Module can't be null!");
        if (_moduleTypes.Contains(moduleType)) throw new ArgumentException($"Module \"{moduleType.Name}\" is already registered!");
        if (!typeof(IArchiveModule).IsAssignableFrom(moduleType)) throw new ArgumentException($"Type \"{moduleType.Name}\" does not implement {nameof(IArchiveModule)}!");

        var module = CreateAndInitModule(moduleType);
        
        SafeInvoke(OnNewModuleRegistered, module);

        if (CurrentRundown != RundownID.RundownUnitialized)
        {
            return true;
        }

        return false;
    }

    private static void InitializeArchiveModuleChainloader()
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
            try
            {
                InitInitializables(type, out _);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Trying to Init \"{type.FullName}\" threw an exception:");
                ArchiveLogger.Exception(ex);
            }
        }

        FeatureManager.Instance.OnGameDataInitialized();

        SafeInvoke(GameDataInitialized, CurrentRundown);
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
            try
            {
                InitInitializables(type, out _);
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"Trying to Init \"{type.FullName}\" threw an exception:");
                ArchiveLogger.Exception(ex);
            }
        }

        FeatureManager.Instance.OnDatablocksReady();
        CustomSettingManager.OnGameDataInited();
        Interop.OnDataBlocksReady();

        SafeInvoke(DataBlocksReady);
    }

    internal static void InvokeGameStateChanged(int eGameState_state)
    {
        CurrentGameState = eGameState_state;
        SafeInvoke(GameStateChanged, eGameState_state);
    }

    internal static void InvokeApplicationFocusChanged(bool focus)
    {
        SafeInvoke(ApplicationFocusStateChanged, focus);
    }

    private static void InitInitializables(Type type, out IInitializable initializable)
    {
        ArchiveLogger.Debug($"Creating instance of: \"{type.FullName}\".");
        var instance = (IInitializable) Activator.CreateInstance(type);
        initializable = instance;

        var doInit = true;

        var initSingletonbase_Type = typeof(InitSingletonBase<>).MakeGenericType(type);
        var isInitSingleton = initSingletonbase_Type.IsAssignableFrom(type);
        if (isInitSingleton)
        {
            initSingletonbase_Type.GetProperty(nameof(InitSingletonBase<string>.Instance), AnyBindingFlagss)!.SetValue(null, instance);
        }

        if (typeof(IInitCondition).IsAssignableFrom(type))
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var conditional = (IInitCondition)instance;

            try
            {
                doInit = conditional!.InitCondition();
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"{nameof(IInitCondition.InitCondition)} method on Type \"{type.FullName}\" failed!");
                ArchiveLogger.Warning($"This {nameof(IInitializable)} won't be initialized.");
                ArchiveLogger.Exception(ex);
                doInit = false;
            }
        }

        if (!doInit)
            return;
        
        try
        {
            instance!.Init();
            if(isInitSingleton)
            {
                initSingletonbase_Type.GetProperty(nameof(InitSingletonBase<String>.HasBeenInitialized), AnyBindingFlagss)!.SetValue(null, true);
            }
        }
        catch(Exception ex)
        {
            ArchiveLogger.Error($"{nameof(IInitializable.Init)} method on Type \"{type.FullName}\" failed!");
            ArchiveLogger.Exception(ex);
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
            try
            {
                InitInitializables(type, out _);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Trying to Init \"{type.FullName}\" (immediately) threw an exception:");
                ArchiveLogger.Exception(ex);
            }
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

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static IArchiveModule CreateAndInitModule(Type moduleType)
    {
        if (moduleType == null) throw new ArgumentException($"Parameter {nameof(moduleType)} can not be null!");

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _moduleTypes.Add(moduleType);
        ArchiveLogger.Info($"Initializing module \"{moduleType.FullName}\" ...");
        var module = (IArchiveModule) Activator.CreateInstance(moduleType)!;

        try
        {
            module.Init();
        }
        catch(Exception ex)
        {
            ArchiveLogger.Error($"Error while trying to init \"{moduleType.FullName}\"!");
            ArchiveLogger.Exception(ex);
        }
        
        if (string.IsNullOrWhiteSpace(module.ModuleGroup))
            throw new Exception($"ArchiveModule: {module.GetType().FullName}, {nameof(IArchiveModule.ModuleGroup)} can not be null!");

        FeatureGroups.GetOrCreateModuleGroup(module.ModuleGroup);

        foreach(var type in moduleType.Assembly.GetTypes())
        {
            InspectType(type, module);
        }

        _moduleAssemblies.Add(moduleType.Assembly);

        Modules.Add(module);

        stopwatch.Stop();
        ArchiveLogger.Debug($"Creation of \"{moduleType.FullName}\" took {stopwatch.Elapsed:ss\\.fff} seconds.");

        return module;
    }

    internal static void OnUpdate()
    {
        FeatureManager.Instance.OnUpdate();
    }

    internal static void OnLateUpdate()
    {
        FeatureManager.Instance.OnLateUpdate();
    }

    private static void AddInternalAttribution()
    {
        try
        {
            var archiveLicense = Encoding.UTF8.GetString(LoadFromResource("TheArchive.Resources.LICENSE"));
            Attribution.Add(new Attribution.AttributionInfo("TheArchive License", $"{archiveLicense}")
            {
                Origin = "TheArchive.Core",
                Comment = "<color=orange><b>Huge thanks to everyone that has contributed!</b> - Check out the repo on GitHub!</color>"
            });

            var bepinLicense = Encoding.UTF8.GetString(LoadFromResource("TheArchive.Resources.LICENSE_BepInEx"));
            Attribution.Add(new Attribution.AttributionInfo("BepInEx Info + License", $"This project contains parts of BepInEx code, denoted in source files.\n\nLICENSE (Truncated, see repository):\n\n{bepinLicense}".Substring(0, 619) + "\n\n[...]")
            {
                Origin = "TheArchive.Core"
            });

            var icons =
                "<color=orange>Material Symbols</color> used in ThunderStore mod icons licensed under <color=orange>Apache License Version 2.0</color>\n\n> https://github.com/google/material-design-icons\n> https://www.apache.org/licenses/LICENSE-2.0.txt";
            Attribution.Add(new Attribution.AttributionInfo("Mod Icon(s) Info + License", icons)
            {
                Origin = "TheArchive.Core"
            });
            
            var jbaLicense = Encoding.UTF8.GetString(LoadFromResource("TheArchive.Resources.LICENSE_JBA"));
            Attribution.Add(new Attribution.AttributionInfo("JetBrains.Annotations License", $"{jbaLicense}")
            {
                Origin = "TheArchive.Core"
            });
        }
        catch (Exception ex)
        {
            ArchiveLogger.Error("Error while trying to add internal AttributionInfos");
            ArchiveLogger.Exception(ex);
        }
    }
}