using System;
using System.Collections.Generic;
using System.IO;
using TheArchive.Core.Managers;
using TheArchive.Loader;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core;

/// <summary>
/// Contains information about different game builds.
/// </summary>
public static class BuildDB
{
    private static int _buildNumber = -1;
    /// <summary>
    /// The current games build number
    /// </summary>
    public static int BuildNumber
    {
        get
        {
            if (_buildNumber != -1)
                return _buildNumber;
            
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
            return _buildNumber;
        }
    }

    /// <summary> The last game build/revision number for each rundown </summary>
    public static readonly Dictionary<int, RundownID> RundownIDMapping = new()
    {
        { 19715, RundownID.RundownOne },
        { 20472, RundownID.RundownTwo },
        { 20869, RundownID.RundownThree },
        { 21989, RundownID.RundownFour },
        { 25829, RundownID.RundownFive },
        { 29742, RundownID.RundownSix },
        { 31994, RundownID.RundownSeven },
        { 32283, RundownID.RundownAltOne },
        { 32416, RundownID.RundownAltTwo },
        { 32577, RundownID.RundownAltThree },
        { 32823, RundownID.RundownAltFour },
        { 33054, RundownID.RundownAltFive },
        { 34156, RundownID.RundownAltSix },
    };

    /// <summary>
    /// Get the latest <c>RundownID</c> based on the provided build number.
    /// </summary>
    /// <param name="buildNumber">Build number</param>
    /// <returns>Latest RundownID for the provided build number</returns>
    public static RundownID GetCurrentRundownID(int buildNumber)
    {
        foreach(var kvp in RundownIDMapping)
        {
            if(buildNumber <= kvp.Key)
            {
                return kvp.Value;
            }
        }

        return GetLatestRundownID();
    }
}