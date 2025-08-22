using System;
using System.Collections.Generic;
using TheArchive.Utilities;

namespace TheArchive.Core.Localization;

public class ModuleLocalizationData
{
    public GroupLocalization ModuleGroup { get; set; } = new();
    
    public Dictionary<uint, GenericLocalization> GenericTexts { get; set; } = new();

    public bool TryGetGenericText(uint id, Language language, out string value, string fallback = null)
    {
        value = fallback;
        
        if (!GenericTexts.TryGetValue(id, out var genericLocalization))
        {
            return false;
        }

        if (!genericLocalization.TryGet(language, out var text))
        {
            return false;
        }

        value = text;
        return true;
    }
    
    public Dictionary<string, EnumLocalization> EnumTexts { get; set; } = new();

    public class GenericLocalization
    {
        /// <summary>
        /// A comment to describe how this text is translated or where it is used.
        /// </summary>
        public string Comment { get; set; } = string.Empty;
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

    public class EnumLocalization
    {
        /// <summary>
        /// A comment to describe how this text is translated or where it is used.
        /// </summary>
        public string Comment { get; set; } = string.Empty;

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