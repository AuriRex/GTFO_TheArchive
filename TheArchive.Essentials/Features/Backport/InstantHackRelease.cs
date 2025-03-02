using AK;
using LevelGeneration;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;
using static HackingTool;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Backport;

[EnableFeatureByDefault]
[RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownFour)]
public class InstantHackRelease : Feature
{
    public override string Name => "R5+ Like Instant Hack Release";

    public override FeatureGroup Group => FeatureGroups.Backport;

    public override string Description => "Change hacking minigame to be more in line with newest version of the game\nMinigame finishes and hack disappears instantly";

    public new static IArchiveLogger FeatureLogger { get; set; }

    [ArchivePatch(typeof(HackingTool), "UpdateHackSequence")]
    internal static class HackingTool_UpdateHackSequencePatch
    {
        private static MethodAccessor<HackingTool> A_HackingTool_ClearScreen = MethodAccessor<HackingTool>.GetAccessor("ClearScreen");
        private static MethodAccessor<HackingTool> A_HackingTool_OnStopHacking = MethodAccessor<HackingTool>.GetAccessor("OnStopHacking");

        private static eHackableStatus _hStatus_Success = GetEnumFromName<eHackableStatus>(nameof(eHackableStatus.Success));

        private static HackSequenceState _HackSequenceState_DoneWait = Utils.GetEnumFromName<HackSequenceState>(nameof(HackSequenceState.DoneWait));
        private static HackSequenceState _HackSequenceState_Done = Utils.GetEnumFromName<HackSequenceState>(nameof(HackSequenceState.Done));
#if IL2CPP
        public static bool Prefix(HackingTool __instance)
        {
            try
            {
                if(__instance.m_state == _HackSequenceState_DoneWait)
                {
                    __instance.m_activeMinigame.EndGame();
                    __instance.m_holoSourceGFX.SetActive(value: false);
                    __instance.Sound.SafePost(EVENTS.BUTTONGENERICBLIPTHREE);
                    __instance.m_stateTimer = 1f;
                    if (__instance.m_currentHackable != null)
                    {
                        LG_LevelInteractionManager.WantToSetHackableStatus(__instance.m_currentHackable, _hStatus_Success, __instance.Owner);
                    }
                    __instance.m_state = _HackSequenceState_Done;
                    return ArchivePatch.SKIP_OG;
                }
                if (__instance.m_state == _HackSequenceState_Done)
                {
                    if (__instance.m_stateTimer < 0f)
                    {
                        A_HackingTool_ClearScreen.Invoke(__instance);
                        A_HackingTool_OnStopHacking.Invoke(__instance);
                        __instance.Sound.SafePost(EVENTS.BUTTONGENERICSEQUENCEFINISHED);
                        __instance.m_state = HackSequenceState.Idle;
                    }
                    else
                    {
                        __instance.m_stateTimer -= Clock.Delta;
                    }
                    return ArchivePatch.SKIP_OG;
                }
            }
            catch (Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }
            return ArchivePatch.RUN_OG;
        }
#else
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
                            return ArchivePatch.SKIP_OG;
                        case HackSequenceState.Done:
                            if (___m_stateTimer < 0f)
                            {
                                A_HackingTool_ClearScreen.Invoke(__instance);
                                A_HackingTool_OnStopHacking.Invoke(__instance);
                                __instance.Sound.Post(EVENTS.BUTTONGENERICSEQUENCEFINISHED);
                                ___m_state = HackSequenceState.Idle;
                            }
                            else
                            {
                                ___m_stateTimer -= Clock.Delta;
                            }
                            return ArchivePatch.SKIP_OG;
                    }
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
                return ArchivePatch.RUN_OG;
            }
#endif
    }
}