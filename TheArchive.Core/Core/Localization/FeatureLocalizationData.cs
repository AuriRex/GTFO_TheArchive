using System.Collections.Generic;

namespace TheArchive.Core.Localization
{
    public class FeatureLocalizationData
    {
        public Dictionary<string, Dictionary<FSType, Dictionary<Language, string>>> FeatureSettingsTexts { get; set; }

        public Dictionary<string, Dictionary<Language, Dictionary<string, string>>> FeatureSettingsEnumTexts { get; set; }

        public List<LocalizationTextData> ExtraTexts { get; set; }
    }
}
