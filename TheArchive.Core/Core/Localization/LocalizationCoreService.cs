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

        public static Language CurrentLanguage { get; private set; } = Language.English;

        private static readonly Dictionary<ILocalizedTextSetter, uint> _textSetters = new();

        private static readonly HashSet<ILocalizedTextUpdater> _textUpdaters = new();

        private static readonly HashSet<FeatureLocalizationService> _localizationServices = new();

        private static readonly Dictionary<uint, Dictionary<Language, string>> _texts = new();

        public static void Init()
        {
            string dir = Path.Combine(Path.GetDirectoryName(ArchiveMod.CORE_PATH), $"Localization");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var path = Path.Combine(dir, $"{nameof(LocalizationCoreService)}_Localization.json");
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(new List<LocalizationTextData>(), ArchiveMod.JsonSerializerSettings));
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
                _texts[item.ID] = dic;
            }
        }

        public static void SetCurrentLanguage(Language language)
        {
            CurrentLanguage = language;

            foreach (var service in _localizationServices)
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
            foreach (KeyValuePair<ILocalizedTextSetter, uint> keyValuePair in _textSetters)
            {
                keyValuePair.Key.SetText(Get(keyValuePair.Value));
            }
            foreach (ILocalizedTextUpdater localizedTextUpdater in _textUpdaters)
            {
                localizedTextUpdater.UpdateText();
            }
        }

        public static void AddTextSetter(ILocalizedTextSetter textSetter, uint textId)
        {
            textSetter.SetText(Get(textId));
            _textSetters.Add(textSetter, textId);
        }

        public static void SetTextSetter(ILocalizedTextSetter textSetter, uint textId)
        {
            textSetter.SetText(Get(textId));
            _textSetters[textSetter] = textId;
        }

        public static void AddTextUpdater(ILocalizedTextUpdater textUpdater)
        {
            textUpdater.UpdateText();
            _textUpdaters.Add(textUpdater);
        }

        public static string Get(uint id, string defaultValue = "UNKNOWN ID: {0}")
        {
            if (!_texts.TryGetValue(id, out var language) || !language.TryGetValue(CurrentLanguage, out var text) || string.IsNullOrWhiteSpace(text))
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
            _localizationServices.Add(service);
        }
    }
}
