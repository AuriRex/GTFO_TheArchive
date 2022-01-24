using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TheArchive;
using TheArchive.Core;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

[assembly: MelonInfo(typeof(ArchiveMod), "TheArchive", "0.1", "AuriRex")]
[assembly: MelonGame("10 Chambers Collective", "GTFO")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("System.Runtime.CompilerServices.Unsafe")]
namespace TheArchive
{

    public class ArchiveMod : MelonMod
    {

        public static ArchiveSettings Settings { get; private set; } = new ArchiveSettings();

        public static bool HudIsVisible { get; set; } = true;

        internal static ArchiveMod Instance;

        private IArchiveModule _mainModule;

        //private static ArchivePatcher Patcher;

        public static RundownID CurrentRundown { get; private set; } = RundownID.RundownUnitialized;

        private List<Type> moduleTypes = new List<Type>();

        private List<IArchiveModule> modules = new List<IArchiveModule>();

        public bool RegisterModule(Type moduleType)
        {
            if (moduleType == null) throw new ArgumentException("Module can't be null!");
            if (moduleTypes.Contains(moduleType)) throw new ArgumentException($"Module \"{moduleType.Name}\" is already registered!");

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
                        subModule = Utilities.Utils.GetResource(moduleType.Assembly, subModuleResourcePath);
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
                ArchiveLogger.Exception(ex);
            }

            modules.Add(module);
            return module;
        }

        public override void OnApplicationStart()
        {
            Instance = this;

            LoadConfig();

            var module = LoadModule(MelonUtils.IsGameIl2Cpp());

            var moduleMainType = module.GetTypes().First(t => typeof(IArchiveModule).IsAssignableFrom(t));

            _mainModule = CreateAndInitModule(moduleMainType);
        }

        private void LoadConfig()
        {
            try
            {
                ArchiveLogger.Info("Loading config file ...");
                var path = Path.Combine(MelonUtils.UserDataDirectory, "TheArchive_Settings.json");
                if (File.Exists(path))
                {
                    Settings = JsonConvert.DeserializeObject<ArchiveSettings>(File.ReadAllText(path));
                }
                File.WriteAllText(path, JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }
            catch(Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            foreach(var module in modules)
            {
                module?.OnSceneWasLoaded(buildIndex, sceneName);
            }
            base.OnSceneWasLoaded(buildIndex, sceneName);
        }

        public void SetCurrentRundownAndPatch(RundownID rundownID)
        {
            CurrentRundown = rundownID;

            if(rundownID != RundownID.RundownUnitialized)
            {
                foreach(var module in modules)
                {
                    module.Patcher.PatchRundownSpecificMethods(module.GetType().Assembly);
                }
                LoadSubModules();
            }
        }

        internal void UnpatchAll()
        {
            foreach(var module in modules)
            {
                UnpatchModule(module);
            }
        }

        public void UnpatchModule(Type moduleType)
        {
            if (!moduleTypes.Contains(moduleType)) throw new ArgumentException($"Can't unpatch non patched module \"{moduleType.FullName}\".");

            foreach(var module in modules)
            {
                if(module.GetType() == moduleType)
                {
                    UnpatchModule(module);
                    return;
                }
            }

            throw new ArgumentException($"Can't unpatch module \"{moduleType.FullName}\", module not found.");
        }

        public void UnpatchModule(IArchiveModule module)
        {
            module.Patcher.Unpatch();
            module.OnExit();
            modules.Remove(module);
            moduleTypes.Remove(module.GetType());
        }

        public override void OnLateUpdate()
        {
            foreach (var module in modules)
            {
                module?.OnLateUpdate();
            }
            base.OnLateUpdate();
        }

        private Assembly LoadModule(bool isIl2Cpp)
        {
            try
            {
                byte[] bytes;
                if(isIl2Cpp)
                {
                    ArchiveLogger.Notice("Loading IL2CPP module ...");
                    bytes = Utils.LoadFromResource("TheArchive.Core.Resources.TheArchive.IL2CPP.dll");
                    if (bytes.Length < 100) throw new BadImageFormatException("IL2CPP Module is too small, this version might not contain the module build but a dummy dll!");
                    return Assembly.Load(bytes);
                }

                ArchiveLogger.Notice("Loading MONO module ...");
                bytes = Utils.LoadFromResource("TheArchive.Core.Resources.TheArchive.MONO.dll");
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

        public List<Assembly> GetModulesAssemblys()
        {
            List<Assembly> assemblyList = new List<Assembly>();
            foreach(var module in modules)
            {
                assemblyList.Add(module.GetType().Assembly);
            }
            return assemblyList;
        }
    }
}
