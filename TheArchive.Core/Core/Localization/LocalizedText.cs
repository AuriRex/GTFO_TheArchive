#if false
namespace TheArchive.Core.Localization;

[JsonConverter(typeof(LocalizedTextJsonConverter))]
public struct LocalizedText
{
    public bool HasTranslation => string.IsNullOrEmpty(UntranslatedText);

    public LocalizedText(string untranslatedText)
    {
        UntranslatedText = untranslatedText;
        Id = 0U;
    }

    public LocalizedText(uint id)
    {
        UntranslatedText = null;
        Id = id;
    }

    public static implicit operator string(LocalizedText localizedText)
    {
        return localizedText.ToString();
    }

    public static implicit operator LocalizedText(uint v)
    {
        return new LocalizedText(v);
    }

    public override string ToString()
    {
        if (!HasTranslation)
        {
            return UntranslatedText;
        }
        return LocalizationCoreService.Get(Id);
    }

    public string UntranslatedText;

    public uint Id;
}
#endif