using System.Collections.Generic;

namespace TheArchive.Core.Localization.Data;

/// <summary>
/// A generic localization element.
/// </summary>
public class GenericLocalizationData
{
    /// <summary>
    /// A comment to describe how this text is translated or where it is used.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Translation data.
    /// </summary>
    public Dictionary<Language, string> Data { get; set; } = new();

    /// <summary>
    /// Try to get a translated value, else fall back to english.
    /// </summary>
    /// <param name="language"></param>
    /// <param name="value"></param>
    /// <param name="fallback"></param>
    /// <returns></returns>
    public bool TryGet(Language language, out string value, string fallback = null)
    {
        if (Data.TryGetValue(language, out value) && !string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (Data.TryGetValue(Language.English, out value) && !string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (fallback == null)
        {
            value = "Error: All values null.";
            return false;
        }

        value = fallback;
        return true;
    }
}
