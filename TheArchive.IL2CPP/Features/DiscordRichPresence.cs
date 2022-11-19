using System;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Managers;
using TheArchive.Core.Models;
using TheArchive.Core.Settings;
using TheArchive.Utilities;

namespace TheArchive.Features
{
    [EnableFeatureByDefault]
    public class DiscordRichPresence : Feature
    {
        public override string Name => "Discord Rich Presence";

        public override string Description => "Show the current game state in detail on discord.";

        [FeatureConfig]
        public static RichPresenceSettings DiscordRPCSettings { get; set; }

        public override void Init()
        {
            DiscordRPCSettings = DiscordRPCSettings?.FillDefaultDictValues();

            PresenceManager.UpdateGameState(PresenceGameState.Startup, false);
        }

        public override void OnEnable()
        {
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

        private void DiscordManager_OnActivityJoin(string secret)
        {
            ulong.TryParse(secret, out ulong value);
            if (value == 0) return;

            SNetwork.SNet.Lobbies.JoinLobby(value);
        }

        public override void OnDisable()
        {
            DiscordManager.OnActivityJoin -= DiscordManager_OnActivityJoin;
            DiscordManager.Disable();
        }

        public void Update()
        {
            DiscordManager.Update();
        }

        public override void OnQuit()
        {
            DiscordManager.Disable();
        }
    }
}
