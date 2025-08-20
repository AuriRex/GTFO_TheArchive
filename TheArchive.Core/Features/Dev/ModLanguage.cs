using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;

namespace TheArchive.Features.Dev;

[HideInModSettings]
[EnableFeatureByDefault]
[DisallowInGameToggle]
internal class ModLanguage : Feature
{
    public override string Name => "Mod Language";

    public override string Description => "Change Language of ModSettings";

    public override FeatureGroup Group => FeatureGroups.ArchiveCore;

    public new static IArchiveLogger FeatureLogger { get; set; }

    public override void Init()
    {
        LocalizationCoreService.Init();
    }

    public static bool TrySetLanguage(Language targetLanguage)
    {
        if (targetLanguage != LocalizationCoreService.CurrentLanguage)
        {
            FeatureLogger.Notice($"Setting Language to {targetLanguage}.");
            LocalizationCoreService.SetCurrentLanguage(targetLanguage);

            FeatureInternal.ReloadAllFeatureSettings();
            ModSettings.RegenerateModSettingsPage();
            return true;
        }
        return false;
    }

#if IL2CPP
    // Inline so it doesn't throw TypeLoadException on R4/R5
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [RundownConstraint(Utilities.Utils.RundownFlags.RundownSix, Utilities.Utils.RundownFlags.Latest)]
    [ArchivePatch(null, nameof(global::Localization.GameDataTextLocalizationService.SetCurrentLanguage))]
    private class GameDataTextLocalizationService__SetCurrentLanguage__Patch
    {
        public static System.Type Type() => typeof(Localization.GameDataTextLocalizationService);

        private static void Postfix(Localization.Language language)
        {
            Language targetLanguage = GameLanguageToModLanguage(language);
            ModLanguageLegacy.StoreLanguage(targetLanguage);
            TrySetLanguage(targetLanguage);
        }
    }
#endif
}
