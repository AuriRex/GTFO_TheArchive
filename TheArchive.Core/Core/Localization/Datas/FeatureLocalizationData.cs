using System.Collections.Generic;

namespace TheArchive.Core.Localization.Datas;

internal class FeatureLocalizationData
{
    public FeatureSettingLocalizationData Internal { get; set; } = new();

    public FeatureSettingLocalizationData External { get; set; } = new();

    public Dictionary<uint, GenericLocalizationData> GenericTexts { get; set; }
}
