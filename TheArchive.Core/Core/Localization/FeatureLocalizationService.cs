using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArchive.Core.Localization
{
    internal class FeatureLocalizationService : ILocalizationService
    {
        public Language CurrentLanguage { get; private set; }

        public void SetCurrentLanguage(Language language)
        {
            CurrentLanguage = language;
        }

        public void Setup(FeatureLocalizationData data)
        {
            LocalizationCoreService.RegisterLocalizationService(this);
            DynamicTexts.Clear();
            foreach (var item in data.FeaturePropertyTexts)
            {
                Dictionary<Language, string> dic = new();
                foreach (Language lang in Enum.GetValues(typeof(Language)))
                {
                    if (!item.Value.TryGetValue(lang, out var text))
                    {
                        text = item.Value.FirstOrDefault().Value;
                    }
                    dic[lang] = text;
                }
                PropertyTexts[item.Key] = dic;
            }
            foreach (var item in data.DynamicTexts)
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
                DynamicTexts[item.ID] = dic;
            }
        }

        public string GetProperty(string propID)
        {
            if (!PropertyTexts.TryGetValue(propID, out var language) || !language.TryGetValue(CurrentLanguage, out var text))
            {
                return null;
            }
            return text;
        }

        public string Get(uint id)
        {
            if (!DynamicTexts.TryGetValue(id, out var language) || !language.TryGetValue(CurrentLanguage, out var text))
            {
                return string.Empty;
            }
            return text;
        }

        public string Format(uint id, params object[] args)
        {
            return string.Format(Get(id), args);
        }

        /*
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
        */

        private Dictionary<uint, Dictionary<Language, string>> DynamicTexts { get; } = new();

        private Dictionary<string, Dictionary<Language, string>> PropertyTexts { get; } = new();

        /*
        private Dictionary<ILocalizedTextSetter, uint> m_textSetters { get; } = new();

        private HashSet<ILocalizedTextUpdater> m_textUpdaters { get; } = new();
        */
    }
}
