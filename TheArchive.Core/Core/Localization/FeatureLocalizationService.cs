using System;
using System.Collections.Generic;

namespace TheArchive.Core.Localization
{
    internal class FeatureLocalizationService : ILocalizationService
    {
        public Language CurrentLanguage { get; private set; }

        public void SetCurrentLanguage(Language language)
        {
            CurrentLanguage = language;
            UpdateAllTexts();
        }

        public void Setup(List<LocalizationTextData> data)
        {
            LocalizationCoreService.RegisterLocalizationService(this);
            Languages.Clear();
            foreach (var item in data)
            {
                Dictionary<Language, string> dict = new();
                foreach (Language lang in Enum.GetValues(typeof(Language)))
                {
                    if (!item.Languages.TryGetValue(lang, out var text))
                    {
                        text = item.UntranslatedText;
                    }
                    dict.TryAdd(lang, text);
                }
                Languages.TryAdd(item.ID, dict);
            }
        }
        public string GetString(uint id)
        {
            if (!Languages.TryGetValue(id, out var language) || !language.TryGetValue(CurrentLanguage, out var text))
            {
                return string.Empty;
            }
            return text;
        }

        public string FormatString(uint id, params object[] args)
        {
            return string.Format(GetString(id), args);
        }

        public void AddTextSetter(ILocalizedTextSetter textSetter, uint textId)
        {
            textSetter.SetText(GetString(textId));
            m_textSetters.Add(textSetter, textId);
        }

        public void SetTextSetter(ILocalizedTextSetter textSetter, uint textId)
        {
            textSetter.SetText(GetString(textId));
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
                keyValuePair.Key.SetText(GetString(keyValuePair.Value));
            }
            foreach (ILocalizedTextUpdater localizedTextUpdater in m_textUpdaters)
            {
                localizedTextUpdater.UpdateText();
            }
        }

        private Dictionary<uint, Dictionary<Language, string>> Languages { get; } = new();

        private Dictionary<ILocalizedTextSetter, uint> m_textSetters { get; } = new();

        private HashSet<ILocalizedTextUpdater> m_textUpdaters { get; } = new();
    }
}
