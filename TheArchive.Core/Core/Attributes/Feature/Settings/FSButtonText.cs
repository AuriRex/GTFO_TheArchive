using System;
using TheArchive.Core.Localization;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FSButtonText : Localized
    {
        public string FButtonText => UntranslatedText;
        public FSButtonText(string buttonName) : base(buttonName)
        {
        }
    }
}
