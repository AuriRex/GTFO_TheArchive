﻿using System.Collections.Generic;
using TheArchive.Core.Localization;

namespace TheArchive.Core
{
    public interface IArchiveModule
    {
        bool ApplyHarmonyPatches { get; }
        bool UsesLegacyPatches { get; }
        ArchiveLegacyPatcher Patcher { get; set; }

        string ModuleGroup { get; }

        Dictionary<Language, string> ModuleGroupLanguages { get; }

        void Init();
        void OnSceneWasLoaded(int buildIndex, string sceneName);
        void OnLateUpdate();
        void OnExit();
    }
}
