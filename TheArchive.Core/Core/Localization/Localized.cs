using System;

namespace TheArchive.Core.Localization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Enum, Inherited = true)]
public class Localized : Attribute
{
    public string UntranslatedText { get; }

    public Localized(string text)
    {
        UntranslatedText = text;
    }
    public Localized() { }
}