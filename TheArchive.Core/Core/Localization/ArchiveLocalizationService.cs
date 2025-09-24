using System;
using System.Collections.Generic;
using TheArchive.Interfaces;
using TheArchive.Loader;

namespace TheArchive.Core.Localization;

internal static class ArchiveLocalizationService
{
    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= LoaderWrapper.CreateLoggerInstance(nameof(ArchiveLocalizationService), ConsoleColor.Yellow);

    private static BaseLocalizationService _localization;

    public static void SetCurrentLanguage(Language language)
    {
        _localization.SetCurrentLanguage(language);

        foreach (var service in _localizationServices)
        {
            try
            {
                service.SetCurrentLanguage(language);
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception has been thrown in {service.Identifier} --> {nameof(SetCurrentLanguage)}. {ex}: {ex.Message}");
                Logger.Exception(ex);
            }
        }
    }

    public static void AddTextSetter(ILocalizedTextSetter textSetter, uint textId) => _localization.AddTextSetter(textSetter, textId);

    public static void RemoveTextSetter(ILocalizedTextSetter textSetter) => _localization.RemoveTextSetter(textSetter);

    public static void AddTextUpdater(ILocalizedTextUpdater textUpdater) => _localization.AddTextUpdater(textUpdater);

    public static void RemoveTextUpdater(ILocalizedTextUpdater textUpdater) => _localization.RemoveTextUpdater(textUpdater);

    public static string GetById(uint id, string fallback = null) => _localization.GetById(id, fallback);

    public static string Get<T>(T value) => _localization.Get(value);

    public static string Format(uint id, string fallback = null, params object[] args) => _localization.Format(id, fallback, args);

    public static void RegisterLocalizationService(BaseLocalizationService service)
    {
        if (service != _localization)
            _localizationServices.Add(service);
    }

    public static void Setup(ILocalizationService localization)
    {
        _localization = localization as BaseLocalizationService;
    }

    public static Language CurrentLanguage => _localization.CurrentLanguage;

    private static HashSet<BaseLocalizationService> _localizationServices = new();
}