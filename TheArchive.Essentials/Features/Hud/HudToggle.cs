using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using UnityEngine;

namespace TheArchive.Features.Hud;

[EnableFeatureByDefault]
public class HudToggle : Feature
{
    public override string Name => "Hud Toggle";

    public override GroupBase Group => GroupManager.Hud;

    public override string Description => "Keybind to toggle parts of the HUD";

    [FeatureConfig]
    public static HudToggleSettings Settings { get; set; }
    
    public class HudToggleSettings
    {
        [FSDisplayName("HUD Toggle Key")]
        [FSDescription("Key used to toggle the HUD.")]
        public KeyCode Key { get; set; } = KeyCode.F1;
    }
    
    private static bool _hudIsVisible = true;
    
    public override void Update()
    {
        if (FocusStateManager.CurrentState != eFocusState.FPS)
            return;
        
        if (!Input.GetKeyDown(Settings.Key))
            return;
        
        // Toggle hud
        _hudIsVisible = !_hudIsVisible;
        GuiManager.PlayerLayer.SetVisible(_hudIsVisible);
        GuiManager.WatermarkLayer.SetVisible(_hudIsVisible);
        GuiManager.CrosshairLayer.SetVisible(_hudIsVisible);
    }

    // update the mods hud state whenever the hud gets toggled by the game
    [ArchivePatch(typeof(PlayerGuiLayer), "SetVisible")]
    internal static class PlayerGuiLayer__SetVisible__Patch
    {
        public static void Postfix(bool visible)
        {
            _hudIsVisible = visible;
        }
    }
}