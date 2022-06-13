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
    //[BindPatchToSetting(nameof(ArchiveSettings.EnableQualityOfLifeImprovements), "QOL")]
    public class QualityOfLifePatches
    {

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

    }
}
