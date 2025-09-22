using Enemies;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Fixes;

[EnableFeatureByDefault]
[RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
internal class BioTrackerSmallRedDotFix : Feature
{
    public override string Name => "Bio Tracker Small Red Dots";

    public override Core.FeaturesAPI.Groups.GroupBase Group => GroupManager.Fixes;

    public override string Description => "Fixes tiny red dots on the bio tracker.";

#if IL2CPP

    [ArchivePatch(typeof(EnemyAgent), nameof(EnemyAgent.ScannerData), patchMethodType: ArchivePatch.PatchMethodType.Setter)]
    internal static class EnemyAgent_ScannerData_Patch
    {
        public static void Postfix(EnemyAgent __instance)
        {
            __instance.m_hasDirtyScannerColor = true;
            var scannerData = __instance.ScannerData;
            if (__instance.Locomotion.CurrentStateEnum != ES_StateEnum.Hibernate) {
                scannerData.m_soundIndex = 0;
            }
        }
    }

#endif

#if MONO
        // TODO
#endif
}