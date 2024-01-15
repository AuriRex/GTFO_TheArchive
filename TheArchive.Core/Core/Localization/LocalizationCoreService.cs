using System;
using System.Collections.Generic;
using System.IO;
using TheArchive.Interfaces;

namespace TheArchive.Core.Localization
{
    internal class LocalizationCoreService
    {
        private static IArchiveLogger _logger;
        private static IArchiveLogger Logger => _logger ??= Loader.LoaderWrapper.CreateLoggerInstance(nameof(LocalizationCoreService), ConsoleColor.Yellow);

        public static void SetCurrentLanguage(Language language)
        {
            CurrentLanguage = language;

            foreach (var service in m_localizationServices)
            {
                try
                {
                    service.SetCurrentLanguage(CurrentLanguage);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Exception has been thrown in Feature {service.Feature.Name}-->SetCurrentLanguage. {ex}: {ex.Message}");
                    Logger.Exception(ex);
                }
            }

            UpdateAllTexts();
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

        public static void SetTextSetter(ILocalizedTextSetter textSetter, uint textId)
        {
            textSetter.SetText(Get(textId));
            m_textSetters[textSetter] = textId;
        }

        public static void AddTextUpdater(ILocalizedTextUpdater textUpdater)
        {
            textUpdater.UpdateText();
            m_textUpdaters.Add(textUpdater);
        }

        public static string Get(uint id, string defaultValue = "UNKNOWN ID: {0}")
        {
            if (!m_texts.TryGetValue(id, out var language) || !language.TryGetValue(CurrentLanguage, out var text) || string.IsNullOrWhiteSpace(text))
            {
                if (defaultValue == "UNKNOWN ID: {0}")
                {
                    defaultValue = string.Format(defaultValue, id);
                }
                return defaultValue;
            }
            return text;
        }

        public static string Format(uint id, string defaultValue = "UNKNOWN ID: {0}", params object[] args)
        {
            return string.Format(Get(id, defaultValue), args);
        }

        public static void RegisterLocalizationService(FeatureLocalizationService service)
        {
            m_localizationServices.Add(service);
        }

        public static void Init()
        {
            string dir = Path.Combine(Path.GetDirectoryName(ArchiveMod.CORE_PATH), "Localization");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var path = Path.Combine(dir, $"{nameof(LocalizationCoreService)}_Localization.json");
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(new(), ArchiveMod.JsonSerializerSettings));
                return;
            }
            var data = JsonConvert.DeserializeObject<List<LocalizationTextData>>(File.ReadAllText(path), ArchiveMod.JsonSerializerSettings);
            foreach (var item in data)
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
                m_texts[item.ID] = dic;
            }
        }

        public static Language CurrentLanguage { get; private set; } = Language.English;

        private static Dictionary<ILocalizedTextSetter, uint> m_textSetters = new();

        private static HashSet<ILocalizedTextUpdater> m_textUpdaters = new();

        private static HashSet<FeatureLocalizationService> m_localizationServices = new();

        private static Dictionary<uint, Dictionary<Language, string>> m_texts = new();
    }
}
