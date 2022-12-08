using System;
using System.Collections.Generic;
using System.IO;
using TheArchive.Core.Managers;
using TheArchive.Loader;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core
{
    public static class BuildDB
    {
        private static int _buildNumber = -1;
        public static int BuildNumber
        {
            get
            {
                if (_buildNumber == -1)
                {
                    try
                    {
                        //CellBuildData.GetRevision()
                        _buildNumber = (int)((
                            ImplementationManager.FindTypeInCurrentAppDomain("CellBuildData")
                            ?.GetMethod("GetRevision", Utils.AnyBindingFlagss)
                            ?.Invoke(null, null)
                            ) ?? -1);

                        if (_buildNumber <= 0)
                        {
                            var buildNumFilePath = Path.Combine(LoaderWrapper.GameDirectory, "revision.txt");

                            if (!File.Exists(buildNumFilePath))
                            {
                                throw new Exception($"File doesn't exist: \"{buildNumFilePath}\"");
                            }

                            var buildStringRaw = File.ReadAllLines(buildNumFilePath)[0];
                            buildStringRaw = buildStringRaw.Replace(" ", ""); // remove the trailing space
                            _buildNumber = int.Parse(buildStringRaw);
                        }

                        if (_buildNumber <= 0)
                            throw new Exception("Build / Revision number couldn't be found ...");
                    }
                    catch (Exception ex)
                    {
                        _buildNumber = 0;
                        ArchiveLogger.Error($"Couldn't load the current build / revision number from CellBuildData or revisions.txt!");
                        ArchiveLogger.Exception(ex);
                    }
                }
                return _buildNumber;
            }
        }

        /// <summary> The last game build/revision number for each rundown </summary>
        public static Dictionary<int, RundownID> rundownIDMapping = new Dictionary<int, RundownID>()
        {
            { 19715, RundownID.RundownOne },
            { 20472, RundownID.RundownTwo },
            { 20869, RundownID.RundownThree },
            { 21989, RundownID.RundownFour },
            { 25829, RundownID.RundownFive },
            { 29742, RundownID.RundownSix },
            { 31994, RundownID.RundownSeven },
            { 32283, RundownID.RundownAltOne },
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

            return RundownID.Latest;
        }
    }
}
