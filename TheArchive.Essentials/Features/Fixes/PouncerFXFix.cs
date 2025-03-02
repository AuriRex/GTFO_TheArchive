using Enemies;
using Player;
using System.Collections;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Fixes;

[EnableFeatureByDefault]
[RundownConstraint(RundownFlags.RundownSeven, RundownFlags.Latest)]
public class PouncerFXFix : Feature
{
    public override string Name => "Pouncer ScreenFX Stuck Fix";

    public override FeatureGroup Group => FeatureGroups.Fixes;

    public override string Description => "(WIP) Prevents the pouncer tentacles from getting stuck on screen.";

#if IL2CPP
    public new static IArchiveLogger FeatureLogger { get; set; }


    /*[ArchivePatch(typeof(PouncerBehaviour), nameof(PouncerBehaviour.RequestConsume))]
    internal static class PouncerBehaviour_RequestConsume_Patch
    {
        public static void Postfix(PouncerBehaviour __instance, int playerSlotIndex)
        {
            var slot = playerSlotIndex;
            PlayerManager.TryGetPlayerAgent(ref slot, out var agent);

            FeatureLogger.Notice($"{nameof(PouncerBehaviour_RequestConsume_Patch)} called! Target: {agent?.Owner?.NickName}");
        }
    }*/

    //OnConsumeRequestReceived
    [ArchivePatch(typeof(PouncerBehaviour), nameof(PouncerBehaviour.OnConsumeRequestReceived))]
    internal static class PouncerBehaviour_OnConsumeRequestReceived_Patch
    {
        public static void Postfix(PouncerBehaviour __instance, pEB_PouncerTargetInfoPacket data)
        {
            var slot = data.PlayerSlot;
            PlayerManager.TryGetPlayerAgent(ref slot, out var agent);

            if (!agent.IsLocallyOwned || agent.Owner.SafeIsBot())
                return;

            if (__instance.m_ai.m_enemyAgent.GetArenaDimension((uint)slot, out var dimension))
            {
                LoaderWrapper.StartCoroutine(CheckForPouncerFXGlitch(dimension.DimensionIndex));

                FeatureLogger.Notice($"{nameof(PouncerBehaviour_OnConsumeRequestReceived_Patch)} called! Target: {agent?.Owner?.NickName}, Dim: {dimension.DimensionIndex}");
            }
        }

        public static IEnumerator CheckForPouncerFXGlitch(eDimensionIndex pouncerDimension)
        {
            yield return new WaitForSeconds(1.2f);
            var localPlayer = PlayerManager.GetLocalPlayerAgent();

            if (localPlayer.DimensionIndex == pouncerDimension)
                yield break;

            FeatureLogger.Notice($"Player Dimension: {localPlayer.DimensionIndex} - Pouncer Dimension: {pouncerDimension}");

            localPlayer.FPSCamera.PouncerScreenFX.SetCovered(false);
            FeatureLogger.Success("Removing PouncerFX from screen in an attempt to prevent PouncerFX glitch.");
        }
    }
#endif
}