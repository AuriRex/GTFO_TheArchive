using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using UnityEngine;

namespace TheArchive.Features.Accessibility;

internal class DisableHudSway : Feature
{
    public override string Name => "Disable HUD Sway";

    public override GroupBase Group => GroupManager.Accessibility;

    public override string Description => "Disables the in-game HUD sway while walking / jumping around.";

    [ArchivePatch(typeof(PlayerGuiLayer), nameof(PlayerGuiLayer.ApplyMovementSway))]
    internal static class PlayerGuiLayer_ApplyMovementSway_Patch
    {
        public static void Prefix(ref Vector3 sway)
        {
            sway = Vector3.zero;
        }
    }

    [ArchivePatch(typeof(PlayerGuiLayer), nameof(PlayerGuiLayer.ApplyMovementScale))]
    internal static class PlayerGuiLayer_ApplyMovementScale_Patch
    {
        public static void Prefix(ref float scale)
        {
            scale = 1f;
        }
    }

    [ArchivePatch(typeof(WatermarkGuiLayer), nameof(WatermarkGuiLayer.ApplyMovementSway))]
    internal static class WatermarkGuiLayer_ApplyMovementSway_Patch
    {
        public static void Prefix(ref Vector3 sway)
        {
            sway = Vector3.zero;
        }
    }

    [ArchivePatch(typeof(WatermarkGuiLayer), nameof(WatermarkGuiLayer.ApplyMovementScale))]
    internal static class WatermarkGuiLayer_ApplyMovementScale_Patch
    {
        public static void Prefix(ref float scale)
        {
            scale = 1f;
        }
    }
}