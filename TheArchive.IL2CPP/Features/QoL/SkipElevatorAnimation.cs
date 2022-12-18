using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;

namespace TheArchive.Features.QoL
{
    internal class SkipElevatorAnimation : Feature
    {
        public override string Name => "Skip Elevator Animation";

        public override string Group => FeatureGroups.QualityOfLife;

        public override string Description => "Automatically skips the elevator intro animation sequence without having to hold down a button.";

        [ArchivePatch(typeof(ElevatorRide), nameof(ElevatorRide.StartPreReleaseSequence))]
        internal static class ElevatorRide_StartPreReleaseSequence_Patch
        {
            public static void Postfix()
            {
                ElevatorRide.SkipPreReleaseSequence();
            }
        }
    }
}
