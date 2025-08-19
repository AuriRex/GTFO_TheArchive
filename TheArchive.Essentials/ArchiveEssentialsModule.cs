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
    public const string MOD_NAME = ManifestInfo.TSName;
    public const string VERSION = ManifestInfo.TSVersion;

    public string ModuleGroup => ArchiveMod.ARCHIVE_CORE_FEATUREGROUP;

    public void Init()
    {
            
    }
}