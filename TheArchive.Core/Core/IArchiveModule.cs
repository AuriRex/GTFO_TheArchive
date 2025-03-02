namespace TheArchive.Core;

public interface IArchiveModule
{
    bool ApplyHarmonyPatches { get; }

    string ModuleGroup { get; }

    void Init();
    void OnLateUpdate();
}