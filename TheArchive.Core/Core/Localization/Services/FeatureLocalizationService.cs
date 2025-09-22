using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization.Datas;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Core.Localization.Services;

internal class FeatureLocalizationService : BaseLocalizationService
{
    public Feature Feature { get; }

    private readonly Dictionary<uint, Dictionary<Language, string>> _genericTexts = new();
    private readonly Dictionary<string, Dictionary<FSType, Dictionary<Language, string>>> _featureSettingTexts = new();
    private Dictionary<string, Dictionary<Language, Dictionary<string, string>>> _internalEnumTexts = new();
    private Dictionary<string, Dictionary<Language, Dictionary<string, string>>> _externalEnumTexts = new();
    
    public FeatureLocalizationService(Feature feature, IArchiveLogger logger) : base(logger, feature.Name)
    {
        Feature = feature;
    }

    private FeatureLocalizationData LoadFeatureLocalizationText()
    {
        var asmLocation = Feature.FeatureInternal.OriginAssembly.Location;

        if (string.IsNullOrWhiteSpace(asmLocation))
        {
            Logger.Warning($"Feature \"{Feature.Name}\"'s OriginAssembly.Location is null or whitespace. (ID:{Feature.Identifier})");
            Logger.Warning("Localization on dynamic assemblies is not supported currently!");
            return new();
        }

        var dir = Path.Combine(Path.GetDirectoryName(asmLocation)!, "Localization");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        var path = Path.Combine(dir, $"Feature_{Feature.Identifier}_Localization.json");
        if (!File.Exists(path))
        {
            var newData = FeatureInternal.GenerateFeatureLocalization(Feature);
            File.WriteAllText(path, JsonConvert.SerializeObject(newData, ArchiveMod.JsonSerializerSettings));
            return newData;
        }

        var data = JsonConvert.DeserializeObject<FeatureLocalizationData>(File.ReadAllText(path), ArchiveMod.JsonSerializerSettings);
        var json = JsonConvert.SerializeObject(data, ArchiveMod.JsonSerializerSettings);
        var rdata = FeatureInternal.GenerateFeatureLocalization(Feature, data);
        var rjson = JsonConvert.SerializeObject(rdata, ArchiveMod.JsonSerializerSettings);
        if (rjson.HashString() != json.HashString())
            File.WriteAllText(path, rjson);
        return rdata;
    }

    public void Setup()
    {
        _featureSettingTexts.Clear();
        _genericTexts.Clear();

        var data = LoadFeatureLocalizationText();

        foreach (var property in data.Internal.FeatureSettingTexts)
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
            _featureSettingTexts[property.Key] = typedic;
        }

        foreach (var property in data.External.FeatureSettingTexts)
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
            _featureSettingTexts[property.Key] = typedic;
        }

        foreach ((var id, var gdata) in data.GenericTexts)
        {
            Dictionary<Language, string> dic = new();
            foreach (Language lang in Enum.GetValues(typeof(Language)))
            {
                if (!gdata.Data.TryGetValue(lang, out var text))
                {
                    gdata.Data.TryGetValue(Language.English, out text);
                }
                dic[lang] = text;
            }
            _genericTexts[id] = dic;
        }

        _internalEnumTexts = data.Internal.EnumTexts;
        _externalEnumTexts = data.External.EnumTexts;

        ArchiveLocalizationService.RegisterLocalizationService(this);
    }

    public bool TryGetFSText(string propID, FSType type, out string text)
    {
        if (!_featureSettingTexts.TryGetValue(propID, out var typedic)
            || !typedic.TryGetValue(type, out var languages)
            || !languages.TryGetValue(CurrentLanguage, out text)
            || string.IsNullOrWhiteSpace(text))
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
        if (!_internalEnumTexts.TryGetValue(enumType.FullName, out var languages)
            || !languages.TryGetValue(CurrentLanguage, out enumTexts)
            || enumTexts.Count != values.Length || enumTexts.Any(p => string.IsNullOrWhiteSpace(p.Value)))
        {
            if (!_externalEnumTexts.TryGetValue(enumType.FullName, out languages)
                || !languages.TryGetValue(CurrentLanguage, out enumTexts)
                || enumTexts.Count != values.Length || enumTexts.Any(p => string.IsNullOrWhiteSpace(p.Value)))
            {
                enumTexts = null;
                return false;
            }
        }
        return true;
    }

    public override string GetById(uint id, string fallback = null)
    {
        if (!_genericTexts.TryGetValue(id, out var language) || !language.TryGetValue(CurrentLanguage, out var text))
        {
            if (!string.IsNullOrWhiteSpace(fallback))
                return fallback;
            return $"UNKNOWN ID: {id}";
        }
        return text;
    }

    public override string Get(Type type, object value)
    {
        if (type.IsEnum)
        {
            if (!TryGetFSEnumText(type, out var enumTexts))
            {
                return value.ToString();
            };
            bool isFlags = type.GetCustomAttribute<FlagsAttribute>() != null;

            if (isFlags)
            {
                return GetFlagsEnumText(type, value, enumTexts);
            }
            else
            {
                return GetSingleEnumText(value, enumTexts);
            }
        }
        return value.ToString();
    }
}