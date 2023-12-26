using System;

namespace TheArchive.Core.Localization
{
    public interface ILocalized
    {
        string TranslatedText { get; }
        Language Language { get; }
        uint ID { get; }
    }
}
