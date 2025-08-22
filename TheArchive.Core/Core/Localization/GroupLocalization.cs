using System.Collections.Generic;
using TheArchive.Utilities;

namespace TheArchive.Core.Localization;

public class GroupLocalization
{
    public Dictionary<Language, string> DisplayName { get; set; } = Utils.GetEmptyLanguageDictionary();
    public Dictionary<Language, string> Description { get; set; } = Utils.GetEmptyLanguageDictionary();
}