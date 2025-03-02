using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Core.Localization;

internal class LocalizationCoreService
{
    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= Loader.LoaderWrapper.CreateLoggerInstance(nameof(LocalizationCoreService), ConsoleColor.Yellow);

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

    public static string Get<T>(T value) where T : Enum
    {
        Type type = typeof(T);
        if (type.IsEnum)
        {
            if (!_enumTexts.TryGetValue(type.FullName, out var languages) || !languages.TryGetValue(CurrentLanguage, out var enumTexts) || enumTexts.Any(p => string.IsNullOrWhiteSpace(p.Value)))
            {
                return value.ToString();
            }
            List<string> result = new();
            foreach (var v in Enum.GetValues(type))
            {
                if (!value.HasFlag((T)v))
                    continue;
                if (!enumTexts.TryGetValue(v.ToString(), out var tvalue))
                {
                    return value.ToString();
                }
                result.Add(tvalue);
            }
            return string.Join(", ", result);
        }
        return value.ToString();
    }

    public static string Format(uint id, string defaultValue = "UNKNOWN ID: {0}", params object[] args)
    {
        return string.Format(Get(id, defaultValue), args);
    }

    public static void RegisterLocalizationService(FeatureLocalizationService service)
    {
        _localizationServices.Add(service);
    }

    public static void RegisterInLocalizaion(Type type)
    {
        _typesToCheck.Add(type);
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
            LocalizationData = new();
            File.WriteAllText(path, JsonConvert.SerializeObject(LocalizationData, ArchiveMod.JsonSerializerSettings));
            return;
        }
        LocalizationData = JsonConvert.DeserializeObject<ArchiveCoreLocalizationData>(File.ReadAllText(path), ArchiveMod.JsonSerializerSettings);

        foreach (var item in LocalizationData.Texts)
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

        _enumTexts = LocalizationData.EnumTexts;

        RegisterType<FeatureInternal.InternalDisabledReason>();
        CheckLocalization();
    }

    public static void RegisterType<T>()
    {
        RegisterTypes.Add(typeof(T));
    }

    public static void CheckLocalization()
    {
        if (!RegisterTypes.Any())
            return;

        foreach (var type in RegisterTypes)
        {
            var key = type.FullName;

            if (type.IsEnum)
            {
                if (_enumTexts.ContainsKey(key))
                    continue;

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
                _enumTexts[key] = enumdic;
            }
        }

        LocalizationData.EnumTexts = _enumTexts;
        var rjson = JsonConvert.SerializeObject(LocalizationData, ArchiveMod.JsonSerializerSettings);
        string dir = Path.Combine(Path.GetDirectoryName(ArchiveMod.CORE_PATH), "Localization");
        var path = Path.Combine(dir, $"{nameof(LocalizationCoreService)}_Localization.json");
        if (File.Exists(path))
        {
            var data = JsonConvert.DeserializeObject<ArchiveCoreLocalizationData>(File.ReadAllText(path), ArchiveMod.JsonSerializerSettings);
            var json = JsonConvert.SerializeObject(data, ArchiveMod.JsonSerializerSettings);
            if (rjson.HashString() != json.HashString())
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(LocalizationData, ArchiveMod.JsonSerializerSettings));
            }
        }
        else
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(LocalizationData, ArchiveMod.JsonSerializerSettings));
        }
    }

    private static HashSet<Type> RegisterTypes = new HashSet<Type>();

    private static ArchiveCoreLocalizationData LocalizationData = new ArchiveCoreLocalizationData();

    public static Language CurrentLanguage { get; private set; } = Language.English;

    private static Dictionary<ILocalizedTextSetter, uint> _textSetters = new();

    private static HashSet<ILocalizedTextUpdater> _textUpdaters = new();

    private static HashSet<FeatureLocalizationService> _localizationServices = new();

    private static Dictionary<uint, Dictionary<Language, string>> _texts = new();

    private static HashSet<Type> _typesToCheck = new();

    private static Dictionary<string, Dictionary<Language, Dictionary<string, string>>> _enumTexts = new();
}