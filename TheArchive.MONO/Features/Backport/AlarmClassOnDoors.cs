﻿using ChainedPuzzles;
using LevelGeneration;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Backport
{
    [EnableFeatureByDefault]
    [RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownThree)]
    public class AlarmClassOnDoors : Feature
    {
        public override string Name => "Alarm Class on Security Doors";

        public override FeatureGroup Group => FeatureGroups.Backport;

        public override string Description => "Add alarm classes to security door interaction texts";

#if MONO
        [ArchivePatch(typeof(LG_SecurityDoor_Locks), "OnDoorState")]
        internal static class LG_SecurityDoor_Locks_OnDoorStatePatch
        {
            static readonly IValueAccessor<ChainedPuzzleInstance, iChainedPuzzleCore[]> A_ChainedPuzzleInstance_m_chainedPuzzleCores = AccessorBase.GetValueAccessor<ChainedPuzzleInstance, iChainedPuzzleCore[]>("m_chainedPuzzleCores");
            static readonly IValueAccessor<Interact_Timed, string> A_Interact_Timed_m_interactMessage = AccessorBase.GetValueAccessor<Interact_Timed, string>("m_interactMessage");

            public static void Postfix(LG_SecurityDoor_Locks __instance, pDoorState state, ref Interact_Timed ___m_intOpenDoor)
            {
                try
                {
                    switch (state.status)
                    {
                        case eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm:
                            if (A_Interact_Timed_m_interactMessage.Get(___m_intOpenDoor).ToLower().Contains("error")) break;
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
