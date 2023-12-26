using System;

namespace TheArchive.Core.Localization
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class Localized : Attribute
    {
        public string TranslatedText { get; }
        public Language Language { get; }
        public FSTarget Target { get; }
        public uint ID { get; internal set; }

        public Localized(string text, FSTarget target, Language language)
        {
            Language = language;
            TranslatedText = text;
            Target = target;
            ID = 0;
        }

        public Localized(uint id, FSTarget target)
        {
            Target = target;
            ID = id;
        }
    }

    public enum FSTarget
    {
        FSDisplayName,
        FSDescription,
        FSHeader
    }
}
