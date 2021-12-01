using AK;
using LevelGeneration;
using Player;
using System;
using System.Reflection;
using TheArchive.Core.Core;
using TheArchive.Utilities;
using UnityEngine;
using static HackingTool;
using static TheArchive.Core.ArchivePatcher;
using static TheArchive.Utilities.Utils;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableQualityOfLifeImprovements), "QOL")]
    public class QualityOfLifePatches
    {
        // add R4 / R5 to the beginning of the header text
        [ArchivePatch(typeof(PlayerGuiLayer), "UpdateObjectiveHeader")]
        internal static class PlayerGuiLayer_UpdateObjectiveHeaderPatch
        {
            public static void Prefix(ref string header)
            {
                header = $"R{ArchiveMod.CurrentRundown.GetIntValue()}{header}";
            }
        }

        // Change hacking minigame to be more in line with newest version of the game -> minigame finishes and hack disappears instantly
        [ArchivePatch(typeof(HackingTool), "UpdateHackSequence", RundownFlags.RundownFour)]
        internal static class HackingTool_UpdateHackSequencePatch
        {
            private static MethodInfo HackingTool_ClearScreen = typeof(HackingTool).GetMethod("ClearScreen", AnyBindingFlags);
            private static MethodInfo HackingTool_OnStopHacking = typeof(HackingTool).GetMethod("OnStopHacking", AnyBindingFlags);

            public static bool Prefix(ref HackingTool __instance)
            {
                try
                {

                    switch (__instance.m_state)
                    {
                        case HackSequenceState.DoneWait:
                            __instance.m_activeMinigame.EndGame();
                            __instance.m_holoSourceGFX.SetActive(value: false);
                            __instance.Sound.Post(EVENTS.BUTTONGENERICBLIPTHREE);
                            __instance.m_stateTimer = 1f;
                            if (__instance.m_currentHackable != null)
                            {
                                LG_LevelInteractionManager.WantToSetHackableStatus(__instance.m_currentHackable, eHackableStatus.Success, __instance.Owner);
                            }
                            __instance.m_state = HackSequenceState.Done;
                            return false;
                        case HackSequenceState.Done:
                            if (__instance.m_stateTimer < 0f)
                            {
                                HackingTool_ClearScreen.Invoke(__instance, null);
                                HackingTool_OnStopHacking.Invoke(__instance, null);
                                __instance.Sound.Post(EVENTS.BUTTONGENERICSEQUENCEFINISHED);
                                __instance.m_state = HackSequenceState.Idle;
                            }
                            else
                            {
                                __instance.m_stateTimer -= Clock.Delta;
                            }
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    MelonLoader.MelonLogger.Msg(ConsoleColor.Red, $"An error occured: {ex.Message}");
                    MelonLoader.MelonLogger.Msg(ConsoleColor.Red, $"{ex.StackTrace}");
                }
                return true;
            }
        }

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
