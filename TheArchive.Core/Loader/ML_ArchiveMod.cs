#if MelonLoader
using MelonLoader;
using System;
using TheArchive;
using TheArchive.Loader;

[assembly: MelonInfo(typeof(ML_ArchiveMod), ArchiveMod.MOD_NAME, ArchiveMod.VERSION_STRING, ArchiveMod.AUTHOR, ArchiveMod.GITHUB_LINK)]
[assembly: MelonGame("10 Chambers Collective", "GTFO")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("UnhollowerBaseLib")]
namespace TheArchive.Loader
{
    public class ML_ArchiveMod : MelonMod
    {
        public override void OnEarlyInitializeMelon()
        {
            MelonEvents.OnApplicationStart.Subscribe(() =>
            {
                ArchiveMod.OnApplicationStart(LoaderWrapper.WrapLogger(LoggerInstance), HarmonyInstance);
            }, 1000);
        }

        public override void OnApplicationQuit()
        {
            ArchiveMod.OnApplicationQuit();
        }

        public override void OnUpdate()
        {
            ArchiveMod.OnUpdate();
        }

        public override void OnLateUpdate()
        {
            ArchiveMod.OnLateUpdate();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            ArchiveMod.OnSceneWasLoaded(buildIndex, sceneName);
        }
    }
}
#endif
