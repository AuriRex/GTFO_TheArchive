using CellMenu;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Managers;
using TheArchive.Core.Models;
using TheArchive.Core.Settings;
using TheArchive.Utilities;

namespace TheArchive.Features.Presence
{
    [EnableFeatureByDefault]
    public class DiscordRichPresence : Feature
    {
        public override string Name => "Archive Discord Rich Presence";

        public override FeatureGroup Group => FeatureGroups.Presence;

        public override string Description => "Show the current game state in detail on discord.";

        public override bool InlineSettingsIntoParentMenu => true;

        public override bool SkipInitialOnEnable => true;

        [FeatureConfig]
        public static RichPresenceSettings DiscordRPCSettings { get; set; }

        public override void Init()
        {
            DiscordRPCSettings = DiscordRPCSettings?.FillDefaultDictValues();

            PresenceManager.UpdateGameState(PresenceGameState.Startup, false);
        }

        private static bool _wasDisabledForR8 = false;

        public override void OnEnable()
        {
            if (Is.R8OrLater && !_wasDisabledForR8 && !DataBlocksReady && DiscordRPCSettings.DisableOnRundownEight)
            {
                FeatureLogger.Notice("Disabling Archive Rich Presence for Rundown 8!");
                FeatureManager.Instance.DisableFeature(this, setConfig: false);
                _wasDisabledForR8 = true; // Only do it once, user might re-enable it
                return;
            }

            try
            {
                ArchiveDiscordManager.OnActivityJoin += DiscordManager_OnActivityJoin;
                ArchiveDiscordManager.Enable(DiscordRPCSettings);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }
        }

        public override void OnGameDataInitialized()
        {
            OnEnable();
        }

        private void DiscordManager_OnActivityJoin(string secret)
        {
            ulong.TryParse(secret, out ulong value);
            if (value == 0) return;

            CM_Utils.JoinLobby(value, new Action(() => FeatureLogger.Success($"Successfully joined lobby \"{secret}\"!")), new Action(() => FeatureLogger.Fail($"Failed to join lobby \"{secret}\".")));
        }

        public override void OnDisable()
        {
            ArchiveDiscordManager.OnActivityJoin -= DiscordManager_OnActivityJoin;
            ArchiveDiscordManager.Disable();
        }

        public override void Update()
        {
            ArchiveDiscordManager.Update();
        }
    }
}
