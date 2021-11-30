using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Core.Core
{
    public class ArchiveSettings
    {

        public bool EnableQualityOfLifeImprovements { get; set; } = true;
        public bool EnableHudToggle { get; set; } = true;
        public bool RedirectUnityDebugLogs { get; set; } = true;

    }
}
