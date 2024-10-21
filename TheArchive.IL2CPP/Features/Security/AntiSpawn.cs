using Enemies;
using Il2CppInterop.Runtime.Runtime;
using SNetwork;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;

namespace TheArchive.Features.Security
{
    [EnableFeatureByDefault]
    [RundownConstraint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
    public class AntiSpawn : Feature
    {
        public override string Name => "Anti Spawn";

        public override FeatureGroup Group => FeatureGroups.Security;

        public override string Description => "Prevents clients from spawning in enemies.";

        public new static IArchiveLogger FeatureLogger { get; set; }

        public static bool IsEnabled { get; set; }

        [FeatureConfig]
        public static AntiSpawnSettings Settings { get; set; }

        public class AntiSpawnSettings
        {
            [FSDisplayName("Punish Friends")]
            [FSDescription("If (Steam) Friends should be affected as well.")]
            public bool PunishFriends { get; set; } = false;

            [FSDisplayName("Punishment")]
            [FSDescription("What to do with griefers that are trying to spawn in enemies.")]
            public PunishmentMode Punishment { get; set; } = PunishmentMode.Kick;

            [Localized]
            public enum PunishmentMode
            {
                NoneAndLog,
                Kick,
                KickAndBan
            }
        }

        public override void OnEnable()
        {
            OneTimePatch();
        }

        private static bool _hasBeenPatched = false;
        private static unsafe void OneTimePatch()
        {
            if (_hasBeenPatched)
                return;

            LoaderWrapper.ApplyNativeHook<SNet_ReplicationManager<pEnemySpawnData, EnemyReplicator>, Original_InternalSpawnRequestFromSlaveCallback>(nameof(SNet_ReplicationManager<pEnemySpawnData, EnemyReplicator>.InternalSpawnRequestFromSlaveCallback), 
                typeof(void).FullName, new string[] { typeof(pEnemySpawnData).FullName }, _detourMethod_InternalSpawnRequestFromSlaveCallback_pEnemySpawnData, out _originalMethod_InternalSpawnRequestFromSlaveCallback_pEnemySpawnData);
            LoaderWrapper.ApplyNativeHook<SNet_ReplicationManager<pEnemyGroupSpawnData, SNet_DynamicReplicator<pEnemyGroupSpawnData>>, Original_InternalSpawnRequestFromSlaveCallback>(nameof(SNet_ReplicationManager<pEnemyGroupSpawnData, SNet_DynamicReplicator<pEnemyGroupSpawnData>>.InternalSpawnRequestFromSlaveCallback),
    typeof(void).FullName, new string[] { typeof(pEnemyGroupSpawnData).FullName }, _detourMethod_InternalSpawnRequestFromSlaveCallback_pEnemyGroupSpawnData, out _originalMethod_InternalSpawnRequestFromSlaveCallback_pEnemyGroupSpawnData);
            _hasBeenPatched = true;
        }


        private static Original_InternalSpawnRequestFromSlaveCallback _originalMethod_InternalSpawnRequestFromSlaveCallback_pEnemySpawnData;
        // cache delegate to fix crash (A callback was made on a garbage collected delegate of type)
        private unsafe static Original_InternalSpawnRequestFromSlaveCallback _detourMethod_InternalSpawnRequestFromSlaveCallback_pEnemySpawnData = InternalSpawnRequestFromSlaveCallback_pEnemySpawnData_Replacement;
        private static Original_InternalSpawnRequestFromSlaveCallback _originalMethod_InternalSpawnRequestFromSlaveCallback_pEnemyGroupSpawnData;
        private unsafe static Original_InternalSpawnRequestFromSlaveCallback _detourMethod_InternalSpawnRequestFromSlaveCallback_pEnemyGroupSpawnData = InternalSpawnRequestFromSlaveCallback_pEnemyGroupSpawnData_Replacement;
        public unsafe delegate void Original_InternalSpawnRequestFromSlaveCallback(IntPtr self, IntPtr spawnData, Il2CppMethodInfo* methodInfo);
        public unsafe static void InternalSpawnRequestFromSlaveCallback_pEnemyGroupSpawnData_Replacement(IntPtr self, IntPtr spawnData, Il2CppMethodInfo* methodInfo)
        {
            if (IsEnabled && SNet.IsMaster && !SNet.Capture.IsCheckpointRecall)
            {
                bool cancelSpawn = true;

                if (SNet.Replication.TryGetLastSender(out var sender))
                {
                    cancelSpawn = PunishPlayer(sender);
                }

                if (cancelSpawn)
                {
                    FeatureLogger.Fail("Cancelled enemy spawn!");
                    return;
                }
            }

            _originalMethod_InternalSpawnRequestFromSlaveCallback_pEnemyGroupSpawnData.Invoke(self, spawnData, methodInfo);
        }
        public unsafe static void InternalSpawnRequestFromSlaveCallback_pEnemySpawnData_Replacement(IntPtr self, IntPtr spawnData, Il2CppMethodInfo* methodInfo)
        {
            if (IsEnabled && SNet.IsMaster && !SNet.Capture.IsCheckpointRecall)
            {
                bool cancelSpawn = true;

                if (SNet.Replication.TryGetLastSender(out var sender))
                {
                    cancelSpawn = PunishPlayer(sender);
                }

                if (cancelSpawn)
                {
                    FeatureLogger.Fail("Cancelled enemy spawn!");
                    return;
                }
            }

            _originalMethod_InternalSpawnRequestFromSlaveCallback_pEnemySpawnData.Invoke(self, spawnData, methodInfo);
        }

        public static bool PunishPlayer(SNet_Player player)
        {
            if (player == null)
                return true;

            if (player.IsFriend() && !Settings.PunishFriends)
            {
                FeatureLogger.Notice($"Friend \"{player.NickName}\" is spawning something in!");
                return false;
            }

            switch (Settings.Punishment)
            {
                case AntiSpawnSettings.PunishmentMode.KickAndBan:
                    PlayerLobbyManagement.BanPlayer(player);
                    goto default;
                case AntiSpawnSettings.PunishmentMode.Kick:
                    PlayerLobbyManagement.KickPlayer(player);
                    goto default;
                default:
                case AntiSpawnSettings.PunishmentMode.NoneAndLog:
                    FeatureLogger.Notice($"Player \"{player.NickName}\" tried to spawn something! ({Settings.Punishment})");
                    return true;
            }
        }
    }
}
