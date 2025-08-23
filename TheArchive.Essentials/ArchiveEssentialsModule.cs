using System.Reflection;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;

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

    public ILocalizationService LocalizationService { get; set; }
    
    public IArchiveLogger Logger { get; set; }

    public void Init()
    {
        
    }
}