using System;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FSDescription : Attribute
    {
        public string Description { get; private set; }
        public FSDescription(string description)
        {
            Description = description;
        }
    }
}
