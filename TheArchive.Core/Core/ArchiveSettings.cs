namespace TheArchive.Core
{
    public class ArchiveSettings
    {

        public bool EnableLocalProgressionPatches { get; set; } = true;
        public bool EnableQualityOfLifeImprovements { get; set; } = true;
        public bool SkipMissionUnlockRequirements { get; set; } = false;
        public bool EnableHudToggle { get; set; } = true;
        public bool DumpDataBlocks { get; set; } = true;
        public bool AlwaysOverrideDataBlocks { get; set; } = false;
        public bool RedirectUnityDebugLogs { get; set; } = true;
        public bool DisableSteamRichPresence { get; set; } = false;
        public bool DisableGameAnalytics { get; set; } = true;
        public bool UnlockAllVanityItems { get; set; } = false;
        public bool EnableDisabledGear { get; set; } = false;

    }
}
