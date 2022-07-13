using System;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FSDisplayName : Attribute
    {
        public string DisplayName { get; private set; }
        public FSDisplayName(string displayName = "")
        {
            DisplayName = displayName;
        }
    }
}
