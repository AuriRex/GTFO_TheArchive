using System.Collections.Generic;

namespace TheArchive.Core.Settings
{
    public class EnabledFeatures
    {
        public Dictionary<string, bool> Features { get; set; } = new Dictionary<string, bool>();
    }
}
