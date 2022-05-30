namespace TheArchive.Core
{
    public class ArchiveSettings
    {
        public string CustomFileSaveLocation { get; set; } = string.Empty;
        public bool UseCommonArchiveSettingsFile { get; set; } = false;
        public string LobbyIdFormatString { get; set; } = "LF%OpenSlots% %Rundown%%Expedition% \"%ExpeditionName%\": `%LobbyID%`";
        public bool EnableDiscordRichPresence { get; set; } = true;
        public bool EnableLocalProgressionPatches { get; set; } = true;
        public bool EnableQualityOfLifeImprovements { get; set; } = true;
        public bool SkipMissionUnlockRequirements { get; set; } = false;
        public bool EnableHudToggle { get; set; } = true;
        /// <summary> hide your weapon (this also makes you unable to use or switch weapons!!) </summary>
        public bool EnableHideWeapon { get; set; } = false;
        public bool DumpDataBlocks { get; set; } = true;
        public bool AlwaysOverrideDataBlocks { get; set; } = false;
        public bool RedirectUnityDebugLogs { get; set; } = true;
        public bool DisableSteamRichPresence { get; set; } = false;
        public bool DisableGameAnalytics { get; set; } = true;
        public bool UnlockAllVanityItems { get; set; } = false;
        public bool EnableLoadoutRandomizer { get; set; } = true;

        /// <summary> Allows the player to run / jump / fall with charged throwables instead of automatically throwing them. </summary>
        public bool EnableAccidentalThrowablesThrowProtection { get; set; } = true;
        public bool AutoSkipIntro { get; set; } = false;
        /// <summary> Allows you to play with others on the second patch (19087) of the game if you're running the third (final R1 build, 19715) </summary>
        public bool R1SNetRevisionOverride { get; set; } = false;

    }
}
