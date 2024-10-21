using Gear;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.Fixes
{
    [EnableFeatureByDefault]
    internal class BioTrackerStuckSoundFix : Feature
    {
        public override string Name => "Bio Stuck Sound Fix";

        public override FeatureGroup Group => FeatureGroups.Fixes;

        public override string Description => "Stops the tagging progress sound <u>after unwielding the tracker</u>, just in case the sound gets stuck.";



        private static uint? _BIOTRACKER_TAGGING_CHARGE_FINISHED_ID = null;
        private static uint BIOTRACKER_TAGGING_CHARGE_FINISHED_ID => _BIOTRACKER_TAGGING_CHARGE_FINISHED_ID ??= SoundEventCache.Resolve(nameof(AK.EVENTS.BIOTRACKER_TAGGING_CHARGE_FINISHED));

        [ArchivePatch(typeof(EnemyScanner), nameof(EnemyScanner.OnUnWield))]
        internal static class EnemyScanner_OnUnWield_Patch
        {
            public static void Prefix(EnemyScanner __instance)
            {
                if (BIOTRACKER_TAGGING_CHARGE_FINISHED_ID == 0)
                    return;
                __instance.Sound.SafePost(BIOTRACKER_TAGGING_CHARGE_FINISHED_ID);
                __instance.Sound.Stop();
            }
        }
    }
}
