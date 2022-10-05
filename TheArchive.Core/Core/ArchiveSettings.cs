namespace TheArchive.Core
{
    public class ArchiveSettings
    {
        public string CustomFileSaveLocation { get; set; } = string.Empty;
        public string CustomLogsAndCacheLocation { get; set; } = string.Empty;
        public bool DumpDataBlocks { get; set; } = true;
        public bool AlwaysOverrideDataBlocks { get; set; } = false;
        public bool FeatureDevMode { get; set; } = false;
    }
}
