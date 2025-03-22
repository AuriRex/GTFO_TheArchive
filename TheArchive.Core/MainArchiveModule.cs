using System.Collections.Generic;
using GameData;
using LevelGeneration;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Localization;
using TheArchive.Utilities;
using UnityEngine.CrashReportHandler;

[assembly: ModDefaultFeatureGroupName("TheArchive")]

namespace TheArchive;

[ArchiveModule(ArchiveMod.GUID, ArchiveMod.MOD_NAME, ArchiveMod.VERSION_STRING)]
public class MainArchiveModule : IArchiveModule
{
    static MainArchiveModule()
    {
        typeof(EnemyDataBlock).RegisterSelf();
        typeof(GameDataBlockBase<>).RegisterSelf();
        typeof(GameDataBlockWrapper<>).RegisterSelf();
        typeof(eGameStateName).RegisterSelf();
        typeof(LG_Area).RegisterSelf();
        typeof(Il2CppSystem.Collections.Generic.List<>).RegisterForIdentifier("GenericList");
    }
    
    public string ModuleGroup => ArchiveMod.ARCHIVE_CORE_FEATUREGROUP;

    public Dictionary<Language, string> ModuleGroupLanguages => new()
    {
        { Language.English, "Archive Core" },
        { Language.Chinese, "Archive 核心" }
    };
    
    public void Init()
    {
        CrashReportHandler.SetUserMetadata("Modded", "true");
        CrashReportHandler.enableCaptureExceptions = false;
    }

    public void OnLateUpdate()
    {
        
    }
}