using MelonLoader;
using System;
using TheArchive;
using TheArchive.Utilities;

[assembly: MelonInfo(typeof(ML_ArchiveMod), ArchiveMod.MOD_NAME, ArchiveMod.VERSION_STRING, ArchiveMod.AUTHOR, ArchiveMod.GITHUB_LINK)]
[assembly: MelonGame("10 Chambers Collective", "GTFO")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("System.Runtime.CompilerServices.Unsafe", "UnhollowerBaseLib")]
namespace TheArchive
{
    public class ML_ArchiveMod : MelonMod
    {
        public override void OnApplicationStart()
        {
            ArchiveMod.OnApplicationStart(LoaderWrapper.WrapLogger(LoggerInstance), HarmonyInstance);
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
