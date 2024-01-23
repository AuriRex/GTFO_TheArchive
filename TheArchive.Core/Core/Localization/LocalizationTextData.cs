using System.Collections.Generic;

namespace TheArchive.Core.Localization
{
    public class LocalizationTextData
    {
        public string UntranslatedText { get; set; }

        public string Description { get; set; }

        public uint ID { get; set; }

        public Dictionary<Language, string> Languages { get; set; }
    }
}