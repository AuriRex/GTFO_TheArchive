using System.Collections.Generic;
using GameData;
using LevelGeneration;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine.CrashReportHandler;

[assembly: ModDefaultFeatureGroupName("TheArchive")]

namespace TheArchive;

[ArchiveModule(ArchiveMod.GUID, ArchiveMod.MOD_NAME, ArchiveMod.VERSION_STRING)]
internal class MainArchiveModule : IArchiveModule
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

    public ILocalizationService LocalizationService { get; set; }
    
    public IArchiveLogger Logger { get; set; }

    public void Init()
    {
        CrashReportHandler.SetUserMetadata("Modded", "true");
        CrashReportHandler.enableCaptureExceptions = false;
    }
}