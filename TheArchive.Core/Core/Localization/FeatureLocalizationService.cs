using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

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
            LocalizationCoreService.RegisterLocalizationService(this);
            ExtraTexts.Clear();
            foreach (var property in data.FeatureSettingsTexts)
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

            foreach (var item in data.ExtraTexts)
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

            FeatureSettingsEnumTexts = data.FeatureSettingsEnumTexts;
        }

        public bool TryGetFSText(string propID, FSType type, out string text)
        {
            if (!FeatureSettingsTexts.TryGetValue(propID, out var typedic) || !typedic.TryGetValue(type, out var languages) || !languages.TryGetValue(CurrentLanguage, out text) || text.IsNullOrWhiteSpace() || string.IsNullOrEmpty(text))
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
            if (!FeatureSettingsEnumTexts.TryGetValue(enumType.FullName, out var languages) || !languages.TryGetValue(CurrentLanguage, out enumTexts) || enumTexts.Count != values.Length || enumTexts.Any(p => !p.Value.IsNullOrWhiteSpaceOrEmpty()))
            {
                enumTexts = null;
                return false;
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

        private Dictionary<uint, Dictionary<Language, string>> ExtraTexts { get; set; } = new();

        private Dictionary<string, Dictionary<FSType, Dictionary<Language, string>>> FeatureSettingsTexts { get; set; } = new();

        private Dictionary<string, Dictionary<Language, Dictionary<string, string>>> FeatureSettingsEnumTexts { get; set; } = new();

        private Dictionary<ILocalizedTextSetter, uint> m_textSetters { get; } = new();

        private HashSet<ILocalizedTextUpdater> m_textUpdaters { get; } = new();
    }
}
