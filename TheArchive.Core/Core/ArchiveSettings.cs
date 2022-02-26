namespace TheArchive.Core
{
    public class ArchiveSettings
    {
        public string LobbyIdFormatString { get; set; } = "LF%OpenSlots% %Rundown%%Expedition% \"%ExpeditionName%\": `%LobbyID%`";
        public string CustomFileSaveLocation { get; set; } = string.Empty;
        public bool UseCommonArchiveSettingsFile { get; set; } = false;
        public bool EnableDiscordRichPresence { get; set; } = true;
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
        public bool EnableLoadoutRandomizer { get; set; } = true;

    }
}
