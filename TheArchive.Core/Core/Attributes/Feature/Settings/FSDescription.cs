using System;
using TheArchive.Core.Localization;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FSDescription : Localized
    {
        public string Description { get; private set; }
        public FSDescription(string description, Language language = Language.English) : base(description, FSTarget.FSDescription, language)
        {
            Description = description;
        }
    }
}
