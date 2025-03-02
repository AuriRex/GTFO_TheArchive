using GameData;
using LevelGeneration;
using System;
using System.Collections.Generic;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Localization;
using TheArchive.Utilities;
using UnityEngine.CrashReportHandler;

[assembly: ModDefaultFeatureGroupName("TheArchive")]
namespace TheArchive
{
    [ArchiveModule(ArchiveMod.GUID, ArchiveMod.MOD_NAME, ArchiveMod.VERSION_STRING)]
    public class ArchiveIL2CPPModule : IArchiveModule
    {
        public bool ApplyHarmonyPatches => false;
        public bool UsesLegacyPatches => false;

        public ArchiveLegacyPatcher Patcher { get; set; }

        public string ModuleGroup => ArchiveMod.ARCHIVE_CORE_FEATUREGROUP;

        public Dictionary<Language, string> ModuleGroupLanguages => new()
        {
            { Language.English, "Archive Core" },
            { Language.Chinese, "Archive 核心" }
        };

       

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
}
