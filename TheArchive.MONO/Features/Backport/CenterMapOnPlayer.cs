using CellMenu;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Backport
{
    [RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownTwo)]
    [EnableFeatureByDefault]
    public class CenterMapOnPlayer : Feature
    {
        public override string Name => "Center Map on Player";

        public override string Group => FeatureGroups.Backport;

        public override string Description => "Center the map on yourself upon opening.";

#if MONO
        [ArchivePatch(typeof(CM_PageMap), "OnEnable")]
        internal static class CM_PageMap_OnEnable_Patch
        {
            public static void Postfix(CM_MapPlayerGUIItem[] ___m_syncedPlayers,
                GameObject ___m_mapHolder,
                GameObject ___m_mapMover)
            {
                if (___m_syncedPlayers != null && ___m_syncedPlayers.Length != 0)
                {
                    for (int i = 0; i < ___m_syncedPlayers.Length; i++)
                    {
                        if (___m_syncedPlayers[i].m_localPlayerIcon.gameObject.activeSelf)
                        {
                            Vector3 position = ___m_syncedPlayers[i].transform.position;
                            Vector3 localPosition = ___m_mapHolder.transform.InverseTransformPoint(position);
                            ___m_mapMover.transform.localPosition = ___m_mapMover.transform.localPosition - localPosition;
                        }
                    }
                }
            }
        }
#endif
    }
}
