using CellMenu;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Models;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Hud;

[RundownConstraint(Utils.RundownFlags.RundownFive, Utils.RundownFlags.Latest)]
internal class FlashlightIconColors : Feature
{
    public override string Name => "Flashlight Icon Colors";

    public override FeatureGroup Group => FeatureGroups.Hud;

    public override string Description => "Customize the flashlight on/off indicator colors.";

    public override bool SkipInitialOnEnable => true;

    [FeatureConfig]
    public static FlashlightIconColorsSettings Settings { get; set; }

    public class FlashlightIconColorsSettings
    {
        [FSDisplayName("Color Flashlight On")]
        public SColor ColorEnabled { get; set; } = new SColor(1f, 0.9206f, 0.7206f, 0.6137f);

        [FSDisplayName("Color Flashlight Off")]
        public SColor ColorDisabled { get; set; } = new SColor(0.7216f, 0.7216f, 0.7216f, 0.3137f);
    }

#if IL2CPP
    public override void OnEnable()
    {
        SetCustomColors(GuiManager.PlayerLayer?.Inventory);

        SetMapFlashlightIcons(defaultColors: false);
    }

    public override void OnFeatureSettingChanged(FeatureSetting setting)
    {
        OnEnable();
    }

    public override void OnDisable()
    {
        if (_defaultsSet)
        {
            SetFlashlightIconColors(GuiManager.PlayerLayer?.Inventory, _default_Enabled, _default_Disabled);

            SetMapFlashlightIcons(defaultColors: true);
        }
    }

    private void SetMapFlashlightIcons(bool defaultColors)
    {
        if (CM_PageMap.Current == null)
            return;

        foreach (var inv in CM_PageMap.Current.m_inventory)
        {
            if (defaultColors)
                SetFlashlightIconColors(inv, _default_Enabled, _default_Disabled);
            else
                SetCustomColors(inv);
        }
    }


    private static bool _defaultsSet = false;
    private static Color _default_Enabled;
    private static Color _default_Disabled;

    public static void SetCustomColors(PUI_Inventory inventory)
    {
        SetFlashlightIconColors(inventory, Settings.ColorEnabled.ToUnityColor(), Settings.ColorDisabled.ToUnityColor());
    }

    public static void SetFlashlightIconColors(PUI_Inventory inventory, Color colorEnabled, Color colorDisabled)
    {
        var flashlightIcon = inventory?.m_iconDisplay?.FlashLightIcon;

        if (flashlightIcon == null)
            return;

        if (!_defaultsSet)
        {
            _default_Enabled = flashlightIcon.Enabled.color;
            _default_Disabled = flashlightIcon.Disabled.color;
            _defaultsSet = true;
        }

        flashlightIcon.Enabled.color = colorEnabled;
        flashlightIcon.Disabled.color = colorDisabled;
    }

    [ArchivePatch(typeof(PUI_Inventory), nameof(PUI_Inventory.Setup), new Type[] { typeof(GuiLayer) })]
    internal static class PUI_Inventory_Setup_Patch
    {
        public static void Postfix(PUI_Inventory __instance)
        {
            SetCustomColors(__instance);
        }
    }
#endif
}