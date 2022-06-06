using AK;
using ChainedPuzzles;
using LevelGeneration;
using Player;
using System;
using System.Collections.Generic;
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
        [ArchivePatch(typeof(LG_GenericTerminalItem), "PlayPing", RundownFlags.RundownOne)]
        internal static class LG_GenericTerminalItem_PlayPingPatch
        {
            private static MethodInfo PlayerAgent_TriggerMarkerPing = typeof(PlayerAgent).GetMethod("TriggerMarkerPing", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            private static ManualPingTarget CustomTerminalPingQoLInstance = null;

            public static void Prefix(ref LG_GenericTerminalItem __instance)
            {
                if(!SNetwork.SNet.IsMaster)
                {
                    return;
                }

                var localPlayerAgent = PlayerManager.GetLocalPlayerAgent();

                if (CustomTerminalPingQoLInstance == null)
                {
                    CustomTerminalPingQoLInstance = new ManualPingTarget();
                    CustomTerminalPingQoLInstance.PingTargetStyle = eNavMarkerStyle.PlayerPingTerminal;
                }

                try
                {
                    var bounds = MonoUtils.GetMaxBounds(__instance.gameObject);

                    PlayerAgent_TriggerMarkerPing.Invoke(localPlayerAgent, new object[] { (iPlayerPingTarget) CustomTerminalPingQoLInstance, bounds.center });
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
        }

        // Change the "WARDEN OBJECTIVE" text in the top left of the screen to the current selected mission, ex: "R1A1:The Admin"
        [ArchivePatch(typeof(PlayerGuiLayer), "UpdateObjectives")] 
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
                            rundownPrefix = $"R{(int)ArchiveMod.CurrentRundown}";
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
        [ArchivePatch(typeof(HackingTool), "UpdateHackSequence")]
        internal static class HackingTool_UpdateHackSequencePatch
        {
            private static MethodInfo HackingTool_ClearScreen = typeof(HackingTool).GetMethod("ClearScreen", AnyBindingFlags);
            private static MethodInfo HackingTool_OnStopHacking = typeof(HackingTool).GetMethod("OnStopHacking", AnyBindingFlags);

            private static eHackableStatus _hStatus_Success = GetEnumFromName<eHackableStatus>(nameof(eHackableStatus.Success));

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
                                LG_LevelInteractionManager.WantToSetHackableStatus(___m_currentHackable, _hStatus_Success, __instance.Owner);
                            }
                            ___m_state = HackSequenceState.Done;
                            return false;
                        case HackSequenceState.Done:
                            if (___m_stateTimer < 0f)
                            {
                                HackingTool_ClearScreen.Invoke(__instance, null);
                                HackingTool_OnStopHacking.Invoke(__instance, null);
                                __instance.Sound.Post(EVENTS.BUTTONGENERICSEQUENCEFINISHED);
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
        [ArchivePatch(typeof(LG_SecurityDoor_Locks), "OnDoorState", RundownFlags.RundownOne, RundownFlags.RundownThree)]
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
        /*[ArchivePatch(typeof(HackingMinigame_TimingGrid), "OnMiss", RundownFlags.RundownOne)]
        internal static class HackingMinigame_TimingGrid_OnMissPatch
        {
            public static void Postfix(ref HackingTool ___m_tool)
            {
                ___m_tool.Owner.Noise = Agents.Agent.NoiseType.MeleeHit;
                ___m_tool.Sound.Post(EVENTS.SENTRYGUN_ACTIVATED, ___m_tool.transform.position);
            }
        }*/

        [ArchivePatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.AddOutput), RundownFlags.RundownOne, new Type[] { typeof(string), typeof(bool) })]
        internal static class LG_ComputerTerminalCommandInterpreter_AddOutputPatch_R1
        {
            private static FieldInfo LG_ComputerTerminalCommandInterpreter_m_terminal = typeof(LG_ComputerTerminalCommandInterpreter).GetField("m_terminal", AnyBindingFlags);

            public static void Postfix(LG_ComputerTerminalCommandInterpreter __instance, string line)
            {
                try
                {
                    var m_terminal = (LG_ComputerTerminal) LG_ComputerTerminalCommandInterpreter_m_terminal.GetValue(__instance);
                    LG_ComputerTerminalCommandInterpreter_AddOutputPatch.Postfix(__instance, ref m_terminal, line);
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
        }

        // Add the current Terminal Key as well as the Zone you're in to the terminal text
        [ArchivePatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.AddOutput), RundownFlags.RundownTwo, RundownFlags.RundownThree, new Type[] { typeof(string), typeof(bool) })]
        internal static class LG_ComputerTerminalCommandInterpreter_AddOutputPatch
        {
            private static HashSet<LG_ComputerTerminalCommandInterpreter> _interpreterSet = new HashSet<LG_ComputerTerminalCommandInterpreter>();

            public static string GetKey(LG_ComputerTerminal terminal) => "TERMINAL_" + terminal.m_serialNumber;

            public static void Postfix(LG_ComputerTerminalCommandInterpreter __instance, ref LG_ComputerTerminal ___m_terminal, string line)
            {
                try
                {
                    if (line.Equals("---------------------------------------------------------------"))
                    {
                        
                        if (_interpreterSet.Contains(__instance))
                        {
                            ArchiveLogger.Debug($"Key & Zone in Terminal: Step 2/2 [{GetKey(___m_terminal)}]");
                            _interpreterSet.Remove(__instance);
                            __instance.AddOutput($"Welcome to {GetKey(___m_terminal)}, located in {___m_terminal.SpawnNode.m_zone.NavInfo.PrefixLong}_{___m_terminal.SpawnNode.m_zone.NavInfo.Number}", true);
                        }
                        else
                        {
                            ArchiveLogger.Debug($"Key & Zone in Terminal: Step 1/2 [{GetKey(___m_terminal)}]");
                            _interpreterSet.Add(__instance);
                        }

                    }
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
        }


        // prioritize resources in ping raycasts
        [ArchivePatch(typeof(CrosshairGuiLayer), "ShowPingIndicator", RundownFlags.RundownOne, RundownFlags.RundownThree)]
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
        [ArchivePatch(typeof(PlayerAgent), "LateUpdate", RundownFlags.RundownOne, RundownFlags.RundownThree)]
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
