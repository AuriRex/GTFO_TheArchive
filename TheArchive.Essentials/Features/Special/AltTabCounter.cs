using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;

namespace TheArchive.Features.Special;

[EnableFeatureByDefault]
internal class AltTabCounter : Feature
{
    public override string Name => "Alt Tab Counter";

    public override FeatureGroup Group => FeatureGroups.Special;

    public override string Description => "Counts the amount of times that the game went out of focus. (ALT + TAB)";

    [FeatureConfig]
    public static AltTabCounterSettings Settings { get; set; }

    public class AltTabCounterSettings
    {
        [FSUseDynamicSubmenu]
        [FSDisplayName("ALT + TAB Counts")]
        public AltTabCounterSettingsForReal AltTabCounts { get; set; } = new AltTabCounterSettingsForReal();

        public class AltTabCounterSettingsForReal
        {
            [FSHeader("ALT + TABs <3")]
            [FSReadOnly]
            [FSDisplayName("Total Count")]
            [FSDescription("All time total of ALT + TABs")]
            public int TotalCount { get; set; }

            [FSReadOnly]
            [FSDisplayName("This Session Count")]
            [FSDescription("ALT + TABs accumulated this session")]
            public int CurrentSessionCount { get; set; }

            [FSReadOnly]
            [FSDisplayName("This Level Count")]
            [FSDescription("ALT + TABs accumulated this level")]
            public int CurrentWhileInLevelCount { get; set; }
        }
    }

    public static bool InLevel { get; private set; } = false;

    public override void Init()
    {
        Settings.AltTabCounts.CurrentSessionCount = 0;
    }

    public override void OnApplicationFocusChanged(bool focus)
    {
        if (!focus)
        {
            Settings.AltTabCounts.TotalCount++;
            Settings.AltTabCounts.CurrentSessionCount++;

            if (InLevel)
            {
                Settings.AltTabCounts.CurrentWhileInLevelCount++;
            }
                
            MarkSettingsAsDirty(Settings);
        }
    }

    public void OnGameStateChanged(eGameStateName state)
    {
        InLevel = state == eGameStateName.InLevel;

        if (InLevel)
        {
            Settings.AltTabCounts.CurrentWhileInLevelCount = 0;

            MarkSettingsAsDirty(Settings);
        }
    }
}