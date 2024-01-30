using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Backport
{
    [EnableFeatureByDefault]
    [RundownConstraint(RundownFlags.RundownOne)]
    public class DontPauseAudio : Feature
    {
        public override string Name => "Don't Pause Audio On Unfocus";

        public override FeatureGroup Group => FeatureGroups.Backport;

        public override string Description => "Audio in R1 was completely paused whenever the game lost focus resulting in sounds piling up and playing on re-focus.";

        public new static IArchiveLogger FeatureLogger { get; set; }

        [ArchivePatch(typeof(AkInitializer), "OnApplicationFocus")]
        internal static class AkInitializer_OnApplicationFocus_Patch
        {
            public static void Prefix(ref bool focus)
            {
                focus = true;
            }
        }
    }
}
