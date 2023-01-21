using SNetwork;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Backport
{
    [EnableFeatureByDefault]
    [RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownThree)]
    public class HostReviveFix : Feature
    {
        public override string Name => "Host Revive Fix";

        public override string Group => FeatureGroups.Backport;

        public override string Description => "Fixes \"Host Revive\" where whenever the host / master revives you, you only start with 1% HP.\n(Makes you revive with 20% HP instead!)";

#if MONO
        [ArchivePatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.OnRevive))]
        internal static class Dam_PlayerDamageLocal_OnRevive_Patch_2
        {
            public static void Prefix(Dam_PlayerDamageLocal __instance, SNet_Packet<pSetHealthData> ___m_setHealthPacket)
            {
                pSetHealthData data = new pSetHealthData
                {
                    health = 5f // 20% HP
                };
                ___m_setHealthPacket.Send(data, SNet_SendGroup.PlayersInGame, SNet_SendQuality.Reliable, 1, null);
                __instance.ReceiveSetHealth(data);
            }
        }
#endif
    }
}
