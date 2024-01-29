using BepInEx;
using Clonesoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using static System.Net.Mime.MediaTypeNames;

namespace TheArchive.Core.Localization
{
    internal class FeatureLocalizationService : ILocalizationService
    {
        public Feature Feature { get; internal set; }

        public Language CurrentLanguage { get; private set; }

        public void SetCurrentLanguage(Language language)
        {
            CurrentLanguage = language;
            UpdateAllTexts();
        }

        public void Setup(Feature feature, FeatureLocalizationData data)
        {
            Feature = feature;
            LocalizationData = data;
            LocalizationCoreService.RegisterLocalizationService(this);

            ExtraTexts.Clear();
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
                FeatureSettingsTexts[property.Key] = typedic;
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
                ExtraTexts[item.ID] = dic;
            }

            FeatureSettingsEnumTexts = data.Internal.FeatureSettingsEnumTexts;

            ExternalEnumTexts = data.External.ExternalEnumTexts ?? new();
        }

        public bool TryGetFSText(string propID, FSType type, out string text)
        {
            if (!FeatureSettingsTexts.TryGetValue(propID, out var typedic) || !typedic.TryGetValue(type, out var languages) || !languages.TryGetValue(CurrentLanguage, out text) || string.IsNullOrWhiteSpace(text))
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
            if (!FeatureSettingsEnumTexts.TryGetValue(enumType.FullName, out var languages) || !languages.TryGetValue(CurrentLanguage, out enumTexts) || enumTexts.Count != values.Length || enumTexts.Any(p => string.IsNullOrWhiteSpace(p.Value)))
            {
                if (!ExternalEnumTexts.TryGetValue(enumType.FullName, out languages) || !languages.TryGetValue(CurrentLanguage, out enumTexts) || enumTexts.Count != values.Length || enumTexts.Any(p => string.IsNullOrWhiteSpace(p.Value)))
                {
                    enumTexts = null;
                    return false;
                }
            }
            return true;
        }

        public string Get(uint id)
        {
            if (!ExtraTexts.TryGetValue(id, out var language) || !language.TryGetValue(CurrentLanguage, out var text))
            {
                return $"UNKNOWN ID {id}";
            }
            return text;
        }


        public string Get<T>(T value)
        {
            Type type = typeof(T);
            if (type.IsEnum)
            {
                var values = Enum.GetNames(type);
                if (!ExternalEnumTexts.TryGetValue(type.FullName, out var languages) || !languages.TryGetValue(CurrentLanguage, out var enumTexts) || enumTexts.Count != values.Length || enumTexts.Any(p => string.IsNullOrWhiteSpace(p.Value)) || !enumTexts.TryGetValue(value.ToString(), out var text))
                {
                    return value.ToString();
                }
                return text;
            }
            return value.ToString();
        }


        public string Format(uint id, params object[] args)
        {
            return string.Format(Get(id), args);
        }

        public void AddTextSetter(ILocalizedTextSetter textSetter, uint textId)
        {
            textSetter.SetText(Get(textId));
            m_textSetters.Add(textSetter, textId);
        }

        public void SetTextSetter(ILocalizedTextSetter textSetter, uint textId)
        {
            textSetter.SetText(Get(textId));
            m_textSetters[textSetter] = textId;
        }

        public void AddTextUpdater(ILocalizedTextUpdater textUpdater)
        {
            textUpdater.UpdateText();
            m_textUpdaters.Add(textUpdater);
        }

        public void UpdateAllTexts()
        {
            foreach (KeyValuePair<ILocalizedTextSetter, uint> keyValuePair in m_textSetters)
            {
                keyValuePair.Key.SetText(Get(keyValuePair.Value));
            }
            foreach (ILocalizedTextUpdater localizedTextUpdater in m_textUpdaters)
            {
                localizedTextUpdater.UpdateText();
            }
        }

        public void RegisterExternType<T>()
        {
            RegisteredExternTypes.Add(typeof(T));
        }

        public void CheckExternLocalization()
        {
            if (!RegisteredExternTypes.Any())
                return;

            foreach (var type in RegisteredExternTypes)
            {
                var key = type.FullName;

                if (type.IsEnum)
                {
                    if (ExternalEnumTexts.ContainsKey(key))
                    {
                        ExternalEnumTexts[key] = ExternalEnumTexts[key].OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
                        continue;
                    }

                    var names = Enum.GetNames(type);
                    var enumdic = new Dictionary<Language, Dictionary<string, string>>();
                    foreach (Language language in Enum.GetValues(typeof(Language)))
                    {
                        var languagedic = new Dictionary<string, string>();
                        foreach (var name in names)
                        {
                            languagedic[name] = null;
                        }
                        enumdic[language] = languagedic;
                    }
                    ExternalEnumTexts[key] = enumdic;
                }
            }

            LocalizationData.External.ExternalEnumTexts = ExternalEnumTexts;
            var rjson = JsonConvert.SerializeObject(LocalizationData, ArchiveMod.JsonSerializerSettings);
            string dir = Path.Combine(Path.GetDirectoryName(Feature.ModuleGroup == ArchiveMod.ARCHIVE_CORE_FEATUREGROUP ? ArchiveMod.CORE_PATH : Feature.FeatureInternal.OriginAssembly.Location), "Localization");
            var path = Path.Combine(dir, $"{Feature.Identifier}_Localization.json");
            if (File.Exists(path))
            {
                var data = JsonConvert.DeserializeObject<FeatureLocalizationData>(File.ReadAllText(path), ArchiveMod.JsonSerializerSettings);
                var json = JsonConvert.SerializeObject(data, ArchiveMod.JsonSerializerSettings);
                if (rjson.ComputeSHA256() != json.ComputeSHA256())
                {
                    File.WriteAllText(path, JsonConvert.SerializeObject(LocalizationData, ArchiveMod.JsonSerializerSettings));
                }
            }
            else
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(LocalizationData, ArchiveMod.JsonSerializerSettings));
            }
        }
        private FeatureLocalizationData LocalizationData { get; set; }

        private HashSet<Type> RegisteredExternTypes { get; } = new();

        private Dictionary<uint, Dictionary<Language, string>> ExtraTexts { get; set; } = new();

        private Dictionary<string, Dictionary<FSType, Dictionary<Language, string>>> FeatureSettingsTexts { get; set; } = new();

        private Dictionary<string, Dictionary<Language, Dictionary<string, string>>> FeatureSettingsEnumTexts { get; set; } = new();

        private Dictionary<string, Dictionary<Language, Dictionary<string, string>>> ExternalEnumTexts { get; set; } = new();

        private Dictionary<ILocalizedTextSetter, uint> m_textSetters { get; } = new();

        private HashSet<ILocalizedTextUpdater> m_textUpdaters { get; } = new();
    }
}
