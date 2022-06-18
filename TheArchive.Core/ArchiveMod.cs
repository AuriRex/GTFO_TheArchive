using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TheArchive;
using TheArchive.Core;
using TheArchive.Core.Managers;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

[assembly: MelonInfo(typeof(ArchiveMod), "TheArchive", ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch, "AuriRex", "https://github.com/AuriRex/GTFO_TheArchive")]
[assembly: MelonGame("10 Chambers Collective", "GTFO")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("System.Runtime.CompilerServices.Unsafe", "UnhollowerBaseLib")]
namespace TheArchive
{
    public class ArchiveMod : MelonMod
    {
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
                    _jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                }
                return _jsonSerializerSettings;
            }
        }

        public static RundownID CurrentRundown { get; private set; } = RundownID.RundownUnitialized;
        public static GameBuildInfo CurrentBuildInfo { get; private set; }
        public static int CurrentGameState { get; private set; }

        public event Action<RundownID> GameDataInitialized;
        public event Action DataBlocksReady;
        public event Action<int> GameStateChanged;

        internal static ArchiveMod Instance;
        internal static event Action<IArchiveModule> OnNewModuleRegistered;

        private IArchiveModule _mainModule;

        private readonly HashSet<Assembly> _inspectedAssemblies = new HashSet<Assembly>();
        private readonly HashSet<Type> _typesToInitOnDataBlocksReady = new HashSet<Type>();
        private readonly HashSet<Type> _typesToInitOnGameDataInit = new HashSet<Type>();
        private readonly HashSet<IInitializable> _iinitializablesToInjectOnGameDataInit = new HashSet<IInitializable>();

        private readonly HashSet<Assembly> _moduleAssemblies = new HashSet<Assembly>();
        private readonly List<Type> _moduleTypes = new List<Type>();

        private readonly List<IArchiveModule> _modules = new List<IArchiveModule>();
        private const string kArchiveSettingsFile = "TheArchive_Settings.json";

        public override void OnApplicationStart()
        {
            Instance = this;

            LoadConfig();

            GTFOLogger.Logger = new MelonLogger.Instance("GTFO-Internals", ConsoleColor.DarkGray);

            CurrentRundown = BuildDB.GetCurrentRundownID(LocalFiles.BuildNumber);
            ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"Current game revision determined to be {LocalFiles.BuildNumber}! ({CurrentRundown})");

            CurrentBuildInfo = new GameBuildInfo
            {
                BuildNumber = LocalFiles.BuildNumber,
                Rundown = CurrentRundown
            };

            FeatureManager.Internal_Init();

            var archiveModule = LoadMainArchiveModule(MelonUtils.IsGameIl2Cpp());

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

        public override void OnApplicationQuit()
        {
            DiscordManager.OnApplicationQuit();
            FeatureManager.Instance.OnApplicationQuit();
            // Doesn't work properly anyways ...
            // UnpatchAll();

            base.OnApplicationQuit();
        }

        private void LoadConfig()
        {
            var path = Path.Combine(MelonUtils.UserDataDirectory, kArchiveSettingsFile);

            LoadConfig(path);


            if (Settings.UseCommonArchiveSettingsFile && !string.IsNullOrWhiteSpace(Settings.CustomFileSaveLocation))
            {
                path = Path.Combine(Settings.CustomFileSaveLocation, kArchiveSettingsFile);
                ArchiveLogger.Notice($"Loading common config from \"{path}\" instead!");
                LoadConfig(path);
            }
        }

        private void LoadConfig(string path)
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

        private void SaveConfig(string path)
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
            Instance.RegisterModule(moduleType);
        }

        public bool RegisterModule(Type moduleType)
        {
            if (moduleType == null) throw new ArgumentException("Module can't be null!");
            if (_moduleTypes.Contains(moduleType)) throw new ArgumentException($"Module \"{moduleType.Name}\" is already registered!");
            if (!typeof(IArchiveModule).IsAssignableFrom(moduleType)) throw new ArgumentException($"Type \"{moduleType.Name}\" does not implement {nameof(IArchiveModule)}!");

            var module = CreateAndInitModule(moduleType);

            OnNewModuleRegistered?.Invoke(module);

            if (CurrentRundown != RundownID.RundownUnitialized)
            {
                module.Patcher.PatchRundownSpecificMethods(module.GetType().Assembly);
                LoadSubModulesFrom(moduleType);
                return true;
            }

            return false;
        }


        private void LoadSubModules()
        {
            ArchiveLogger.Info("Loading all SubModules ...");
            foreach (var moduleType in new List<Type>(_moduleTypes))
            {
                LoadSubModulesFrom(moduleType);
            }
        }

        private HashSet<Type> _submodulesLoadedFrom = new HashSet<Type>();

        private void LoadSubModulesFrom(Type moduleType)
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
        public void InvokeGameDataInitialized(uint rundownId)
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

            /*var rundown = Utils.IntToRundownEnum((int) rundownId);
            ApplyPatches(rundown);*/

            GameDataInitialized?.Invoke(CurrentRundown);
        }

        [Obsolete("Do not call!")]
        public void InvokeDataBlocksReady()
        {
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

            DataBlocksReady?.Invoke();
        }

        [Obsolete("Do not call!")]
        public void InvokeGameStateChanged(int eGameState_state)
        {
            CurrentGameState = eGameState_state;
            GameStateChanged?.Invoke(eGameState_state);
        }



        public void InjectInstanceIntoModules(object instance)
        {
            if (instance == null) return;

            foreach (var module in _modules)
            {
                InjectInstanceIntoModules(instance, module);
            }
        }

        private void InjectInstanceIntoModules(object instance, IArchiveModule module)
        {
            foreach (var prop in module.GetType().GetProperties().Where(p => p.SetMethod != null && !p.SetMethod.IsStatic && p.PropertyType.IsAssignableFrom(instance.GetType())))
            {
                prop.SetValue(module, instance);
            }
        }

        private void InitInitializables(Type type, out IInitializable initializable)
        {
            ArchiveLogger.Debug($"Creating instance of: \"{type.FullName}\".");
            var instance = (IInitializable) Activator.CreateInstance(type);
            initializable = instance;

            bool init = true;

            if (typeof(IInitCondition).IsAssignableFrom(type))
            {
                var conditional = (IInitCondition)instance;

                init = conditional.InitCondition();
            }

            if (init)
            {
                //ArchiveLogger.Info($"Initializing instance of type \"{type.FullName}\", Interfaces:[{string.Join(",", type.GetInterfaces().Select(x => x.FullName))}]");
                instance.Init();
            }
            else
            {
                //ArchiveLogger.Info($"NOT Initializing instance of type \"{type.FullName}\", Interfaces:[{string.Join(",", type.GetInterfaces().Select(x => x.FullName))}], {nameof(IInitCondition.InitCondition)} returned false");
            }
        }

        private void InspectType(Type type)
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

        private IArchiveModule CreateAndInitModule(Type moduleType)
        {
            if (moduleType == null) throw new ArgumentException($"Parameter {nameof(moduleType)} can not be null!");

            _moduleTypes.Add(moduleType);
            ArchiveLogger.Info($"Initializing module \"{moduleType.FullName}\" ...");
            var module = (IArchiveModule) Activator.CreateInstance(moduleType);

            foreach(var type in moduleType.Assembly.GetTypes())
            {
                InspectType(type);
            }

            _moduleAssemblies.Add(moduleType.Assembly);

            module.Patcher = new ArchivePatcher(HarmonyInstance, $"{moduleType.Assembly.GetName().Name}_{moduleType.FullName}_ArchivePatcher");
            module.Core = this;

            try
            {
                module.Init();

                if (module.ApplyHarmonyPatches)
                {
                    ArchiveLogger.Warning($"Applying regular Harmony patches on module \"{moduleType.FullName}\" ...");
                    HarmonyInstance.PatchAll(moduleType.Assembly);
                }
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"Error while trying to init \"{moduleType.FullName}\"!");
                ArchiveLogger.Exception(ex);
            }

            _modules.Add(module);
            return module;
        }

        private void ApplyPatches(RundownID rundownID)
        {
            if (rundownID != RundownID.RundownUnitialized)
            {
                foreach (var module in _modules)
                {
                    module.Patcher.PatchRundownSpecificMethods(module.GetType().Assembly);
                }
                LoadSubModules();
            }
        }

        internal void UnpatchAll()
        {
            foreach (var module in _modules)
            {
                UnpatchModule(module);
            }
        }

        public void UnpatchModule(Type moduleType)
        {
            if (!_moduleTypes.Contains(moduleType)) throw new ArgumentException($"Can't unpatch non patched module \"{moduleType.FullName}\".");

            foreach (var module in _modules)
            {
                if (module.GetType() == moduleType)
                {
                    UnpatchModule(module);
                    return;
                }
            }

            throw new ArgumentException($"Can't unpatch module \"{moduleType.FullName}\", module not found.");
        }

        public void UnpatchModule(IArchiveModule module)
        {
            try
            {
                module.Patcher.Unpatch();
                module.OnExit();
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Error while trying to unpatch and/or run {nameof(IArchiveModule.OnExit)} in module \"{module?.GetType()?.FullName ?? "Unknown"}\"!");
                ArchiveLogger.Exception(ex);
            }
            _modules.Remove(module);
            _moduleTypes.Remove(module.GetType());
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            foreach(var module in _modules)
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
            base.OnSceneWasLoaded(buildIndex, sceneName);
        }

        public override void OnUpdate()
        {
            FeatureManager.Instance.OnUpdate();
        }

        public override void OnLateUpdate()
        {
            foreach (var module in _modules)
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

        private Assembly LoadMainArchiveModule(bool isIl2Cpp)
        {
            try
            {
                byte[] bytes;
                if(isIl2Cpp)
                {
                    ArchiveLogger.Notice("Loading IL2CPP module ...");
                    bytes = Utils.LoadFromResource("TheArchive.Resources.TheArchive.IL2CPP.dll");
                    if (bytes.Length < 100) throw new BadImageFormatException("IL2CPP Module is too small, this version might not contain the module build but a dummy dll!");
                    return Assembly.Load(bytes);
                }

                ArchiveLogger.Notice("Loading MONO module ...");
                bytes = Utils.LoadFromResource("TheArchive.Resources.TheArchive.MONO.dll");
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
