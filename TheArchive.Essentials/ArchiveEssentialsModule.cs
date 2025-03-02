using System.Reflection;
using TheArchive.Core;
using TheArchive.Core.Attributes;

[assembly: AssemblyVersion(TheArchive.ArchiveEssentialsModule.VERSION)]
[assembly: AssemblyFileVersion(TheArchive.ArchiveEssentialsModule.VERSION)]
[assembly: AssemblyInformationalVersion(TheArchive.ArchiveEssentialsModule.VERSION)]

namespace TheArchive;

[ArchiveModule(GUID, MOD_NAME, VERSION)]
public class ArchiveEssentialsModule : IArchiveModule
{
    public const string GUID = $"{ArchiveMod.GUID}.Essentials";
    public const string MOD_NAME = $"{ArchiveMod.MOD_NAME}.Essentials";
    public const string VERSION = "0.0.1";

    public bool ApplyHarmonyPatches => false;
    public bool UsesLegacyPatches => false;

    public ArchiveLegacyPatcher Patcher { get; set; }

    public string ModuleGroup => ArchiveMod.ARCHIVE_CORE_FEATUREGROUP;

    public void Init()
    {
            
    }

    public void OnSceneWasLoaded(int buildIndex, string sceneName)
    {

    }

    public void OnLateUpdate()
    {

    }

    public void OnExit()
    {

    }
}