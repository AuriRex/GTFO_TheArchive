using System.Collections.Generic;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core
{
    public static class BuildDB
    {
        // The last game build/revision number for each rundown
        public static Dictionary<int, RundownID> rundownIDMapping = new Dictionary<int, RundownID>()
        {
            { 19715, RundownID.RundownOne },
            { 20472, RundownID.RundownTwo },
            { 20869, RundownID.RundownThree },
            { 21989, RundownID.RundownFour },
            { 25829, RundownID.RundownFive },
            { 29742, RundownID.RundownSix },
        };

        public static RundownID GetCurrentRundownID(int buildNumber)
        {
            foreach(var kvp in rundownIDMapping)
            {
                if(buildNumber <= kvp.Key)
                {
                    return kvp.Value;
                }
            }

            return RundownID.RundownUnknown;
        }

    }
}
