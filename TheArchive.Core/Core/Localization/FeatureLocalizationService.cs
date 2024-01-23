using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.FeaturesAPI;

namespace TheArchive.Core.Localization
{
    internal class FeatureLocalizationService : ILocalizationService
    {
        public Feature Feature { get; internal set; }

        public Language CurrentLanguage { get; private set; }

        private readonly Dictionary<uint, Dictionary<Language, string>> _extraTexts = new();

        private readonly Dictionary<string, Dictionary<FSType, Dictionary<Language, string>>> _featureSettingsTexts = new();

        private Dictionary<string, Dictionary<Language, Dictionary<string, string>>> _featureSettingsEnumTexts = new();

        private readonly Dictionary<ILocalizedTextSetter, uint> _textSetters = new();

        private readonly HashSet<ILocalizedTextUpdater> _textUpdaters = new();

        public void Setup(Feature feature, FeatureLocalizationData data)
        {
            Feature = feature;
            LocalizationCoreService.RegisterLocalizationService(this);
            _extraTexts.Clear();
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
                _featureSettingsTexts[property.Key] = typedic;
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
                _extraTexts[item.ID] = dic;
            }

            _featureSettingsEnumTexts = data.FeatureSettingsEnumTexts;
        }

        public void SetCurrentLanguage(Language language)
        {
            CurrentLanguage = language;
            UpdateAllTexts();
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
                enumTexts = null;
                return false;
            }
            return true;
        }

        public string Get(uint id)
        {
            if (!_extraTexts.TryGetValue(id, out var language) || !language.TryGetValue(CurrentLanguage, out var text))
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
            _textSetters.Add(textSetter, textId);
        }

        public void SetTextSetter(ILocalizedTextSetter textSetter, uint textId)
        {
            textSetter.SetText(Get(textId));
            _textSetters[textSetter] = textId;
        }

        public void AddTextUpdater(ILocalizedTextUpdater textUpdater)
        {
            textUpdater.UpdateText();
            _textUpdaters.Add(textUpdater);
        }

        public void UpdateAllTexts()
        {
            foreach (KeyValuePair<ILocalizedTextSetter, uint> keyValuePair in _textSetters)
            {
                keyValuePair.Key.SetText(Get(keyValuePair.Value));
            }
            foreach (ILocalizedTextUpdater localizedTextUpdater in _textUpdaters)
            {
                localizedTextUpdater.UpdateText();
            }
        }
    }
}
