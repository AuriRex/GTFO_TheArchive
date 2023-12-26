using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;

namespace TheArchive.Features
{
    [EnableFeatureByDefault]
    [DisallowInGameToggle]
    public class ModLanguage : Feature
    {
        public override string Name => "Language";

        public override string Description => "Change Language of TheArchive Mod";

        [FeatureConfig]
        public static LanguageSetting Settings { get; set; }

        public class LanguageSetting
        {
            [FSDisplayName("语言", Language.Chinese)]
            [FSDisplayName("Language")]
            public Language Language { get; set; } = Language.Chinese;
        }

        public override void Init()
        {
            LocalizationCoreService.SetCurrentLanguage(Settings.Language);
        }
    }
}
