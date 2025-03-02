using Enemies;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.Fixes;

[EnableFeatureByDefault]
public class NoDeadPings : Feature
{
    public override string Name => "No Dead Pings";

    public override FeatureGroup Group => FeatureGroups.Fixes;

    public override string Description => "Prevents bio pings (<#F00><u>/\\</u></color>) on dead enemies.";


    [ArchivePatch(typeof(EnemyAgent), nameof(EnemyAgent.SyncPlaceNavMarkerTag))]
    internal static class EnemyAgent_SyncPlaceNavMarkerTag_Patch
    {
        private static IValueAccessor<EnemyAgent, bool> _A_m_alive;

        public static void Init()
        {
            _A_m_alive = AccessorBase.GetValueAccessor<EnemyAgent, bool>("m_alive");
        }

        public static bool Prefix(EnemyAgent __instance)
        {
            if (!_A_m_alive.Get(__instance))
            {
                return ArchivePatch.SKIP_OG;
            }

            if (!Is.R6OrLater)
            {
                // R6+ calls RemoveNavMarker before placing again
                __instance.RemoveNavMarker();
            }

            return ArchivePatch.RUN_OG;
        }
    }
}