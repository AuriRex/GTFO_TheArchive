using SNetwork;
using TheArchive.Core;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.R1SNetRevisionOverride), "R1-P2-Downgrade")]
    public class R1SNetRevisionOverridePatch
    {
        [BuildConstraint(19715)]
        [ArchivePatch(typeof(SNet), nameof(SNet.Setup), Utils.RundownFlags.RundownOne)]
        internal static class SNet_SetupPatch
        {
            public static void Prefix(ref int gameRevision)
            {
                var revisionOverride = 19087;
                ArchiveLogger.Notice($"SNET : Setting revision to {revisionOverride} (previous:{gameRevision})");
                gameRevision = revisionOverride;
            }
        }
    }
}
