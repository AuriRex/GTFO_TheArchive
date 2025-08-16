using System;
using System.IO;
using LevelGeneration;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;

namespace TheArchive.Features.Dev;

[HideInModSettings]
[EnableFeatureByDefault]
public class SeedDebuggerNicknamePathFix : Feature
{
    public override string Name => "SeedDebugger-Nickname-Path-Fix";

    public override FeatureGroup Group => FeatureGroups.Dev;

    public override string Description => "Makes sure GenerationChecksumFails and other SeedDebugger related log files properly get saved even if the local player has invalid path characters in their name.";
	
    public new static IArchiveLogger FeatureLogger { get; set; }
    
    
    [ArchivePatch(typeof(LG_SeedDebugPrinter), nameof(LG_SeedDebugPrinter.WriteLogToDisk))]
    internal static class LG_SeedDebugPrinter__WriteLogToDisk__Patch
    {
	    public static void Prefix(ref string filename)
	    {
		    try
		    {
			    foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars())
			    {
				    if (!filename.Contains(invalidFileNameChar))
					    continue;
				    
				    filename = filename.Replace(invalidFileNameChar, '_');
			    }
		    }
		    catch (Exception ex)
		    {
			    FeatureLogger.Warning("This should not happen.");
			    FeatureLogger.Exception(ex);
		    }
	    }
    }
}