using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.LocalProgression
{
    [EnableFeatureByDefault]
    [DisallowInGameToggle]
    public class LocalProgressionController : Feature
    {
        public override string Name => "Local Progression";

        public override string Group => FeatureGroups.LocalProgression;

        public override bool RequiresRestart => true;

        public new static IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static LocalProgressionSettings Settings { get; set; }

        public override void OnGameDataInitialized()
        {
            bool rundownFiveOrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownFive.ToLatest());
            bool rundownSixOrLater = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownSix.ToLatest());
            bool shouldEnableVanity = Settings.LocalVanity && rundownSixOrLater;
            bool shouldEnableBoosters = (Settings.LocalBoosters && rundownFiveOrLater) | (rundownSixOrLater && shouldEnableVanity);

            if (Settings.LocalProgression)
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
        }
    }
}
