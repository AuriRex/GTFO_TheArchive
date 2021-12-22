using Player;
using System;
using TheArchive.Core.Core;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.IL2CPP.R5.ArchivePatches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableQualityOfLifeImprovements), "QOL")]
    public class QualityOfLifePatches
    {
        // prioritize resources in ping raycasts
        [ArchivePatch(typeof(PlayerAgent), nameof(PlayerAgent.TriggerMarkerPing))]
        internal static class PlayerAgent_TriggerMarkerPingPatch
        {
            public static void Prefix(PlayerAgent __instance, ref iPlayerPingTarget target, ref Vector3 worldPos)
            {
                try
                {
                    if (__instance == null || !__instance.IsLocallyOwned || target == null)
                    {
                        return;
                    }

                    var rayCastHits = Physics.RaycastAll(__instance.CamPos, __instance.FPSCamera.Forward, 40f, LayerManager.MASK_PING_TARGET, QueryTriggerInteraction.Ignore);

                    if (rayCastHits == null || rayCastHits.Length == 0)
                    {
                        return;
                    }

                    float distanceToLockerOrBox = -1;
                    foreach (var hit in rayCastHits)
                    {
                        var hitTarget = hit.collider.GetComponentInChildren<iPlayerPingTarget>();
                        if (hitTarget == null) continue;
                        //ArchiveLogger.Notice($" -> Hit this: {hit.collider?.gameObject?.name} - {hitTarget?.PingTargetStyle}, distance: {hit.distance}");
                        if (hitTarget?.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceBox || hitTarget?.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceLocker)
                        {
                            distanceToLockerOrBox = hit.distance;
                            break;
                        }
                    }

                    if (distanceToLockerOrBox < 0)
                    {
                        return;
                    }

                    foreach (var hit in rayCastHits)
                    {
                        var hitTarget = hit.collider.GetComponentInChildren<iPlayerPingTarget>();
                        if (hitTarget == null) continue;
                        if (hitTarget.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceBox || hitTarget.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceLocker)
                        {
                            continue;
                        }
                        //ArchiveLogger.Warning($" -> Also Hit this: {hit.collider.gameObject.name} - {hitTarget.PingTargetStyle}, distance: {hit.distance}");
                        if (hit.distance < distanceToLockerOrBox + 1f)
                        {
                            target = hitTarget;
                            worldPos = hit.point;
                            __instance.m_pingTarget = hitTarget;
                            __instance.m_pingPos = hit.point;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Error(ex.Message);
                    ArchiveLogger.Error(ex.StackTrace);
                }
            }
        }

    }
}
