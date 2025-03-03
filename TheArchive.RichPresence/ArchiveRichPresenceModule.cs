using System;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;

namespace TheArchive;

[ArchiveModule(GUID, MOD_NAME, VERSION)]
public class ArchiveRichPresenceModule : IArchiveModule
{
    public const string GUID = $"{ArchiveMod.GUID}.RichPresence";
    public const string MOD_NAME = $"{ArchiveMod.MOD_NAME}.RichPresence";
    public const string VERSION = "0.0.1";

    private IArchiveLogger _logger = LoaderWrapper.CreateLoggerInstance(MOD_NAME);
    
    public bool ApplyHarmonyPatches => false;
    public string ModuleGroup => ArchiveMod.ARCHIVE_CORE_FEATUREGROUP;
    
    public void Init()
    {
        ArchiveMod.GameDataInitialized += OnGameDataInitialized;
    }

    private void OnGameDataInitialized(Utils.RundownID obj)
    {
        try
        {
            typeof(PresenceManager).RegisterAllPresenceFormatProviders(false);
            PresenceFormatter.Setup();
        }
        catch (Exception ex)
        {
            _logger.Exception(ex);
        }
    }

    public void OnLateUpdate()
    {
        
    }
}