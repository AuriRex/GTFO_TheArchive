using CellMenu;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Managers;
using TheArchive.Core.Models;
using TheArchive.Core.Settings;
using TheArchive.Interfaces;

namespace TheArchive.Features.Presence;

[EnableFeatureByDefault]
public class DiscordRichPresence : Feature
{
    public override string Name => "Archive Discord Rich Presence";

    public override FeatureGroup Group => FeatureGroups.Presence;

    public override string Description => "Show the current game state in detail on discord.";

    public new static IArchiveLogger FeatureLogger { get; set; }
    
    public override bool InlineSettingsIntoParentMenu => true;

    public override bool SkipInitialOnEnable => true;

    public override Type[] LocalizationExternalTypes => new[] { typeof(RichPresenceSettings) };

    [FeatureConfig]
    public static RichPresenceSettings DiscordRPCSettings { get; set; }

    public override void Init()
    {
        DiscordRPCSettings = DiscordRPCSettings?.FillDefaultDictValues();

        PresenceManager.UpdateGameState(PresenceGameState.Startup, false);
    }

    public override void OnEnable()
    {
        if (!DataBlocksReady)
            return;

        if (ArchiveDiscordManager.IsEnabled)
            return;
        
        try
        {
            ArchiveDiscordManager.RenewPartyGuid();
            ArchiveDiscordManager.OnActivityJoin += DiscordManager_OnActivityJoin;
            ArchiveDiscordManager.Enable(DiscordRPCSettings);
        }
        catch (Exception ex)
        {
            FeatureLogger.Exception(ex);
        }
    }

    public void OnGameStateChanged(eGameStateName state)
    {
        if (state == eGameStateName.NoLobby)
        {
            FeatureLogger.Debug($"Creating a new Party GUID.");
            ArchiveDiscordManager.RenewPartyGuid();
        }
    }

    public override void OnDatablocksReady()
    {
        OnEnable();
    }

    private static void DiscordManager_OnActivityJoin(string secret)
    {
        if (!ulong.TryParse(secret, out var value)) return;
        if (value == 0) return;

        CM_Utils.JoinLobby(value, new Action(() => FeatureLogger.Success($"Successfully joined lobby \"{secret}\"!")), new Action(() => FeatureLogger.Fail($"Failed to join lobby \"{secret}\".")));
    }

    public override void OnDisable()
    {
        if (!ArchiveDiscordManager.IsEnabled)
            return;
        
        try
        {
            ArchiveDiscordManager.OnActivityJoin -= DiscordManager_OnActivityJoin;
            ArchiveDiscordManager.Disable();
        }
        catch (Exception ex)
        {
            FeatureLogger.Exception(ex);
        }
    }

    public override void Update()
    {
        ArchiveDiscordManager.Update();
    }
}