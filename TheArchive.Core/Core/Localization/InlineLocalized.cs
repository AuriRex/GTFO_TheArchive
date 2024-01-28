using System;

namespace TheArchive.Core.Localization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Enum, Inherited = true)]
    public class InlineLocalized : Attribute
    {
    }
}
