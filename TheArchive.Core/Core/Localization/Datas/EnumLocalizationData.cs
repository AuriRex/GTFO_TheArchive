using System;
using System.Collections.Generic;
using TheArchive.Utilities;

namespace TheArchive.Core.Localization.Datas;

/// <summary>
/// Enum localization data.
/// </summary>
public class EnumLocalizationData
{
    /// <summary>
    /// A comment to describe how this text is translated or where it is used.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Localization entries for the given enum.
    /// </summary>
    public Dictionary<Language, Dictionary<string, string>> Entries { get; set; } = new();

    /// <summary>
    /// Creates an empty tree structure for the provided enum <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The enum type to use.</typeparam>
    public void SetupEmptyForEnum<T>() where T : struct, Enum
    {
        Entries = Utils.GetEmptyLanguageDictionary(Utils.GetEmptyEnumDictionary<T>);
    }
}
