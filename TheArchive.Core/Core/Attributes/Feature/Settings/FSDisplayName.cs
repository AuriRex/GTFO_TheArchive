using System;
using TheArchive.Core.Localization;

namespace TheArchive.Core.Attributes.Feature.Settings;

[AttributeUsage(AttributeTargets.Property)]
public class FSDisplayName : Localized
{
    public string DisplayName => UntranslatedText;
    public FSDisplayName(string displayName) : base(displayName)
    {
    }
}