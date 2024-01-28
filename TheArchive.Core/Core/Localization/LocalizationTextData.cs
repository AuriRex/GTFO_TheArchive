using System.Collections.Generic;

namespace TheArchive.Core.Localization;

internal class LocalizationTextData
{
    public string UntranslatedText { get; set; }

    public string Description { get; set; }

    public uint ID { get; set; }

    public Dictionary<Language, string> Languages { get; set; }
}

internal class FeatureExternalLocalizationData
{
    public Dictionary<string, Dictionary<Language, Dictionary<string, string>>> ExternEnumTexts { get; set; }
}

internal class FeatureInternalLocalizationData
{
    public Dictionary<string, Dictionary<FSType, Dictionary<Language, string>>> FeatureSettingsTexts { get; set; }

    public Dictionary<string, Dictionary<Language, Dictionary<string, string>>> FeatureSettingsEnumTexts { get; set; }

    public List<LocalizationTextData> ExtraTexts { get; set; }
}

internal class FeatureLocalizationData
{
    public FeatureInternalLocalizationData Internal { get; set; } = new();

    public FeatureExternalLocalizationData External { get; set; } = new();
}

internal enum FSType
{
    FName,
    FDescription,
    FSDisplayName,
    FSDescription,
    FSHeader,
    FSLabelText,
    FSButtonText
}
