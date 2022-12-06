namespace TheArchive.Core
{
    /// <summary>
    /// Main mod settings data
    /// </summary>
    public class ArchiveSettings
    {
        /// <summary> Overrides the default save folder location </summary>
        public string CustomFileSaveLocation { get; set; } = string.Empty;

        /// <summary> Set a custom folder location for where to save logs, generated weapon images and other cache things to </summary>
        public string CustomLogsAndCacheLocation { get; set; } = string.Empty;

        /// <summary> Dumps the games internal configuration files into the saves folder </summary>
        public bool DumpDataBlocks { get; set; } = false;

        /// <summary> Force a redump of the internal configuration files each game start </summary>
        public bool AlwaysOverrideDataBlocks { get; set; } = false;

        /// <summary> Shows hidden Features in ModSettings </summary>
        public bool FeatureDevMode { get; set; } = false;
    }
}
