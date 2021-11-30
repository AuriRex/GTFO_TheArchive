using AK;
using ChainedPuzzles;
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
    public class QualityOfLifePatches
    {
        // Fix ladder movement so that W is always upwards and S always downwards no matter where you're looking
        [ArchivePatch(typeof(LG_Ladder), "GetMoveVec", RundownFlags.RundownOne, nameof(ArchiveSettings.EnableQualityOfLifeImprovements))]
        internal static class LG_Ladder_GetMoveVecPatch
        {
            public static bool Prefix(ref Vector3 __result, Vector3 camDir, float axisVertical)
            {
                __result = Vector3.ClampMagnitude(Vector3.up * axisVertical, 1f);

                return false;
            }
        }

        // Use the player ping (Middle Mouse Ping) for terminal ping command pings
        [ArchivePatch(typeof(LG_GenericTerminalItem), "PlayPing", RundownFlags.RundownOne, nameof(ArchiveSettings.EnableQualityOfLifeImprovements))]
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
                    MelonLoader.MelonLogger.Msg(ConsoleColor.Red, $"An error occured: {ex.Message}");
                    MelonLoader.MelonLogger.Msg(ConsoleColor.Red, $"{ex.StackTrace}");
                }
            }
        }

        // Change the "WARDEN OBJECTIVE" text in the top left of the screen to the current selected mission, ex: "R1A1:The Admin"
        [ArchivePatch(typeof(PlayerGuiLayer), "UpdateObjectives", RundownFlags.RundownOne | RundownFlags.RundownThree, nameof(ArchiveSettings.EnableQualityOfLifeImprovements))] 
        internal static class PlayerGuiLayer_UpdateObjectivesPatch
        {
            public static void Postfix(ref PlayerGuiLayer __instance, ref PUI_GameObjectives ___m_wardenObjective)
            {
                try
                {
                    if (RundownManager.ActiveExpedition == null) return;
                    //MelonLoader.MelonLogger.Msg(ConsoleColor.DarkMagenta, "Replacing WARDEN OBJECTIVE text ...");

                    pActiveExpedition activeExpeditionData = RundownManager.GetActiveExpeditionData();

                    var rundownNumber = ArchiveMod.CurrentRundown.GetIntValue();

                    ___m_wardenObjective.m_header.text = $"R{rundownNumber}{RundownManager.ActiveExpedition.Descriptive.Prefix}{activeExpeditionData.expeditionIndex + 1}:{RundownManager.ActiveExpedition.Descriptive.PublicName}";
                }
                catch (Exception ex)
                {
                    MelonLoader.MelonLogger.Msg(ConsoleColor.Red, $"An error occured: {ex.Message}");
                    MelonLoader.MelonLogger.Msg(ConsoleColor.Red, $"{ex.StackTrace}");
                }
            }
        }

        // Change hacking minigame to be more in line with newest version of the game -> minigame finishes and hack disappears instantly
        [ArchivePatch(typeof(HackingTool), "UpdateHackSequence", RundownFlags.RundownOne, RundownFlags.RundownThree, nameof(ArchiveSettings.EnableQualityOfLifeImprovements))]
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
                            ___m_state = HackSequenceState.Done;

                            if (___m_currentHackable != null)
                            {
                                LG_LevelInteractionManager.WantToSetHackableStatus(___m_currentHackable, eHackableStatus.Success, __instance.Owner);
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
                    MelonLoader.MelonLogger.Msg(ConsoleColor.Red, $"An error occured: {ex.Message}");
                    MelonLoader.MelonLogger.Msg(ConsoleColor.Red, $"{ex.StackTrace}");
                }
                return true;
            }
        }

        // Add alarm classes to security door interaction text
        [ArchivePatch(typeof(LG_SecurityDoor_Locks), "OnDoorState", RundownFlags.RundownTwo, nameof(ArchiveSettings.EnableQualityOfLifeImprovements))]
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
                    MelonLoader.MelonLogger.Msg(ConsoleColor.Red, $"An error occured: {ex.Message}");
                    MelonLoader.MelonLogger.Msg(ConsoleColor.Red, $"{ex.StackTrace}");
                }
            }
        }

        // update the mods hud state whenever the hud gets toggled by the game
        [ArchivePatch(typeof(PlayerGuiLayer), "SetVisible", RundownFlags.All, nameof(ArchiveSettings.EnableQualityOfLifeImprovements))]
        internal static class PlayerGuiLayer_SetVisiblePatch
        {
            public static void Postfix(bool visible)
            {
                ArchiveModule.instance.HudIsVisible = visible;
            }
        }


        // R2 and up like hack fail effect (might be a bit stricter than the officially implemented one because it's essentially the same as hitting your hammer on the box)
        [ArchivePatch(typeof(HackingMinigame_TimingGrid), "OnMiss", RundownFlags.RundownOne, nameof(ArchiveSettings.EnableQualityOfLifeImprovements))]
        internal static class HackingMinigame_TimingGrid_OnMissPatch
        {
            public static void Postfix(ref HackingTool ___m_tool)
            {
                ___m_tool.Owner.Noise = Agents.Agent.NoiseType.MeleeHit;
                ___m_tool.Sound.Post(EVENTS.SENTRYGUN_ACTIVATED, ___m_tool.transform.position);
            }
        }

        // prioritize resources in ping raycasts
        [ArchivePatch(typeof(CrosshairGuiLayer), "ShowPingIndicator", nameof(ArchiveSettings.EnableQualityOfLifeImprovements))]
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

        [ArchivePatch(typeof(PlayerAgent), "LateUpdate", nameof(ArchiveSettings.EnableQualityOfLifeImprovements))]
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
                    ArchiveLogger.Error(ex.Message);
                    ArchiveLogger.Error(ex.StackTrace);
                }
            }
        }

    }
}
