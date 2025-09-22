using System.Collections.Generic;
using TheArchive.Utilities;

namespace TheArchive.Core.Localization.Datas;

/// <summary>
/// Localization data for groups.
/// </summary>
public class GroupLocalizationData
{
    /// <summary>
    /// Localized display name.
    /// </summary>
    public Dictionary<Language, string> DisplayName { get; set; } = Utils.GetEmptyLanguageDictionary();
    
    /// <summary>
    /// Localized description.
    /// </summary>
    public Dictionary<Language, string> Description { get; set; } = Utils.GetEmptyLanguageDictionary();

    /// <summary>
    /// Gets the localized display name or english if null.
    /// </summary>
    /// <param name="language">The language to get.</param>
    /// <param name="fallback">The fallback to use as last resort.</param>
    /// <returns>The localized string.</returns>
    public string GetLocalizedDisplayName(Language language, string fallback = null)
    {
        return GetLocalized(language, DisplayName, fallback);
    }
    
    /// <summary>
    /// Gets the localized description or english if null.
    /// </summary>
    /// <param name="language">The language to get.</param>
    /// <param name="fallback">The fallback to use as last resort.</param>
    /// <returns>The localized string.</returns>
    public string GetLocalizedDescription(Language language, string fallback = null)
    {
        return GetLocalized(language, Description, fallback);
    }
    
    private static string GetLocalized(Language language, Dictionary<Language, string> dictionary, string fallback = null)
    {
        if (dictionary.TryGetValue(language, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (dictionary.TryGetValue(Language.English, out value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }

    /// <summary>
    /// Appends localization data from another GroupLocalizationData instance.
    /// Only adds entries that don't already exist in the current instance.
    /// </summary>
    /// <param name="data">The localization data to append.</param>
    public void AppendData(GroupLocalizationData data)
    {
        if (data == null)
            return;

        foreach ((var language, var text) in data.DisplayName)
        {
            if (!DisplayName.ContainsKey(language) || string.IsNullOrWhiteSpace(DisplayName[language]))
            {
                DisplayName[language] = text;
            }
        }

        foreach ((var language, var text) in data.Description)
        {
            if (!Description.ContainsKey(language) || string.IsNullOrWhiteSpace(Description[language]))
            {
                Description[language] = text;
            }
        }
    }
}