using System;
using System.Reflection;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Localization;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;

[assembly: AssemblyVersion(TheArchive.ArchiveRichPresenceModule.VERSION)]
[assembly: AssemblyFileVersion(TheArchive.ArchiveRichPresenceModule.VERSION)]
[assembly: AssemblyInformationalVersion(TheArchive.ArchiveRichPresenceModule.VERSION)]

namespace TheArchive;

[ArchiveModule(GUID, MOD_NAME, VERSION)]
public class ArchiveRichPresenceModule : IArchiveModule
{
    public const string GUID = $"{ArchiveMod.GUID}.RichPresence";
    public const string MOD_NAME = ManifestInfo.TSName;
    public const string VERSION = ManifestInfo.TSVersion;

    private IArchiveLogger _logger = LoaderWrapper.CreateLoggerInstance(MOD_NAME);
    
    public string ModuleGroup => ArchiveMod.ARCHIVE_CORE_FEATUREGROUP;

    public ILocalizationService LocalizationService { get; set; }
    public IArchiveLogger Logger { get; set; }

    public void Init()
    {
        ArchiveMod.GameDataInitialized += OnGameDataInitialized;

        try
        {
            var discordrpcLicense =
                System.Text.Encoding.UTF8.GetString(Utils.GetResource(Assembly.GetExecutingAssembly(),
                    "TheArchive.DiscordGameSDK.LICENSE_DiscordRPC"));
            Attribution.Add(new Attribution.AttributionInfo("DiscordRPC License", discordrpcLicense)
            {
                Comment = "<#fff18e>Huge shoutouts to <#88e2e9>Lachee</color> for her re-implementation of the discord_game_sdks functions in C#!</color>",
                Origin = MOD_NAME.Replace("_", "."),
            });
        }
        catch (Exception ex)
        {
            _logger.Error("Something went wrong while trying to add Attribution.");
            _logger.Exception(ex);
        }
    }

    private void OnGameDataInitialized(Utils.RundownID _)
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
}