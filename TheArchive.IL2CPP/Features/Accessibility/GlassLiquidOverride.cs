using Player;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Components;
using TheArchive.Core.FeaturesAPI.Settings;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Accessibility
{
    [RundownConstraint(RundownFlags.RundownTwo, RundownFlags.Latest)]
    internal class GlassLiquidOverride : Feature
    {
        public override string Name => "Glass Liquid System Override";

        public override string Group => FeatureGroups.Accessibility;

        public override string Description => "Adjust the games \"Glass Liquid System\"\nThe thing that renders the blood splatters etc on your visor.";

        public override bool PlaceSettingsInSubMenu => true;

        [FeatureConfig]
        public static GlassLiquidOverrideSettings Settings { get; set; }

        public class GlassLiquidOverrideSettings
        {
            [FSDisplayName("Enable Liquid System")]
            [FSDescription("Turning this off stops the Glass Liquid System from updating.\n\nMake sure that nothing's left on your visor or else it'll be stuck there until you re-enable this setting.")]
            public bool EnableGlassLiquidSystem { get; set; } = true;

            [FSDescription("The Systems resolution")]
            public GlassLiquidSystemQuality Quality { get; set; } = GlassLiquidSystemQuality.Default;

            [FSDisplayName("Clean Screen")]
            [FSDescription("Applies the Disinfect Station visuals to clear any other liquids.\n\n(Might fix \"Void Bug\" once pressed)")]
            public FButton ApplyDisinfectButton => new FButton("Clean");

            [FSHeader("Warning")]
            [FSDisplayName("Completely Disable System")]
            [FSDescription("Toggling this option on <u>completely disables the Glass Liquid System</u> and makes you <u>unable to re-enable it mid game</u>.\nMight save some frames on lower end graphics hardware.\n\n<color=orange>A re-drop into the level is required after disabling this to re-enable the system!</color>")]
            public bool CompletelyDisable { get; set; } = false;
        }

        public enum GlassLiquidSystemQuality
        {
            VeryBad = 1,
            Bad,
            SlightlyWorse,
            Default,
            Good,
            Better,
            Amazing,
            Extraordinary
        }

        private static bool _isDisabling = false;

        public override void OnEnable()
        {
            _isDisabling = false;
            SetGlassLiquidSystemActive(Settings.EnableGlassLiquidSystem);
        }

        public override void OnDisable()
        {
            _isDisabling = true;
            SetGlassLiquidSystemActive(true);
        }

        public override void OnButtonPressed(ButtonSetting setting)
        {
            if (setting.ButtonID.Contains(nameof(GlassLiquidOverrideSettings.ApplyDisinfectButton)))
                ScreenLiquidManager.DirectApply(ScreenLiquidSettingName.disinfectionStation_Apply, new UnityEngine.Vector2(0.5f, 0.5f), UnityEngine.Vector2.zero);
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            SetGlassLiquidSystemActive(Settings.EnableGlassLiquidSystem);
        }

        public static void SetGlassLiquidSystemActive(bool active, PlayerAgent player = null)
        {
            player ??= PlayerManager.GetLocalPlayerAgent();

            if (player == null || !player.IsLocallyOwned)
                return;

            var fpsCamera = player.FPSCamera;

            if (fpsCamera == null)
                return;

            var gls = fpsCamera.GetComponent<GlassLiquidSystem>();

            if (Settings.CompletelyDisable && !_isDisabling)
            {
                if (gls != null)
                {
                    UnityEngine.Object.Destroy(gls);
                }
                ScreenLiquidManager.Clear();
                ScreenLiquidManager.LiquidSystem = null;
                SetCollectCommandsScreenLiquid(fpsCamera, false);
                return;
            }

            if (gls == null)
                return;

            if (active)
            {
                SetCollectCommandsScreenLiquid(fpsCamera, true);
                gls.OnResolutionChange(UnityEngine.Screen.currentResolution);
            }
            else
            {
                ScreenLiquidManager.Clear();
                ScreenLiquidManager.LiquidSystem = null;
                SetCollectCommandsScreenLiquid(fpsCamera, false);
            }
        }

        private static void SetCollectCommandsScreenLiquid(FPSCamera camera, bool value)
        {
            if (Is.R6OrLater)
                SetCollectCommandsScreenLiquidR6(camera, value);
        }

        private static void SetCollectCommandsScreenLiquidR6(FPSCamera camera, bool value)
        {
#if IL2CPP
            camera.CollectCommandsScreenLiquid = value;
#endif
        }

#if IL2CPP
        [RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
        [ArchivePatch(nameof(LocalPlayerAgent.Setup))]
        internal static class LocalPlayerAgent_Setup_Patch
        {
            public static Type Type() => typeof(LocalPlayerAgent);

            public static void Postfix(LocalPlayerAgent __instance) => PlayerAgent_Setup_Patch.Postfix(__instance);
        }
#endif

        [RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownFive)]
        [ArchivePatch(typeof(PlayerAgent), nameof(PlayerAgent.Setup))]
        internal static class PlayerAgent_Setup_Patch
        {
            public static void Postfix(PlayerAgent __instance)
            {
                if (!__instance.IsLocallyOwned)
                    return;

                SetGlassLiquidSystemActive(Settings.EnableGlassLiquidSystem, __instance);
            }
        }

        [ArchivePatch(nameof(GlassLiquidSystem.Setup))]
        internal static class GlassLiquidSystem_Setup_Patch
        {
            public static Type Type() => typeof(GlassLiquidSystem);
            public static void Prefix(ref int quality)
            {
                quality = (int)Settings.Quality;
            }
        }
    }
}
