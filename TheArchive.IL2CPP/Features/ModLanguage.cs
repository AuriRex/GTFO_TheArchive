using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;

namespace TheArchive.Features;

// 仍未完全实现
[HideInModSettings]
[EnableFeatureByDefault]
[DisallowInGameToggle]
public class ModLanguage : Feature
{
    public override string Name => "Language";

    public override string Description => "Change Language of TheArchive";

    private static Language GameLanguageToModLanguage(Localization.Language gameLanguage)
    {
        switch (gameLanguage)
        {
            case global::Localization.Language.Chinese_Simplified:
            case global::Localization.Language.Chinese_Traditional:
                return Language.Chinese;
            case global::Localization.Language.English:
            default:
                return Language.English;
        }
    }

    [ArchivePatch(typeof(global::Localization.GameDataTextLocalizationService), nameof(global::Localization.GameDataTextLocalizationService.SetCurrentLanguage))]
    private class GameDataTextLocalizationService__SetCurrentLanguage__Patch
    {
        private static void Postfix(Localization.Language language)
        {
            Language targetLanguage = GameLanguageToModLanguage(language);
            if (LocalizationCoreService.CurrentLanguage != targetLanguage)
            {
                LocalizationCoreService.SetCurrentLanguage(targetLanguage);

                /*
                // Exception here
                CM_PageSettings_Setup_Patch.DestroyModSettingsPage();
                CM_PageSettings_Setup_Patch.SetupMainModSettingsPage();

                ArchiveLogger.Msg($"Language changed to {LocalizationCoreService.CurrentLanguage}");
                */
            }
        }
    }
}