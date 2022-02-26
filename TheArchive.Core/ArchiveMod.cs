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
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

[assembly: MelonInfo(typeof(ArchiveMod), "TheArchive", "0.1", "AuriRex")]
[assembly: MelonGame("10 Chambers Collective", "GTFO")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("System.Runtime.CompilerServices.Unsafe", "UnhollowerBaseLib")]
namespace TheArchive
{
    public class ArchiveMod : MelonMod
    {
        public static ArchiveSettings Settings { get; private set; } = new ArchiveSettings();

        private static uint _currentRundownInt = 0;
        public static uint CurrentRundownInt
        {
            get
            {
                return _currentRundownInt;
            }
            private set
            {
                _currentRundownInt = value;
                _currentRundown = Utils.IntToRundownEnum((int) value);
            }
        }
        private static RundownID _currentRundown = RundownID.RundownUnitialized;
        public static RundownID CurrentRundown
        {
            get
            {
                return _currentRundown;
            }
        }

        public event Action<RundownID> GameDataInitialized;
        public event Action DataBlocksReady;

        public static bool HudIsVisible { get; set; } = true;

        internal static ArchiveMod Instance;

        private IArchiveModule _mainModule;

        private List<Type> moduleTypes = new List<Type>();

        private List<IArchiveModule> modules = new List<IArchiveModule>();
        private const string kArchiveSettingsFile = "TheArchive_Settings.json";

        public override void OnApplicationStart()
        {
            Instance = this;

            LoadConfig();

            var archiveModule = LoadMainArchiveModule(MelonUtils.IsGameIl2Cpp());

            var moduleMainType = archiveModule.GetTypes().First(t => typeof(IArchiveModule).IsAssignableFrom(t));

            _mainModule = CreateAndInitModule(moduleMainType);
        }

        public override void OnApplicationQuit()
        {
            DiscordManager.OnApplicationQuit();
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
                ArchiveLogger.Info("Loading config file ...");
                
                if (File.Exists(path))
                {
                    Settings = JsonConvert.DeserializeObject<ArchiveSettings>(File.ReadAllText(path));
                }
                File.WriteAllText(path, JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }
            catch (Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }
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
            if (moduleTypes.Contains(moduleType)) throw new ArgumentException($"Module \"{moduleType.Name}\" is already registered!");
            if (!typeof(IArchiveModule).IsAssignableFrom(moduleType)) throw new ArgumentException($"Type \"{moduleType.Name}\" does not implement {nameof(IArchiveModule)}!");

            var module = CreateAndInitModule(moduleType);

            if (CurrentRundown != RundownID.RundownUnitialized)
            {
                module.Patcher.PatchRundownSpecificMethods(module.GetType().Assembly);
                return true;
            }

            return false;
        }


        private void LoadSubModules()
        {
            ArchiveLogger.Info("Loading all SubModules ...");
            foreach (var moduleType in new List<Type>(moduleTypes))
            {
                LoadSubModulesFrom(moduleType);
            }
        }

        private void LoadSubModulesFrom(Type moduleType)
        {
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
        }

        [Obsolete("Do not call!")]
        public void InvokeGameDataInitialized(uint rundownId)
        {
            ArchiveLogger.Info($"GameData has been initialized, invoking event.");
            CurrentRundownInt = rundownId;

            try
            {
                PresenceFormatter.Setup();
                DiscordManager.Setup();
                DiscordManager.UpdateGameState(Core.Models.PresenceGameState.Startup);
            }
            catch(Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }

            var rundown = Utils.IntToRundownEnum((int) rundownId);
            ApplyPatches(rundown);

            GameDataInitialized?.Invoke(CurrentRundown);
        }

        [Obsolete("Do not call!")]
        public void InvokeDataBlocksReady()
        {
            ArchiveLogger.Info($"DataBlocks should be ready to be interacted with, invoking event.");
            DataBlocksReady?.Invoke();
        }

        private IArchiveModule CreateAndInitModule(Type moduleType)
        {
            if (moduleType == null) throw new ArgumentException($"Parameter {nameof(moduleType)} can not be null!");

            moduleTypes.Add(moduleType);
            ArchiveLogger.Info($"Initializing module \"{moduleType.FullName}\" ...");
            var module = (IArchiveModule) Activator.CreateInstance(moduleType);

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

            modules.Add(module);
            return module;
        }

        private void ApplyPatches(RundownID rundownID)
        {
            if (rundownID != RundownID.RundownUnitialized)
            {
                foreach (var module in modules)
                {
                    module.Patcher.PatchRundownSpecificMethods(module.GetType().Assembly);
                }
                LoadSubModules();
            }
        }

        internal void UnpatchAll()
        {
            foreach (var module in modules)
            {
                UnpatchModule(module);
            }
        }

        public void UnpatchModule(Type moduleType)
        {
            if (!moduleTypes.Contains(moduleType)) throw new ArgumentException($"Can't unpatch non patched module \"{moduleType.FullName}\".");

            foreach (var module in modules)
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
            modules.Remove(module);
            moduleTypes.Remove(module.GetType());
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            foreach(var module in modules)
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
            DiscordManager.Update();
        }

        public override void OnLateUpdate()
        {
            foreach (var module in modules)
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
            base.OnLateUpdate();
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
