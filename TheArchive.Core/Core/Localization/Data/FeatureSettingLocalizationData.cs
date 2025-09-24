using System.Collections.Generic;

namespace TheArchive.Core.Localization.Data;

internal class FeatureSettingLocalizationData
{
    public Dictionary<string, Dictionary<FSType, Dictionary<Language, string>>> FeatureSettingTexts { get; set; } = new();

    public Dictionary<string, Dictionary<Language, Dictionary<string, string>>> EnumTexts { get; set; } = new();
}