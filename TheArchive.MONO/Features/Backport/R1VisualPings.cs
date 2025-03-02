using LevelGeneration;
using Player;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Backport
{
    [RundownConstraint(RundownFlags.RundownOne)]
    public class R1VisualPings : Feature
    {
        public override string Name => "Visual Ping Indicators in R1";

        public override string Description => "Visualize terminal pings in R1 by abusing the local players Middle-Mouse-Ping.\n(Only works as Host)";

        public override FeatureGroup Group => FeatureGroups.Backport;

#if MONO
        private static MethodAccessor<PlayerAgent> A_PlayerAgent_TriggerMarkerPing;
        private static ManualPingTarget CustomTerminalPingQoLInstance = null;

        public override void Init()
        {
            A_PlayerAgent_TriggerMarkerPing = MethodAccessor<PlayerAgent>.GetAccessor("TriggerMarkerPing");

            CustomTerminalPingQoLInstance = new ManualPingTarget();
            CustomTerminalPingQoLInstance.PingTargetStyle = eNavMarkerStyle.PlayerPingTerminal;
        }

        [ArchivePatch(typeof(LG_GenericTerminalItem), "PlayPing")]
        internal static class LG_GenericTerminalItem_PlayPingPatch
        {
            public static void Prefix(ref LG_GenericTerminalItem __instance)
            {
                if (!SNetwork.SNet.IsMaster)
                {
                    return;
                }

                var localPlayerAgent = PlayerManager.GetLocalPlayerAgent();

                try
                {
                    var bounds = __instance.gameObject.GetMaxBounds();

                    A_PlayerAgent_TriggerMarkerPing.Invoke(localPlayerAgent, (iPlayerPingTarget)CustomTerminalPingQoLInstance, bounds.center);
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
        }
#endif
    }
}
