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

        internal static ArchiveMod Instance;

        private IArchiveModule _module;

        private static ArchivePatcher Patcher;

        public static RundownID CurrentRundown { get; private set; } = RundownID.RundownUnknown;


        public override void OnApplicationStart()
        {
            Instance = this;

            var gameTypeString = MelonUtils.IsGameIl2Cpp() ? "IL2CPP" : "Mono";

            Patcher = new ArchivePatcher(HarmonyInstance, $"Patcher_{gameTypeString}");

            var module = LoadModule();

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

        private Assembly LoadModule()
        {
            try
            {
                if(MelonUtils.IsGameIl2Cpp())
                {
                    ArchiveLogger.Notice("Loading IL2CPP module ...");
                    return Assembly.Load(Utils.LoadFromResource("TheArchive.Core.Resources.TheArchive.IL2CPP.dll"));
                }

                ArchiveLogger.Notice("Loading MONO module ...");
                return Assembly.Load(Utils.LoadFromResource("TheArchive.Core.Resources.TheArchive.MONO.dll"));
            }
            catch (Exception)
            {
                ArchiveLogger.Warning("Could not load module!");
                return null;
            }
        }
/*
        [HarmonyPatch(typeof(SteamManager), "Awake")]
        internal static class SteamManager_AwakePatch
        {
            public static void Postfix()
            {
                MelonLogger.Msg(ConsoleColor.DarkMagenta, $"Steam Manager has awoken.");
            }

        }

        [HarmonyPatch(typeof(StartMainGame), "Awake")]
        internal static class StartMainGame_AwakePatch
        {
            public static void Postfix()
            {
                var rundownId = Global.RundownIdToLoad;
                //Global.RundownIdToLoad = 17;

#if !RD001
                if(rundownId != 17)
                    AllowFullRundown();
#endif

                OnAfterStartMainGameAwake?.Invoke(rundownId);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void AllowFullRundown()
            {
                Global.AllowFullRundown = true;
            }
        }

        [HarmonyPatch(typeof(GlobalSetup), "Awake")]
        internal static class GlobalSetup_AwakePatch
        {
            public static void Prefix(GlobalSetup __instance)
            {
                //__instance.m_allowFullRundown = true;
            }
        }*/
    }
}
