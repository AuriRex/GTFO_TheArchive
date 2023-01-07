using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using UnityEngine;

namespace TheArchive.Features
{
    [EnableFeatureByDefault]
    public class HudToggle : Feature
    {
        public override string Name => "Hud Toggle (F1)";

        public override string Description => "Keybind to toggle parts of the HUD";

        private static bool _hudIsVisible = true;

        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                // Toggle hud
                _hudIsVisible = !_hudIsVisible;
                GuiManager.PlayerLayer.SetVisible(_hudIsVisible);
                GuiManager.WatermarkLayer.SetVisible(_hudIsVisible);
                GuiManager.CrosshairLayer.SetVisible(_hudIsVisible);
            }
        }

        // update the mods hud state whenever the hud gets toggled by the game
        [ArchivePatch(typeof(PlayerGuiLayer), "SetVisible")]
        internal static class PlayerGuiLayer_SetVisiblePatch
        {
            public static void Postfix(bool visible)
            {
                _hudIsVisible = visible;
            }
        }
    }
}
