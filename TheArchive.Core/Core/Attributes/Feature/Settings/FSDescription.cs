using System;
using TheArchive.Core.Localization;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FSDescription : Localized
    {
        public string Description => UntranslatedText;
        public FSDescription(string description) : base(description)
        {
        }
    }
}
