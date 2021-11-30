using MelonLoader;
using System;
using System.Linq;
using System.Reflection;
using TheArchive;
using TheArchive.Core;
using TheArchive.Core.Core;
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

        private IArchiveModule _module;

        private static ArchivePatcher Patcher;

        public static RundownID CurrentRundown { get; private set; } = RundownID.RundownUnknown;


        public override void OnApplicationStart()
        {
            Instance = this;

            var gameTypeString = MelonUtils.IsGameIl2Cpp() ? "IL2CPP" : "Mono";

            Patcher = new ArchivePatcher(HarmonyInstance, $"Patcher_{gameTypeString}");

            var module = LoadModule(MelonUtils.IsGameIl2Cpp());

            var moduleMainType = module.GetTypes().First(t => typeof(IArchiveModule).IsAssignableFrom(t));

            _module = (IArchiveModule) Activator.CreateInstance(moduleMainType);

            _module.Init(Patcher, Instance);

            ArchiveLogger.Warning("Applying regular Harmony patches ...");
            HarmonyInstance.PatchAll(moduleMainType.Assembly);

        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            _module?.OnSceneWasLoaded(buildIndex, sceneName);
            base.OnSceneWasLoaded(buildIndex, sceneName);
        }

        public void SetCurrentRundownAndPatch(RundownID rundownID)
        {
            CurrentRundown = rundownID;

            if(rundownID != RundownID.RundownUnknown)
            {
                Patcher.PatchRundownSpecificMethods(_module.GetType().Assembly);
            }
        }

        public override void OnLateUpdate()
        {
            _module?.OnLateUpdate();

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

    }
}
