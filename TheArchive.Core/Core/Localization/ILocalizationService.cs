using System;

namespace TheArchive.Core.Localization;

public interface ILocalizationService
{
    Language CurrentLanguage { get; }

    string Get(uint id);

    string Get<T>(T value) where T : Enum;

    string Format(uint id, params object[] args);

    void AddTextSetter(ILocalizedTextSetter textSetter, uint textId);

    void SetTextSetter(ILocalizedTextSetter textSetter, uint textId);

    void AddTextUpdater(ILocalizedTextUpdater textUpdater);
}