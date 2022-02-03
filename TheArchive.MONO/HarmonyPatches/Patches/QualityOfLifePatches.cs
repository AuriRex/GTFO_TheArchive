using AK;
using ChainedPuzzles;
using LevelGeneration;
using Player;
using System;
using System.Reflection;
using TheArchive.Core;
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
        // Restyle Hacking minigame (success = blue) & fail = Red, smaller & expanding elements
        //MinigameBlock
        /*[ArchivePatch(typeof(HologramGraphics), "Awake")]
        internal static class HologramGraphics_AwakePatch
        {
            public static void Postfix(HologramGraphics __instance)
            {
                __instance.m_spherify = 1f;
            }
        }*/

        // At this point it might be easier to just recreate the R4/5 minigame alltogether ...
        // SetSelectorRows(int width, Color colNeutral)
        /*[ArchivePatch(typeof(HackingMinigame_TimingGrid), "SetSelectorRows")]
        internal static class HackingMinigame_TimingGrid_SetSelectorRowsPatch
        {
            static Color colorWrong = new Color(0.509f, 0.129f, 0.164f, 0.85f);
            static Color colorCorrect = new Color(0f, 0.392f, 0.737f, 0.85f);
            static Color colorHighlight = new Color(0.847f, 0.898f, 0.850f, 1f);
            static FieldInfo FIm_colSelectorRow; // Color
            static FieldInfo FIm_colSelectorRowActive; // Color
            static FieldInfo FIm_moveOffUp; // float
            public static void Prefix(HackingMinigame_TimingGrid __instance, int width, ref Color colNeutral)
            {
                switch(width)
                {
                    case 9:

                        break;
                    case 6:

                        break;
                    default:
                    case 3:

                        break;
                }
                if(FIm_colSelectorRow == null)
                {
                    FIm_colSelectorRow = typeof(HackingMinigame_TimingGrid).GetField("m_colSelectorRow", HarmonyLib.AccessTools.all);
                    FIm_colSelectorRow.SetValue(__instance, colorCorrect);
                }
                if(FIm_colSelectorRowActive == null)
                {
                    FIm_colSelectorRowActive = typeof(HackingMinigame_TimingGrid).GetField("m_colSelectorRowActive", HarmonyLib.AccessTools.all);
                    FIm_colSelectorRowActive.SetValue(__instance, colorHighlight);
                }
                if(FIm_moveOffUp == null)
                {
                    FIm_moveOffUp = typeof(HackingMinigame_TimingGrid).GetField("m_moveOffUp", HarmonyLib.AccessTools.all);
                    FIm_moveOffUp.SetValue(__instance, .23f);
                }
                
                
                colNeutral = colorWrong;
            }
        }*/

        // Fix ladder movement so that W is always upwards and S always downwards no matter where you're looking
        [ArchivePatch(typeof(LG_Ladder), "GetMoveVec", RundownFlags.RundownOne)]
        internal static class LG_Ladder_GetMoveVecPatch
        {
            public static bool Prefix(ref Vector3 __result, Vector3 camDir, float axisVertical)
            {
                __result = Vector3.ClampMagnitude(Vector3.up * axisVertical, 1f);

                return false;
            }
        }

        // Use the player ping (Middle Mouse Ping) for terminal ping command pings in R1
