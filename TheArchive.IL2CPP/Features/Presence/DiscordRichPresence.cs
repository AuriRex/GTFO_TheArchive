using CellMenu;
using GameData;
using System;
using TheArchive.Core;
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
        public override string Name => "Discord Rich Presence";

        public override string Group => FeatureGroups.Presence;

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

        public override void OnEnable()
        {
            if (DiscordRPCSettings.DoRundown8DisableCheck && Is.R6OrLater && LikelyIsOnR8())
            {
                FeatureLogger.Notice("Disabling Rich Presence for Rundown 8 for now!");
                DiscordRPCSettings.DoRundown8DisableCheck = false;
                MarkSettingsAsDirty(DiscordRPCSettings);
                FeatureManager.Instance.DisableFeature(this);
                return;
            }

            try
            {
                DiscordManager.OnActivityJoin += DiscordManager_OnActivityJoin;
                DiscordManager.Enable(DiscordRPCSettings);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }
        }

        private bool LikelyIsOnR8()
        {
#if IL2CPP
            var setupDB = GameSetupDataBlock.GetBlock(1);

            return BuildDB.BuildNumber > 33871 && setupDB.RundownIdsToLoad.Count > 7;
#else
            return false;
#endif
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
            DiscordManager.OnActivityJoin -= DiscordManager_OnActivityJoin;
            DiscordManager.Disable();
        }

        public override void Update()
        {
            DiscordManager.Update();
        }
    }
}
