using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using TheArchive.Core;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Managers;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive
{
    public static class ArchiveMod
    {
        public const string GUID = "dev." + AUTHOR + ".gtfo." + MOD_NAME;
        public const string MOD_NAME = "TheArchive";
        public const string ABBREVIATION = "Ar";
        public const string AUTHOR = "AuriRex";
        public const string VERSION_STRING = ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch;
        public const string GITHUB_LINK = "https://github.com/AuriRex/GTFO_TheArchive";
        public static readonly bool GIT_IS_DIRTY = ThisAssembly.Git.IsDirty;
        public const string GIT_COMMIT_SHORT_HASH = ThisAssembly.Git.Commit;
        public const string GIT_COMMIT_DATE = ThisAssembly.Git.CommitDate;

        public const string MTFO_GUID = "com.dak.MTFO";

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
                    _jsonSerializerSettings.Converters.Add(new Core.FeaturesAPI.Components.FButton.FButtonConverter());
                }
                return _jsonSerializerSettings;
            }
        }

        public static bool IsPlayingModded { get; private set; } = false;
        public static RundownID CurrentRundown { get; private set; } = RundownID.RundownUnitialized;
        public static GameBuildInfo CurrentBuildInfo { get; private set; }
        public static int CurrentGameState { get; private set; }
        public static bool IsOnALTBuild { get; private set; }
        /// <summary>
        /// The currently selected rundown on the rundown screen.<br/>
        /// Is equal to <seealso cref="string.Empty"/> on the "Select Rundown" screen.<br/>
        /// </summary>
        public static string CurrentlySelectedRundownKey
        {
            get => _currentlySelectedRundownKey;
            set
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

        public static List<IArchiveModule> Modules { get; private set; } = new List<IArchiveModule>();
        public static Type IL2CPP_BaseType { get; private set; } = null;

        private const string kArchiveSettingsFile = "TheArchive_Settings.json";

        private static HarmonyLib.Harmony _harmonyInstance;

        public static void OnApplicationStart(IArchiveLogger logger, HarmonyLib.Harmony harmonyInstance)
        {
            
            ArchiveLogger.logger = LoaderWrapper.CreateLoggerInstance(MOD_NAME, ConsoleColor.DarkMagenta);
            _harmonyInstance = harmonyInstance;

            PrintVersion();

#if IL2CPP
            IL2CPP_BaseType = typeof(Il2CppSystem.Object);
#endif

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
                File.WriteAllText(steam_appidtxt, "493520");
            }

            FeatureManager.Internal_Init();

            var archiveModule = LoadMainArchiveModule(LoaderWrapper.IsGameIL2CPP());

            var moduleMainType = archiveModule.GetTypes().First(t => typeof(IArchiveModule).IsAssignableFrom(t));

            _mainModule = CreateAndInitModule(moduleMainType);

            try
            {
                ApplyPatches(CurrentRundown);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }
        }

        private static void PrintVersion()
        {
            ArchiveLogger.Info("--------------------------------------");
            ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"{MOD_NAME}");
            ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"v{VERSION_STRING}");
            ArchiveLogger.Msg(ConsoleColor.Green, $"Commit-Hash: {GIT_COMMIT_SHORT_HASH}");
            ArchiveLogger.Msg(ConsoleColor.Cyan, $"Commit-Date: {GIT_COMMIT_DATE}");
            if (GIT_IS_DIRTY)
            {
                ArchiveLogger.Warning("Git is dirty, this is a development build!");
            }
            ArchiveLogger.Info("--------------------------------------");
        }

        public static void OnApplicationQuit()
        {
            FeatureManager.Instance.OnApplicationQuit();
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

        public static void RegisterArchiveModule(Assembly asm)
        {
            foreach(var type in asm.GetTypes().Where(t => typeof(IArchiveModule).IsAssignableFrom(t)))
            {
                RegisterArchiveModule(type);
            }
        }

        public static void RegisterArchiveModule(Type moduleType)
        {
            RegisterModule(moduleType);
        }

        public static bool RegisterModule(Type moduleType)
        {
            if (moduleType == null) throw new ArgumentException("Module can't be null!");
            if (_moduleTypes.Contains(moduleType)) throw new ArgumentException($"Module \"{moduleType.Name}\" is already registered!");
            if (!typeof(IArchiveModule).IsAssignableFrom(moduleType)) throw new ArgumentException($"Type \"{moduleType.Name}\" does not implement {nameof(IArchiveModule)}!");

            var module = CreateAndInitModule(moduleType);

            OnNewModuleRegistered?.Invoke(module);

            if (CurrentRundown != RundownID.RundownUnitialized)
            {
                module.Patcher?.PatchRundownSpecificMethods(module.GetType().Assembly);
                LoadSubModulesFrom(moduleType);
                return true;
            }

            return false;
        }


        private static void LoadSubModules()
        {
            ArchiveLogger.Info("Loading all SubModules ...");
            foreach (var moduleType in new List<Type>(_moduleTypes))
            {
                LoadSubModulesFrom(moduleType);
            }
        }

        private static HashSet<Type> _submodulesLoadedFrom = new HashSet<Type>();

        private static void LoadSubModulesFrom(Type moduleType)
        {
            if (_submodulesLoadedFrom.Contains(moduleType))
                return;

            foreach (var prop in moduleType.GetProperties())
            {
                if (prop.PropertyType != typeof(string) || !prop.GetMethod.IsStatic) continue;

                var subModuleAttribute = prop.GetCustomAttribute<SubModuleAttribute>();

                if (subModuleAttribute == null) continue;

                if (FlagsContain(subModuleAttribute.Rundowns, CurrentRundown))
                {
                    string subModuleResourcePath = (string) prop.GetValue(null);

                    byte[] subModule = null;
                    try
                    {
                        subModule = GetResource(moduleType.Assembly, subModuleResourcePath);
                    }
                    catch (Exception ex)
                    {
                        ArchiveLogger.Error($"Module \"{moduleType.FullName}\" ({moduleType.Assembly.GetName().Name}) has invalid SubModule at path \"{subModuleResourcePath}\": {ex}: {ex.Message}\n{ex.StackTrace}");
                        continue;
                    }

                    if (subModule == null || subModule.Length < 100)
                    {
                        ArchiveLogger.Error($"Module \"{moduleType.FullName}\" ({moduleType.Assembly.GetName().Name}) has invalid SubModule at path \"{subModuleResourcePath}\"!");
                        continue;
                    }

                    try
                    {
                        var subModuleAssembly = Assembly.Load(subModule);

                        var subModuleType = subModuleAssembly.GetTypes().First(t => typeof(IArchiveModule).IsAssignableFrom(t));

                        RegisterModule(subModuleType);
                    }
                    catch(Exception ex)
                    {
                        ArchiveLogger.Error($"Loading of sub-module at resource path \"{subModuleResourcePath}\" from module \"{moduleType.FullName}\" failed! {ex}: {ex.Message}\n{ex.StackTrace}");
                    }

                    break;
                }
            }
            _submodulesLoadedFrom.Add(moduleType);
        }

        [Obsolete("Do not call!")]
        public static void InvokeGameDataInitialized()
        {
            ArchiveLogger.Info($"GameData has been initialized, invoking event.");
            //CurrentRundownInt = rundownId;

            try
            {
                PresenceFormatter.Setup();
            }
            catch (Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }

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

        [Obsolete("Do not call!")]
        public static void InvokeDataBlocksReady()
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

            DataBlocksReady?.Invoke();
        }

        [Obsolete("Do not call!")]
        public static void InvokeGameStateChanged(int eGameState_state)
        {
            CurrentGameState = eGameState_state;
            GameStateChanged?.Invoke(eGameState_state);
        }

        [Obsolete("Do not call!")]
        public static void InvokeApplicationFocusChanged(bool focus)
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
            if (initSingletonbase_Type.IsAssignableFrom(type))
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

                init = conditional.InitCondition();
            }

            if (init)
            {
                try
                {
                    instance.Init();
                    initSingletonbase_Type.GetProperty(nameof(InitSingletonBase<String>.HasBeenInitialized), AnyBindingFlagss).SetValue(null, true);
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Error($"{nameof(IInitializable.Init)} method on Type \"{type.FullName}\" failed!");
                    ArchiveLogger.Exception(ex);
                }
            }
        }

        private static void InspectType(Type type)
        {
            if(typeof(Feature).IsAssignableFrom(type) && type != typeof(Feature))
            {
                FeatureManager.Instance.InitFeature(type);
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

        private static IArchiveModule CreateAndInitModule(Type moduleType)
        {
            if (moduleType == null) throw new ArgumentException($"Parameter {nameof(moduleType)} can not be null!");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _moduleTypes.Add(moduleType);
            ArchiveLogger.Info($"Initializing module \"{moduleType.FullName}\" ...");
            var module = (IArchiveModule) Activator.CreateInstance(moduleType);

            foreach(var type in moduleType.Assembly.GetTypes())
            {
                InspectType(type);
            }

            _moduleAssemblies.Add(moduleType.Assembly);

            if(module.UsesLegacyPatches)
                module.Patcher = new ArchiveLegacyPatcher(_harmonyInstance, $"{moduleType.Assembly.GetName().Name}_{moduleType.FullName}_ArchivePatcher");

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

        private static void ApplyPatches(RundownID rundownID)
        {
            if (rundownID != RundownID.RundownUnitialized)
            {
                foreach (var module in Modules)
                {
                    module.Patcher?.PatchRundownSpecificMethods(module.GetType().Assembly);
                }
                LoadSubModules();
            }
        }

        internal static void UnpatchAll()
        {
            foreach (var module in Modules)
            {
                UnpatchModule(module);
            }
        }

        public static void UnpatchModule(Type moduleType)
        {
            if (!_moduleTypes.Contains(moduleType)) throw new ArgumentException($"Can't unpatch non patched module \"{moduleType.FullName}\".");

            foreach (var module in Modules)
            {
                if (module.GetType() == moduleType)
                {
                    UnpatchModule(module);
                    return;
                }
            }

            throw new ArgumentException($"Can't unpatch module \"{moduleType.FullName}\", module not found.");
        }

        public static void UnpatchModule(IArchiveModule module)
        {
            try
            {
                module.Patcher?.Unpatch();
                module.OnExit();
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Error while trying to unpatch and/or run {nameof(IArchiveModule.OnExit)} in module \"{module?.GetType()?.FullName ?? "Unknown"}\"!");
                ArchiveLogger.Exception(ex);
            }
            Modules.Remove(module);
            _moduleTypes.Remove(module.GetType());
        }

        public static void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            foreach(var module in Modules)
            {
                try
                {
                    module?.OnSceneWasLoaded(buildIndex, sceneName);
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Error($"Error while trying to run {nameof(OnSceneWasLoaded)} in module \"{module?.GetType()?.FullName ?? "Unknown"}\"!");
                    ArchiveLogger.Exception(ex);
                }
            }
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

        private static Assembly LoadMainArchiveModule(bool isIl2Cpp)
        {
            try
            {
                byte[] bytes;
                if(isIl2Cpp)
                {
                    ArchiveLogger.Notice("Loading IL2CPP module ...");
                    bytes = File.ReadAllBytes(Path.Combine(LocalFiles.ModulesPath, "TheArchive.IL2CPP.dll"));

                    //bytes = Utils.LoadFromResource("TheArchive.Resources.TheArchive.IL2CPP.dll");
                    if (bytes.Length < 100) throw new BadImageFormatException("IL2CPP Module is too small, this version might not contain the module build but a dummy dll!");
                    return Assembly.Load(bytes);
                }

                ArchiveLogger.Notice("Loading MONO module ...");

                //bytes = Utils.LoadFromResource("TheArchive.Resources.TheArchive.MONO.dll");
                bytes = File.ReadAllBytes(Path.Combine(LocalFiles.ModulesPath, "TheArchive.MONO.dll"));
                if (bytes.Length < 100) throw new BadImageFormatException("MONO Module is too small, this version might not contain the module build but a dummy dll!");
                
                
                return Assembly.Load(bytes);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Could not load {(isIl2Cpp ? "IL2CPP" : "MONO")} module! {ex}: {ex.Message}");
                ArchiveLogger.Error($"{ex.StackTrace}");
                return null;
            }
        }
    }
}