#warning TODO: Fix bug where it doesn't check for the terminal user to avoid multiple visual ping indicators if more than one player has the patch enabled.
        [ArchivePatch(typeof(LG_GenericTerminalItem), "PlayPing", RundownFlags.RundownOne)]
        internal static class LG_GenericTerminalItem_PlayPingPatch
        {
            private static MethodInfo PlayerAgent_TriggerMarkerPing = typeof(PlayerAgent).GetMethod("TriggerMarkerPing", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            private static ManualPingTarget CustomTerminalPingQoLInstance = null;

            public static void Prefix(ref LG_GenericTerminalItem __instance)
            {
                var localPlayerAgent = GuiManager.PlayerLayer.m_player; //(PlayerAgent) MusicManager_m_localPlayer.GetValue(MusicManager.Current);

                if (CustomTerminalPingQoLInstance == null)
                {
                    CustomTerminalPingQoLInstance = new ManualPingTarget();
                    CustomTerminalPingQoLInstance.PingTargetStyle = eNavMarkerStyle.PlayerPingTerminal;
                }

                MelonLoader.MelonLogger.Msg(ConsoleColor.Red, "Ping patch executing!");
                try
                {
                    var bounds = Utilities.MonoUtils.GetMaxBounds(__instance.gameObject);

                    PlayerAgent_TriggerMarkerPing.Invoke(localPlayerAgent, new object[] { (iPlayerPingTarget) CustomTerminalPingQoLInstance, bounds.center });
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
        }

        // Change the "WARDEN OBJECTIVE" text in the top left of the screen to the current selected mission, ex: "R1A1:The Admin"
        [ArchivePatch(typeof(PlayerGuiLayer), "UpdateObjectives", RundownFlags.RundownOne, RundownFlags.RundownThree)] 
        internal static class PlayerGuiLayer_UpdateObjectivesPatch
        {
            public static void Postfix(ref PlayerGuiLayer __instance, ref PUI_GameObjectives ___m_wardenObjective)
            {
                try
                {
                    if (RundownManager.ActiveExpedition == null) return;

                    pActiveExpedition activeExpeditionData = RundownManager.GetActiveExpeditionData();

                    string rundownPrefix = string.Empty;

                    switch(ArchiveMod.CurrentRundown)
                    {
                        case RundownID.RundownTwo:
                        case RundownID.RundownThree:
                            break;
                        default:
                            rundownPrefix = $"R{ArchiveMod.CurrentRundown.GetIntValue()}";
                            break;
                    }

                    ___m_wardenObjective.m_header.text = $"{rundownPrefix}{RundownManager.ActiveExpedition.Descriptive.Prefix}{activeExpeditionData.expeditionIndex + 1}:{RundownManager.ActiveExpedition.Descriptive.PublicName}";
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
        }

        // Change hacking minigame to be more in line with newest version of the game -> minigame finishes and hack disappears instantly
        [ArchivePatch(typeof(HackingTool), "UpdateHackSequence", RundownFlags.RundownOne, RundownFlags.RundownThree)]
        internal static class HackingTool_UpdateHackSequencePatch
        {
            private static MethodInfo HackingTool_ClearScreen = typeof(HackingTool).GetMethod("ClearScreen", AnyBindingFlags);
            private static MethodInfo HackingTool_OnStopHacking = typeof(HackingTool).GetMethod("OnStopHacking", AnyBindingFlags);

            public static bool Prefix(ref HackingTool __instance, ref HackSequenceState ___m_state, ref iHackable ___m_currentHackable, ref float ___m_stateTimer, ref iHackingMinigame ___m_activeMinigame, ref GameObject ___m_holoSourceGFX)
            {
                try
                {

                    switch (___m_state)
                    {
                        case HackSequenceState.DoneWait:
                            ___m_activeMinigame.EndGame();
                            ___m_holoSourceGFX.SetActive(value: false);
                            __instance.Sound.Post(EVENTS.BUTTONGENERICBLIPTHREE);
                            ___m_stateTimer = 1f;
                            if (___m_currentHackable != null)
                            {
                                // Rundown 2 added HackingMiss to the enum pushing Success one back so this should give the correct value and not make you unable to hack anything in R1 ...
                                eHackableStatus status = ArchiveMod.CurrentRundown == RundownID.RundownOne ? eHackableStatus.HackingMiss : eHackableStatus.Success;
                                LG_LevelInteractionManager.WantToSetHackableStatus(___m_currentHackable, status, __instance.Owner);
                            }
                            ___m_state = HackSequenceState.Done;
                            return false;
                        case HackSequenceState.Done:
                            if (___m_stateTimer < 0f)
                            {
                                HackingTool_ClearScreen.Invoke(__instance, null);
                                HackingTool_OnStopHacking.Invoke(__instance, null);
                                __instance.Sound.Post(EVENTS.BUTTONGENERICSEQUENCEFINISHED);
                                /*if (m_currentHackable != null)
                                {
                                    LG_LevelInteractionManager.WantToSetHackableStatus(m_currentHackable, eHackableStatus.Success, base.Owner);
                                }*/
                                ___m_state = HackSequenceState.Idle;
                            }
                            else
                            {
                                ___m_stateTimer -= Clock.Delta;
                            }
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
                return true;
            }
        }

        // Add alarm classes to security door interaction text
        [ArchivePatch(typeof(LG_SecurityDoor_Locks), "OnDoorState", RundownFlags.RundownOne | RundownFlags.RundownTwo)]
        internal static class LG_SecurityDoor_Locks_OnDoorStatePatch
        {
            // iChainedPuzzleCore[]
            static FieldInfo ChainedPuzzleInstance_m_chainedPuzzleCores = typeof(ChainedPuzzleInstance).GetField("m_chainedPuzzleCores", AnyBindingFlags);

            public static void Postfix(LG_SecurityDoor_Locks __instance, pDoorState state, ref Interact_Timed ___m_intOpenDoor)
            {
                try
                {
                    switch (state.status)
                    {
                        case eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm:
                            if (__instance.ChainedPuzzleToSolve == null) break;
                            var puzzles = (iChainedPuzzleCore[]) ChainedPuzzleInstance_m_chainedPuzzleCores.GetValue(__instance.ChainedPuzzleToSolve);

                            if (puzzles == null) break;
                            var value = Utils.ToRoman(puzzles.Length);
                            ___m_intOpenDoor.SetInteractionMessage($"Start Security Scan Sequence <color=red>[WARNING:CLASS {value} ALARM DETECTED]</color>");
                            break;
                    }
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
        }

        // R2 and up like hack fail effect (might be a bit stricter than the officially implemented one because it's essentially the same as hitting your hammer on the box)
        [ArchivePatch(typeof(HackingMinigame_TimingGrid), "OnMiss", RundownFlags.RundownOne)]
        internal static class HackingMinigame_TimingGrid_OnMissPatch
        {
            public static void Postfix(ref HackingTool ___m_tool)
            {
                ___m_tool.Owner.Noise = Agents.Agent.NoiseType.MeleeHit;
                ___m_tool.Sound.Post(EVENTS.SENTRYGUN_ACTIVATED, ___m_tool.transform.position);
            }
        }

        // prioritize resources in ping raycasts
        [ArchivePatch(typeof(CrosshairGuiLayer), "ShowPingIndicator")]
        internal static class CrosshairGuiLayer_ShowPingIndicatorPatch
        {
            internal static bool ShouldRun { get; set; } = false;
            internal static bool WouldHaveRun { get; set; } = false;
            public static bool Prefix()
            {
                if(!ShouldRun)
                    WouldHaveRun = true;
                return ShouldRun;
            }
        }

        // prioritize resources in ping raycasts
        [ArchivePatch(typeof(PlayerAgent), "LateUpdate")]
        internal static class PlayerAgent_LateUpdatePatch
        {
            
            private static void PlayPingSfxAndStuff(iPlayerPingTarget ___m_pingTarget)
            {
                CrosshairGuiLayer_ShowPingIndicatorPatch.ShouldRun = true;
                GuiManager.CrosshairLayer.ShowPingIndicator(___m_pingTarget != null);
                CrosshairGuiLayer_ShowPingIndicatorPatch.ShouldRun = false;
                CrosshairGuiLayer_ShowPingIndicatorPatch.WouldHaveRun = false;
            }

            public static void Postfix(PlayerAgent __instance, ref iPlayerPingTarget ___m_pingTarget, ref Vector3 ___m_pingPos)
            {
                try
                {
                    if (__instance == null || !__instance.IsLocallyOwned || ___m_pingTarget == null)
                    {
                        return;
                    }

                    if (CrosshairGuiLayer_ShowPingIndicatorPatch.WouldHaveRun && !CrosshairGuiLayer_ShowPingIndicatorPatch.ShouldRun)
                    {
                        var rayCastHits = Physics.RaycastAll(__instance.CamPos, __instance.FPSCamera.Forward, 40f, LayerManager.MASK_PING_TARGET, QueryTriggerInteraction.Ignore);

                        if (rayCastHits == null || rayCastHits.Length == 0)
                        {
                            PlayPingSfxAndStuff(___m_pingTarget);
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
                            PlayPingSfxAndStuff(___m_pingTarget);
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
                                //ArchiveLogger.Msg(ConsoleColor.DarkRed, $"Set this as new ping target!");
                                ___m_pingTarget = hitTarget;
                                ___m_pingPos = hit.point;
                                break;
                            }
                        }

                        CrosshairGuiLayer_ShowPingIndicatorPatch.ShouldRun = true;
                        GuiManager.CrosshairLayer.ShowPingIndicator(___m_pingTarget != null);
                        CrosshairGuiLayer_ShowPingIndicatorPatch.ShouldRun = false;
                    }
                    CrosshairGuiLayer_ShowPingIndicatorPatch.WouldHaveRun = false;
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
        }

    }
}
