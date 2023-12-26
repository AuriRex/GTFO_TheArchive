using System.Collections.Generic;

namespace TheArchive.Core.Localization
{
    public interface ILocalizationService
    {
        Language CurrentLanguage { get; }

        void SetCurrentLanguage(Language language);

        string GetString(uint id);

        string FormatString(uint id, params object[] args);

        /*
        void AddTextSetter(ILocalizedTextSetter textSetter, uint textId);

        void SetTextSetter(ILocalizedTextSetter textSetter, uint textId);

        void AddTextUpdater(ILocalizedTextUpdater textUpdater);
        */
    }

    public enum Language
    {
        English = 1,
        French,
        Italian,
        German,
        Spanish,
        Russian,
        Portuguese_Brazil,
        Polish,
        Japanese,
        Korean,
        Chinese
    }
}
