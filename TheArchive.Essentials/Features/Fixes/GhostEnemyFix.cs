using Enemies;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;

namespace TheArchive.Features.Fixes;

[EnableFeatureByDefault]
public class GhostEnemyFix : Feature
{
    public override string Name => "Ghost Enemy Fix";

    public override string Description => "Preventing the issue where enemies are killed immediately after spawning, causing their corpses to not be properly disposed of.";

    public override Core.FeaturesAPI.Groups.GroupBase Group => GroupManager.Fixes;

    [ArchivePatch(typeof(EnemyAgent), nameof(EnemyAgent.Alive), patchMethodType: ArchivePatch.PatchMethodType.Setter)]
    private static class EnemyAgent__set_Alive__Patch
    {
        private static void Postfix(EnemyAgent __instance, bool value)
        {
            if (!value || __instance.UpdateMode != NodeUpdateMode.None)
                return;

            EnemyUpdateManager.Current.Register(__instance, __instance.CourseNode.m_enemyUpdateMode);
        }
    }
}
