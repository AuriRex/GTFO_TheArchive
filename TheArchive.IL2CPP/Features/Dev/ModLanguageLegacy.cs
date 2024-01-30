using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Localization;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Dev;

[HideInModSettings]
[EnableFeatureByDefault]
[DisallowInGameToggle]
internal class ModLanguageLegacy : Feature
{
    public override string Name => "Mod Language Legacy";

    public override string Description => "Change Language of ModSettings for OG Rundowns 1 to 5";

    public override FeatureGroup Group => FeatureGroups.ArchiveCore;

    public override bool InlineSettingsIntoParentMenu => true;

    [FeatureConfig]
    public static ModLanguageLegacySettings Settings { get; set; }

    public class ModLanguageLegacySettings
    {
        [FSRundownHint(RundownFlags.RundownOne, RundownFlags.RundownFive)]
        [FSDisplayName("Legacy Version Language")]
        [FSDescription("The Language used for ModSettings when playing Original Rundowns 1 to 5.\n\nUpdates automatically whenever you choose your Language on the latest game version!")]
        public Language Language { get; set; } = Language.English;
    }

    public override void Init()
    {
        Localization.RegisterExternType<Language>();
    }

    public static void StoreLanguage(Language language)
    {
        if(Settings.Language != language)
            Settings.Language = language;
    }

    public override void OnDatablocksReady()
    {
        TrySetLanguageLegacy();
    }

    public override void OnFeatureSettingChanged(FeatureSetting setting)
    {
        TrySetLanguageLegacy();
    }

    private static void TrySetLanguageLegacy()
    {
        if (Is.R6OrLater)
            return;

        ModLanguage.TrySetLanguage(Settings.Language);
    }
}
