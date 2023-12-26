using System;
using TheArchive.Core.Localization;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FSDisplayName : Localized
    {
        public string DisplayName { get; private set; }
        public FSDisplayName(string displayName, Language language = Language.English) : base(displayName, FSTarget.FSDisplayName, language)
        {
            DisplayName = displayName;
        }
    }
}
