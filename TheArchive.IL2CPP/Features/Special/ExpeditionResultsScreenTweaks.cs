using CellMenu;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Special
{
    // TODO: Custom Images / Sounds?
    public class ExpeditionResultsScreenTweaks : Feature
    {
        public override string Name => "Results Screen Tweaks";

        public override string Group => FeatureGroups.Special;

        public override string Description => "Tweak the Expedition Fail/Success screens!";

        public override bool PlaceSettingsInSubMenu => true;

        public override bool SkipInitialOnEnable => true;

        [FeatureConfig]
        public static ExpeditionResultsScreenTweaksSettings Settings { get; set; }

        public class ExpeditionResultsScreenTweaksSettings
        {
            [FSDisplayName("Disable Success Background")]
            [FSDescription("Removes the background image on the <b>Expedition Success Screen</b>.\n(background turns black instead)")]
            [FSRundownHint(RundownFlags.RundownSix, RundownFlags.Latest)]
            [FSIdentifier(nameof(DisableSuccessMask))]
            public bool DisableSuccessMask { get; set; } = false;

            [FSDisplayName("Disable Fail Background")]
            [FSDescription("Removes the background image on the <b>Expedition Fail Screen</b>.\n(background turns black instead)")]
            [FSRundownHint(RundownFlags.RundownSix, RundownFlags.Latest)]
            [FSIdentifier(nameof(DisableFailSkull))]
            public bool DisableFailSkull { get; set; } = false;
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            if(setting.Identifier == nameof(ExpeditionResultsScreenTweaksSettings.DisableSuccessMask))
            {
                SetSuccessMaskActive(!Settings.DisableSuccessMask);
            }
            if (setting.Identifier == nameof(ExpeditionResultsScreenTweaksSettings.DisableFailSkull))
            {
                SetFailMaskActive(!Settings.DisableFailSkull);
            }
        }

        public override void OnEnable()
        {
            if(Settings.DisableSuccessMask)
                SetSuccessMaskActive(false);
            if (Settings.DisableFailSkull)
                SetFailMaskActive(false);
        }

        public override void OnDisable()
        {
            SetSuccessMaskActive(true);
            SetFailMaskActive(true);
        }

        private static IValueAccessor<MainMenuGuiLayer, MainMenuGuiLayer> A_MainMenuGuiLayer_Current;

        public override void Init()
        {
            A_MainMenuGuiLayer_Current = AccessorBase.GetValueAccessor<MainMenuGuiLayer, MainMenuGuiLayer>("Current");
        }

        public static void SetSuccessMaskActive(bool active) => SetMaskBackgroundActive(A_MainMenuGuiLayer_Current.Get(null)?.PageExpeditionSuccess?.transform, active);

        public static void SetFailMaskActive(bool active) => SetMaskBackgroundActive(A_MainMenuGuiLayer_Current.Get(null)?.PageExpeditionFail?.transform, active);

        public static void SetMaskBackgroundActive(Transform trans, bool active)
        {
            trans?.GetChildWithExactName("Backgrounds")?.GetChildWithExactName("Middle")?.gameObject?.SetActive(active);
        }

        [ArchivePatch(typeof(CM_PageExpeditionSuccess), nameof(CM_PageExpeditionSuccess.Setup))]
        internal static class CM_PageExpeditionSuccess_Setup_Patch
        {
            public static void Postfix(CM_PageExpeditionSuccess __instance)
            {
                if (Settings.DisableSuccessMask)
                    SetMaskBackgroundActive(__instance.transform, false);
            }
        }

        [ArchivePatch(typeof(CM_PageExpeditionFail), nameof(CM_PageExpeditionFail.Setup))]
        internal static class CM_PageExpeditionFail_Setup_Patch
        {
            public static void Postfix(CM_PageExpeditionFail __instance)
            {
                if (Settings.DisableFailSkull)
                    SetMaskBackgroundActive(__instance.transform, false);
            }
        }
    }
}
