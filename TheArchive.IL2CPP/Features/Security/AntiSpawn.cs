using Enemies;
using SNetwork;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;

namespace TheArchive.Features.Security
{
#if BepInEx
    [ForceDisable] // LoaderWrapper.NativeHookAttach isn't implemented for BIE yet
#endif
    [EnableFeatureByDefault]
    public class AntiSpawn : Feature
    {
        public override string Name => "Anti Spawn";

        public override string Group => FeatureGroups.Security;

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

#warning TODO: Refactor after ML upgrade to Il2CppInterop
        private static unsafe TDelegate ApplyNativePatch<TToPatch, TDelegate>(MethodInfo patchMethod, string originalMethodName, Type[] parameterTypes = null) where TDelegate : Delegate
        {
            var ptr = GetMethodPointerFromGenericType<TToPatch>(originalMethodName, parameterTypes);

            var patch = patchMethod.MethodHandle.GetFunctionPointer();

            FeatureLogger.Debug($"Attaching Native Patch ...");
            FeatureLogger.Debug($"Info: {nameof(GetMethodPointerFromGenericType)}: {typeof(TToPatch).FullName} | {originalMethodName} (Ptr:{ptr})");

            //calls MelonUtils.NativeHookAttach((IntPtr)(&ptr), patch);
            LoaderWrapper.NativeHookAttach((IntPtr)(&ptr), patch);

            var del = (TDelegate)Marshal.GetDelegateForFunctionPointer(ptr, typeof(TDelegate));

            FeatureLogger.Debug($"OG Ptr: {ptr}");
            FeatureLogger.Debug($"Patch Ptr: {patch}");
            FeatureLogger.Debug($"Delegate Ptr: {del.Method.MethodHandle.GetFunctionPointer()}");

            return del;
        }

        private static unsafe IntPtr GetMethodPointerFromGenericType<T>(string originalMethodName, Type[] parameterTypes = null)
        {
            var constructedGenericType = typeof(T);

            MethodInfo originalMethodInfo;

            if (parameterTypes != null)
            {
                originalMethodInfo = constructedGenericType
                    .GetMethod(originalMethodName, Utils.AnyBindingFlagss, null, parameterTypes, null);
            }
            else
            {
                originalMethodInfo = constructedGenericType
                    .GetMethod(originalMethodName, Utils.AnyBindingFlagss);
            }

#if Unhollower
            var field = UnhollowerBaseLib.UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(originalMethodInfo);
#elif Il2CppInterop
            var field = Il2CppInterop.Common.Il2CppInteropUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(originalMethodInfo);
#else
            FieldInfo field = null;
            throw new NotImplementedException();
#endif

            // does not work at all if one does not resolve the field from the generic type
            field = constructedGenericType.GetField(field.Name, Utils.AnyBindingFlagss);

            var ptr = *(IntPtr*)(IntPtr)field.GetValue(null);

            return ptr;
        }

        private static bool _hasBeenPatched = false;
        private static unsafe void OneTimePatch()
        {
            if (_hasBeenPatched)
                return;

            //_originalMethod_InternalSpawnRequestFromSlaveCallback = ApplyNativePatch<SNet_ReplicationManager<pEnemySpawnData, EnemyReplicator>, Original_InternalSpawnRequestFromSlaveCallback>(typeof(AntiSpawn).GetMethod(nameof(InternalSpawnRequestFromSlaveCallback_Replacement)), "InternalSpawnRequestFromSlaveCallback");

            _originalMethod_InternalSpawnCallback = ApplyNativePatch<SNet_ReplicationManager<pEnemySpawnData, EnemyReplicator>, Original_InternalSpawnCallback>(typeof(AntiSpawn).GetMethod(nameof(InternalSpawnCallback_Replacement)), "InternalSpawnCallback");
            _originalMethod_Spawn = ApplyNativePatch<SNet_ReplicationManager<pEnemySpawnData, EnemyReplicator>, Original_Spawn>(typeof(AntiSpawn).GetMethod(nameof(Spawn_Replacement)), "Spawn", new Type[] { typeof(pEnemySpawnData) });

            _hasBeenPatched = true;
        }

        /*
        private static Original_InternalSpawnRequestFromSlaveCallback _originalMethod_InternalSpawnRequestFromSlaveCallback;
        public delegate void Original_InternalSpawnRequestFromSlaveCallback(IntPtr self, IntPtr spawnData);
        public static void InternalSpawnRequestFromSlaveCallback_Replacement(IntPtr self, IntPtr spawnData)
        {
            var lastSenderIsMaster = SNet.Replication.IsLastSenderMaster();

            if (IsEnabled)
            {
                FeatureLogger.Fail($"Intercepted enemy spawn request packet from client! lastSenderIsMaster: {lastSenderIsMaster}");
            }
            else
            {
                _originalMethod_InternalSpawnRequestFromSlaveCallback.Invoke(self, spawnData);
            }
        }
        */

        internal static bool isLocallySpawned = false;
        private static Original_Spawn _originalMethod_Spawn;
        public delegate void Original_Spawn(IntPtr type, IntPtr self, IntPtr spawnData);
        public static void Spawn_Replacement(IntPtr type, IntPtr self, IntPtr spawnData)
        {
            isLocallySpawned = true;
            _originalMethod_Spawn.Invoke(type, self, spawnData);
            isLocallySpawned = false;
        }

        private static Original_InternalSpawnCallback _originalMethod_InternalSpawnCallback;
        public delegate void Original_InternalSpawnCallback(IntPtr type, IntPtr self, IntPtr spawnData);
        public static void InternalSpawnCallback_Replacement(IntPtr type, IntPtr self, IntPtr spawnData)
        {
            if (IsEnabled && SNet.IsMaster && !isLocallySpawned)
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

            _originalMethod_InternalSpawnCallback.Invoke(type, self, spawnData);
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
