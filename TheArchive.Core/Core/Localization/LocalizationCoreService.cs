using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Utilities;

namespace TheArchive.Core.Localization
{
    internal class LocalizationCoreService
    {
        public static void SetCurrentLanguage(Language language)
        {
            CurrentLanguage = language;

            foreach (var service in m_localizationServices)
            {
                service.SetCurrentLanguage(CurrentLanguage);
            }

            //UpdateAllTexts();
        }

        public static void UpdateAllTexts()
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

        public static void AddTextSetter(ILocalizedTextSetter textSetter, uint textId)
        {
            textSetter.SetText(Get(textId));
            m_textSetters.Add(textSetter, textId);
        }

        public static void AddTextSetterDynamic(ILocalizedTextSetter textSetter, uint textId)
        {
            textSetter.SetText(Get(textId));
            m_textSettersDynamic.Add(textSetter, textId);
        }

        public static void SetTextSetter(ILocalizedTextSetter textSetter, uint textId)
        {
            textSetter.SetText(Get(textId));
            m_textSetters[textSetter] = textId;
        }

        public static void SetTextSetterDynamic(ILocalizedTextSetter textSetter, uint textId)
        {
            textSetter.SetText(Get(textId));
            m_textSettersDynamic[textSetter] = textId;
        }

        public static void AddTextUpdater(ILocalizedTextUpdater textUpdater)
        {
            textUpdater.UpdateText();
            m_textUpdaters.Add(textUpdater);
        }

        public static string Get(uint id)
        {
            if (!m_texts.TryGetValue(id, out var language) || !language.TryGetValue(CurrentLanguage, out var text))
            {
                return string.Empty;
            }
            return text;
        }

        public static string Format(uint id, params object[] args)
        {
            return string.Format(Get(id), args);
        }

        internal static void RegisterLocalizationService(ILocalizationService service)
        {
            m_localizationServices.Add(service);
        }

        public static Language CurrentLanguage { get; private set; } = Language.English;

        private static Dictionary<ILocalizedTextSetter, uint> m_textSetters = new();

        private static Dictionary<ILocalizedTextSetter, uint> m_textSettersDynamic = new();

        private static HashSet<ILocalizedTextUpdater> m_textUpdaters = new();

        private static HashSet<ILocalizationService> m_localizationServices = new();

        private static Dictionary<uint, Dictionary<Language, string>> m_texts = new();
    }
}
