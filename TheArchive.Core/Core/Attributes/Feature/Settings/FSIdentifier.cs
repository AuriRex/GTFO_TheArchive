using System;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FSIdentifier : Attribute
    {
        public string Identifier { get; private set; }
        public FSIdentifier(string identifier)
        {
            Identifier = identifier;
        }
    }
}
