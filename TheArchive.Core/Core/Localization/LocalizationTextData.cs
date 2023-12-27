using System.Collections.Generic;

namespace TheArchive.Core.Localization;

public class LocalizationTextData
{
    public string UntranslatedText { get; set; }

    public string Description { get; set; }

    public uint ID { get; set; }

    public Dictionary<Language, string> Languages { get; set; }
}

public class FeatureLocalizationData
{
    public Dictionary<string, Dictionary<FSType, Dictionary<Language, string>>> FeatureSettingsTexts { get; set; }

    public List<LocalizationTextData> ExtraTexts { get; set; }
}

public enum FSType
{
    FSDisplayName,
    FSDescription,
    FSHeader,
    FSLabel
}
