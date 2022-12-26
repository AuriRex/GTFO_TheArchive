using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Models;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.QoL
{
    [RundownConstraint(Utils.RundownFlags.RundownFive, Utils.RundownFlags.Latest)]
    internal class FlashlightIconColors : Feature
    {
        public override string Name => "Flashlight Icon Colors";

        public override string Group => FeatureGroups.QualityOfLife;

        public override string Description => "Customize the flashlight on/off indicator colors.";

        public override bool SkipInitialOnEnable => true;

#if IL2CPP
        public override void OnEnable()
        {
            SetCustomColors();
        }

        public override void OnDisable()
        {
            if(_defaultsSet)
            {
                SetFlashlightIconColors(_default_Enabled, _default_Disabled);
            }
        }

        [FeatureConfig]
        public static FlashlightIconColorsSettings Settings { get; set; }

        public class FlashlightIconColorsSettings
        {
            public SColor ColorEnabled { get; set; } = new SColor(1f, 0.9206f, 0.7206f, 0.6137f);
            public SColor ColorDisabled { get; set; } = new SColor(0.7216f, 0.7216f, 0.7216f, 0.3137f);
        }

        private static bool _defaultsSet = false;
        private static Color _default_Enabled;
        private static Color _default_Disabled;

        public static void SetCustomColors()
        {
            SetFlashlightIconColors(Settings.ColorEnabled.ToUnityColor(), Settings.ColorDisabled.ToUnityColor());
        }

        public static void SetFlashlightIconColors(Color colorEnabled, Color colorDisabled)
        {
            var icons = GuiManager.PlayerLayer?.Inventory?.m_iconDisplay;

            if (icons == null)
                return;

            var flashlightIcon = icons?.FlashLightIcon;

            if (flashlightIcon == null)
                return;

            if(!_defaultsSet)
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
            public static void Postfix()
            {
                SetCustomColors();
            }
        }
#endif
    }
}
