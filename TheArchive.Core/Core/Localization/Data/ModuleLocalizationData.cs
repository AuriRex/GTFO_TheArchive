using System.Collections.Generic;

namespace TheArchive.Core.Localization.Data;

/// <summary>
/// Localization data for modules.
/// </summary>
public class ModuleLocalizationData
{
    /// <summary>
    /// Generic localization texts.
    /// </summary>
    public Dictionary<uint, GenericLocalizationData> GenericTexts { get; set; } = new();

    /// <summary>
    /// Enum type translations.
    /// </summary>
    public Dictionary<string, EnumLocalizationData> EnumTexts { get; set; } = new();

    /// <summary>
    /// Gets a translated text for the given ID, uses english as fallback if the translated value does not exist.
    /// </summary>
    /// <param name="id">The localization ID.</param>
    /// <param name="language">The language to get.</param>
    /// <param name="value">The translated value.</param>
    /// <param name="fallback">A fallback value.</param>
    /// <returns>The translated text.</returns>
    public bool TryGetGenericText(uint id, Language language, out string value, string fallback = null)
    {
        value = fallback;

        if (!GenericTexts.TryGetValue(id, out var genericLocalization) || genericLocalization == null)
        {
            return false;
        }

        if (genericLocalization.TryGet(language, out var text) && !string.IsNullOrWhiteSpace(text))
        {
            value = text;
            return true;
        }

        if (genericLocalization.TryGet(Language.English, out text) && !string.IsNullOrWhiteSpace(text))
        {
            value = text;
            return true;
        }

        return false;
    }
}