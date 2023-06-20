using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.LocalProgression
{
    [EnableFeatureByDefault]
    [DisallowInGameToggle]
    [DoNotSaveToConfig]
    public class LocalProgressionController : Feature
    {
        public override string Name => "Local Progression";

        public override string Group => FeatureGroups.LocalProgression;

        public override bool InlineSettingsIntoParentMenu => true;

        public override bool RequiresRestart => true;

        public new static IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static LocalProgressionSettings Settings { get; set; }

        public static bool ForceEnable { get; internal set; } = false;

        public override void OnGameDataInitialized()
        {
            if (ForceEnable)
            {
                FeatureLogger.Notice($"Local Progression is forced on!");
            }

            if (!ForceEnable && Settings.DisableLocalProgressionOnLatest && BuildInfo.Rundown.IsIncludedIn(RundownFlags.Latest))
            {
                FeatureLogger.Notice($"Detected build to be latest ({BuildInfo.Rundown}), disabling LocalProgression because {nameof(LocalProgressionSettings.DisableLocalProgressionOnLatest)} is set.");
                return;
            }

            bool rundownFiveOrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownFive.ToLatest());
            bool rundownSixOrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownSix.ToLatest());
            bool shouldEnableVanity = (ForceEnable || Settings.LocalVanity) && rundownSixOrLater;
            bool shouldEnableBoosters = ((ForceEnable || Settings.LocalBoosters) && rundownFiveOrLater) | (rundownSixOrLater && shouldEnableVanity);

            if (ForceEnable || Settings.LocalProgression)
            {
                FeatureManager.EnableAutomatedFeature(typeof(PlayFabManagerPatches));
                FeatureManager.EnableAutomatedFeature(typeof(LocalProgressionCore));
            }

            // Enable boosters either if the setting is set or LocalVanity is enabled in R6 and later
            // reason being the one GetInventory patch returning both Boosters and vanity at the same time

            if (shouldEnableBoosters)
                FeatureManager.EnableAutomatedFeature(typeof(LocalBoosters));
            if (shouldEnableVanity)
                FeatureManager.EnableAutomatedFeature(typeof(LocalVanity));

            if (Settings.SkipExpeditionUnlockRequirements)
                FeatureManager.EnableAutomatedFeature(typeof(ExpeditionUnlockSkip));
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            RequestRestart();
        }

        public class LocalProgressionSettings
        {
            [FSDisplayName("Enable Local Progression")]
            public bool LocalProgression { get; set; } = true;

            [FSDisplayName("Enable Local Boosters")]
            [FSRundownHint(RundownFlags.RundownFive, RundownFlags.Latest)]
            public bool LocalBoosters { get; set; } = true;

            [FSDisplayName("Enable Local Vanity")]
            [FSRundownHint(RundownFlags.RundownSix, RundownFlags.Latest)]
            public bool LocalVanity { get; set; } = true;

            [FSHeader("Progression :// Misc")]
            [FSRundownHint(RundownFlags.Latest)]
            [FSDisplayName("Use Servers on Latest")]
            [FSDescription($"Disables the options above automatically on the latest game version")]
            public bool DisableLocalProgressionOnLatest { get; set; } = true;

            [FSDisplayName("All Expeditions Unlocked")]
            [FSDescription("Only works with LocalProgression enabled!")]
            public bool SkipExpeditionUnlockRequirements { get; set; } = false;
        }
    }
}
