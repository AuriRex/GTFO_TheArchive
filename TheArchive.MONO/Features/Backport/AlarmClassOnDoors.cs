using ChainedPuzzles;
using LevelGeneration;
using System;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Backport
{
    [EnableFeatureByDefault]
    [RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownThree)]
    public class AlarmClassOnDoors : Feature
    {
        public override string Name => "Alarm Class on Security Doors";

        public override string Description => "Add alarm classes to security door interaction texts";

        public override string Group => FeatureGroups.Backport;

#if MONO
        [ArchivePatch(typeof(LG_SecurityDoor_Locks), "OnDoorState")]
        internal static class LG_SecurityDoor_Locks_OnDoorStatePatch
        {
            static FieldAccessor<ChainedPuzzleInstance, iChainedPuzzleCore[]> A_ChainedPuzzleInstance_m_chainedPuzzleCores = FieldAccessor<ChainedPuzzleInstance, iChainedPuzzleCore[]>.GetAccessor("m_chainedPuzzleCores");

            public static void Postfix(LG_SecurityDoor_Locks __instance, pDoorState state, ref Interact_Timed ___m_intOpenDoor)
            {
                try
                {
                    switch (state.status)
                    {
                        case eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm:
                            if (__instance.ChainedPuzzleToSolve == null) break;
                            var puzzles = A_ChainedPuzzleInstance_m_chainedPuzzleCores.Get(__instance.ChainedPuzzleToSolve);

                            if (puzzles == null) break;
                            var value = Utils.ToRoman(puzzles.Length);
                            ___m_intOpenDoor.SetInteractionMessage($"Start Security Scan Sequence <color=red>[WARNING:CLASS {value} ALARM DETECTED]</color>");
                            break;
                    }
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
