using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Core.Core
{
    public class ArchiveSettings
    {

        public bool EnableLocalProgressionPatches { get; set; } = true;
        public bool EnableQualityOfLifeImprovements { get; set; } = true;
        public bool EnableHudToggle { get; set; } = true;
        public bool DumpDataBlocks { get; set; } = true;
        public bool RedirectUnityDebugLogs { get; set; } = true;
        public bool DisableSteamRichPresence { get; set; } = false;
        public bool DisableGameAnalytics { get; set; } = true;
        public bool UnlockAllVanityItems { get; set; } = false;

    }
}
