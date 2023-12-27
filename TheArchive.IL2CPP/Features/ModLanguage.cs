using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;
using TheArchive.Features.Dev;
using TheArchive.Utilities;

namespace TheArchive.Features;

// 仍未完全实现
[HideInModSettings]
[EnableFeatureByDefault]
[DisallowInGameToggle]
public class ModLanguage : Feature
{
    public override string Name => "Language";

    public override string Description => "Change Language of ModSettings";

    public override string Group => FeatureGroups.ArchiveCore;

    private static Language GameLanguageToModLanguage(Localization.Language gameLanguage)
    {
        switch (gameLanguage)
        {
            case global::Localization.Language.Chinese_Simplified:
            case global::Localization.Language.Chinese_Traditional:
                return Language.Chinese;
            case global::Localization.Language.French:
                return Language.French;
            case global::Localization.Language.Italian:
                return Language.Italian;
            case global::Localization.Language.German:
                return Language.German;
            case global::Localization.Language.Spanish:
                return Language.Spanish;
            case global::Localization.Language.Russian:
                return Language.Russian;
            case global::Localization.Language.Portuguese_Brazil:
                return Language.Portuguese_Brazil;
            case global::Localization.Language.Polish:
                return Language.Polish;
            case global::Localization.Language.Japanese:
                return Language.Japanese;
            case global::Localization.Language.Korean:
                return Language.Korean;
            case global::Localization.Language.English:
            default:
                return Language.English;
        }
    }

    [ArchivePatch(typeof(Localization.GameDataTextLocalizationService), nameof(global::Localization.GameDataTextLocalizationService.SetCurrentLanguage))]
    private class GameDataTextLocalizationService__SetCurrentLanguage__Patch
    {
        private static void Postfix(Localization.Language language)
        {
            Language targetLanguage = GameLanguageToModLanguage(language);
            if (targetLanguage != LocalizationCoreService.CurrentLanguage)
            {
                LocalizationCoreService.SetCurrentLanguage(targetLanguage);

                FeatureInternal.RegenerateAllFeatureSettings();
                ModSettings.RegenerateModSettingsPage();
            }
        }
    }
}
