using System;
using TheArchive.Core.Localization;
using TheArchive.Core.Models;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FSHeader : Localized
    {
        public string Title => UntranslatedText;
        public SColor Color { get; private set; }
        public bool Bold { get; private set; } = true;

        public FSHeader(string title, bool bold = true) : base(title)
        {
            Color = SColor.DARK_ORANGE.WithAlpha(0.8f);
            Bold = bold;
        }

        public FSHeader(string title, SColor color, bool bold = true) : base(title)
        {
            Color = color;
            Bold = bold;
        }
    }
}
