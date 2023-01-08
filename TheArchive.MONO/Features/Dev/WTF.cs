using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;

namespace TheArchive.Features.Dev
{
    [HideInModSettings]
    [DoNotSaveToConfig]
    [EnableFeatureByDefault]
    public class WTF : Feature
    {
        public override string Name => "GlobalThreadTerminator3400 Terminator";

        public override string Group => FeatureGroups.Dev;

        public override string Description => "This seems to be the little sh*thead who was preventing my code from saving my config files in R1! >:(";

        public new static IArchiveLogger FeatureLogger { get; set; }

        [ArchivePatch(typeof(GlobalThreadTerminator3400), "OnApplicationQuit")]
        internal static class GlobalThreadTerminator3400_OnApplicationQuit_Patch
        {
            public static bool Prefix()
            {
                FeatureLogger.Notice("Terminated GlobalThreadTerminator3400!!!");
                return ArchivePatch.SKIP_OG;
            }
        }
    }
}
