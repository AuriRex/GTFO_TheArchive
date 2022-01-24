using TheArchive.Core;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    public class HudTogglePatches
    {
        // update the mods hud state whenever the hud gets toggled by the game
        [ArchivePatch(typeof(PlayerGuiLayer), "SetVisible")]
        [BindPatchToSetting(nameof(ArchiveSettings.EnableHudToggle))]
        internal static class PlayerGuiLayer_SetVisiblePatch
        {
            public static void Postfix(bool visible)
            {
                ArchiveMod.HudIsVisible = visible;
            }
        }
    }
}
