using System;
using TheArchive.Core.Models;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FSHeader : Attribute
    {
        public string Title { get; private set; }
        public SColor Color { get; private set; }
        public bool Bold { get; private set; } = true;

        public FSHeader(string title, bool bold = true)
        {
            Title = title;
            Color = SColor.DARK_ORANGE.WithAlpha(0.8f);
            Bold = bold;
        }

        public FSHeader(string title, SColor color, bool bold = true)
        {
            Title = title;
            Color = color;
            Bold = bold;
        }
    }
}
