using System;
using TheArchive.Core.Localization;
using TheArchive.Core.Models;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FSHeader : Localized
    {
        public string Title
        {
            get
            {
                if (LocalizationCoreService.GetForAttribute(ID, out var text))
                {
                    return text;
                }
                return _title;
            }
            private set
            {
                _title = value;
            }
        }
        private string _title;
        public SColor Color { get; private set; }
        public bool Bold { get; private set; } = true;

        public FSHeader(string title, Language language = Language.English, bool bold = true) : base(title, FSTarget.FSHeader, language)
        {
            _title = title;
            Color = SColor.DARK_ORANGE.WithAlpha(0.8f);
            Bold = bold;
        }

        public FSHeader(string title, SColor color, Language language = Language.English, bool bold = true) : base(title, FSTarget.FSHeader, language)
        {
            _title = title;
            Color = color;
            Bold = bold;
        }
    }
}
