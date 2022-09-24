namespace TheArchive.Core
{
    public interface IArchiveModule
    {
        bool ApplyHarmonyPatches { get; }
        bool UsesLegacyPatches { get; }
        ArchiveLegacyPatcher Patcher { get; set; }

        void Init();
        void OnSceneWasLoaded(int buildIndex, string sceneName);
        void OnLateUpdate();
        void OnExit();
    }
}
