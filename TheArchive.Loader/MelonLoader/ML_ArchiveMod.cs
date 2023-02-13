#if MelonLoader
using MelonLoader;
using System.Runtime.CompilerServices;
using TheArchive;
using TheArchive.Loader;

[assembly: MelonInfo(typeof(ML_ArchiveMod), Info.MOD_NAME, Info.VERSION_STRING, Info.AUTHOR, ArchiveMod.GITHUB_LINK)]
[assembly: MelonGame("10 Chambers Collective", "GTFO")]
[assembly: MelonColor(0, 104, 4, 168)]
[assembly: MelonOptionalDependencies("TheArchive.Core")]
namespace TheArchive.Loader
{
    public class ML_ArchiveMod : MelonMod
    {
        public override void OnEarlyInitializeMelon()
        {
            LoaderCore.LoadMainModASM();

            MelonEvents.OnApplicationStart.Subscribe(ApplicationStart, -1);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ApplicationStart()
        {
            CoreModEvents.OnApplicationStart(LoggerInstance, HarmonyInstance);
        }

        public override void OnApplicationQuit()
        {
            CoreModEvents.OnApplicationQuit();
        }

        public override void OnUpdate()
        {
            CoreModEvents.OnUpdate();
        }

        public override void OnLateUpdate()
        {
            CoreModEvents.OnLateUpdate();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            CoreModEvents.OnSceneWasLoaded(buildIndex, sceneName);
        }
    }
}
#endif
