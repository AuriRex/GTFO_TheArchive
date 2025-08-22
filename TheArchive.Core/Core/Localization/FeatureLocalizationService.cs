using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;

namespace TheArchive.Core.Localization;

internal class FeatureLocalizationService : BaseLocalizationService
{
    public Feature Feature { get; }

    private readonly FeatureLocalizationData _localizationData;
    private readonly Dictionary<uint, Dictionary<Language, string>> _extraTexts = new();
    private readonly Dictionary<string, Dictionary<FSType, Dictionary<Language, string>>> _featureSettingsTexts = new();
    private Dictionary<string, Dictionary<Language, Dictionary<string, string>>> _featureSettingsEnumTexts = new();
    private Dictionary<string, Dictionary<Language, Dictionary<string, string>>> _externalEnumTexts = new();
    
    public FeatureLocalizationService(Feature feature, FeatureLocalizationData data, IArchiveLogger logger) : base(logger, feature.Name)
    {
        Feature = feature;
        _localizationData = data;
        
        Setup(data);
    }

    private void Setup(FeatureLocalizationData data)
    {
        LocalizationCoreService.RegisterLocalizationService(this);

        _featureSettingsTexts.Clear();
        _extraTexts.Clear();

        foreach (var property in data.Internal.FeatureSettingsTexts)
        {
            Dictionary<FSType, Dictionary<Language, string>> typedic = new();
            foreach (var type in property.Value)
            {
                Dictionary<Language, string> dic = new();
                foreach (Language lang in Enum.GetValues(typeof(Language)))
                {
                    if (!type.Value.TryGetValue(lang, out var text))
                    {
                        text = type.Value.FirstOrDefault().Value;
                    }
                    dic[lang] = text;
                }
                typedic[type.Key] = dic;
            }
            _featureSettingsTexts[property.Key] = typedic;
        }

        foreach (var property in data.External.ExternalFeatureSettingsTexts)
        {
            Dictionary<FSType, Dictionary<Language, string>> typedic = new();
            foreach (var type in property.Value)
            {
                Dictionary<Language, string> dic = new();
                foreach (Language lang in Enum.GetValues(typeof(Language)))
                {
                    if (!type.Value.TryGetValue(lang, out var text))
                    {
                        text = type.Value.FirstOrDefault().Value;
                    }
                    dic[lang] = text;
                }
                typedic[type.Key] = dic;
            }
            _featureSettingsTexts[property.Key] = typedic;
        }

        foreach (var item in data.Internal.ExtraTexts)
        {
            Dictionary<Language, string> dic = new();
            foreach (Language lang in Enum.GetValues(typeof(Language)))
            {
                if (!item.Languages.TryGetValue(lang, out var text))
                {
                    text = item.UntranslatedText;
                }
                dic[lang] = text;
            }
            _extraTexts[item.ID] = dic;
        }

        _featureSettingsEnumTexts = data.Internal.FeatureSettingsEnumTexts;

        _externalEnumTexts = data.External.ExternalEnumTexts ?? new();
    }

    public bool TryGetFSText(string propID, FSType type, out string text)
    {
        if (!_featureSettingsTexts.TryGetValue(propID, out var typedic) || !typedic.TryGetValue(type, out var languages) || !languages.TryGetValue(CurrentLanguage, out text) || string.IsNullOrWhiteSpace(text))
        {
            text = null;
            return false;
        }
        return true;
    }

    public bool TryGetFSEnumText(Type enumType, out Dictionary<string, string> enumTexts)
    {
        if (enumType == null)
        {
            enumTexts = null;
            return false;
        }
        var values = Enum.GetNames(enumType);
        if (!_featureSettingsEnumTexts.TryGetValue(enumType.FullName, out var languages) || !languages.TryGetValue(CurrentLanguage, out enumTexts) || enumTexts.Count != values.Length || enumTexts.Any(p => string.IsNullOrWhiteSpace(p.Value)))
        {
            if (!_externalEnumTexts.TryGetValue(enumType.FullName, out languages) || !languages.TryGetValue(CurrentLanguage, out enumTexts) || enumTexts.Count != values.Length || enumTexts.Any(p => string.IsNullOrWhiteSpace(p.Value)))
            {
                enumTexts = null;
                return false;
            }
        }
        return true;
    }

    public override string Get(uint id)
    {
        if (!_extraTexts.TryGetValue(id, out var language) || !language.TryGetValue(CurrentLanguage, out var text))
        {
            return $"UNKNOWN ID {id}";
        }
        return text;
    }

    public override string Get(Type type, object value)
    {
        if (type.IsEnum)
        {
            if (TryGetFSEnumText(type, out var dic) && dic.TryGetValue(value.ToString(), out var text))
            {
                return text;
            }
            return value.ToString();
        }
        return value.ToString();
    }
}