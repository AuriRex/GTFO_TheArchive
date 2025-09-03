using System;
using System.Collections.Generic;
using TheArchive.Utilities;

namespace TheArchive.Core.Localization;

/// <summary>
/// Localization data for your modules.
/// </summary>
public class ModuleLocalizationData
{
    /// <summary>
    /// Module group localization data.
    /// </summary>
    public GroupLocalization ModuleGroup { get; set; } = new();
    
    /// <summary>
    /// Generic localization texts
    /// </summary>
    public Dictionary<uint, GenericLocalization> GenericTexts { get; set; } = new();

    /// <summary>
    /// Enum type translations.
    /// </summary>
    public Dictionary<string, EnumLocalization> EnumTexts { get; set; } = new();
    
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

    /// <summary>
    /// A generic localization element.
    /// </summary>
    public class GenericLocalization
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

    /// <summary>
    /// Enum localization data.
    /// </summary>
    public class EnumLocalization
    {
        /// <summary>
        /// A comment to describe how this text is translated or where it is used.
        /// </summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// Localization entries for the given enum.
        /// </summary>
        public Dictionary<Language, Dictionary<string, string>> Entries { get; set; }

        /// <summary>
        /// Creates an empty tree structure for the provided enum <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The enum type to use.</typeparam>
        public void SetupEmptyForEnum<T>() where T : struct, Enum
        {
            Entries = Utils.GetEmptyLanguageDictionary(Utils.GetEmptyEnumDictionary<T>);
        }
    }
}