#if MelonLoader
using MelonLoader;
using System.Runtime.CompilerServices;
using TheArchive;
using TheArchive.Loader;

[assembly: MelonInfo(typeof(ML_ArchiveMod), ArchiveMod.MOD_NAME, ArchiveMod.VERSION_STRING, ArchiveMod.AUTHOR, ArchiveMod.GITHUB_LINK)]
[assembly: MelonGame("10 Chambers Collective", "GTFO")]
[assembly: MelonColor(0, 104, 4, 168)]
[assembly: MelonOptionalDependencies("TheArchive.Core")]
namespace TheArchive.Loader
{
    public class ML_ArchiveMod : MelonMod
    {
        public override void OnEarlyInitializeMelon()
        {
            CoreModLoader.LoadMainModASM();

            MelonEvents.OnApplicationStart.Subscribe(() =>
            {
                ApplicationStart();
            }, -1);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ApplicationStart()
        {
            LoaderCore.OnApplicationStart(LoggerInstance, HarmonyInstance);
        }

        public override void OnApplicationQuit()
        {
            LoaderCore.OnApplicationQuit();
        }

        public override void OnUpdate()
        {
            LoaderCore.OnUpdate();
        }

        public override void OnLateUpdate()
        {
            LoaderCore.OnLateUpdate();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            LoaderCore.OnSceneWasLoaded(buildIndex, sceneName);
        }
    }
}
#endif
