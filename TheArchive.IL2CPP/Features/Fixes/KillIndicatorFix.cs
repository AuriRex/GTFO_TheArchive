using Agents;
using Enemies;
using Player;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Fixes
{
    [EnableFeatureByDefault]
    [RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
    internal class KillIndicatorFix : Feature
    {
        public override string Name => "Kill Indicator Fix";

        public override string Group => FeatureGroups.Fixes;

        public override string Description => "Fixes orange kill indicators not being consistent for clients.";

        public override bool InlineSettingsIntoParentMenu => true;

        public static new IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static KillIndicatorFixSettings Settings { get; set; }

        public class KillIndicatorFixSettings
        {
            [FSDisplayName("Tag Buffer Period")]
            [FSDescription("Determines how long (in ms) an enemy is tracked for after getting shot at.")]
            public int TagBufferPeriod { get; set; } = 1000;

            [FSDisplayName("Marker Life Time")]
            [FSDescription("Determines how long (in ms) a shown kill marker is tracked for to prevent duplicates.")]
            public int MarkerLifeTime { get; set; } = 3000;

            [FSHide]
            [FSDescription("Prints debug info to console.")]
            public bool DebugLog { get; set; } = false;
        }

        private struct Tag
        {
            public long timestamp;
            public Vector3 localHitPosition; // Store local position to prevent desync when enemy moves since hit position is relative to world not enemy.

            public Tag(long timestamp, Vector3 localHitPosition)
            {
                this.timestamp = timestamp;
                this.localHitPosition = localHitPosition;
            }
        }

        private static Dictionary<ushort, Tag> taggedEnemies = new Dictionary<ushort, Tag>();
        private static Dictionary<ushort, long> markers = new Dictionary<ushort, long>();

        public override void Init()
        {
            RundownManager.add_OnExpeditionGameplayStarted((Action)OnRundownStart);
        }

        private void OnRundownStart()
        {
            markers.Clear();
            taggedEnemies.Clear();
        }

#if IL2CPP
        [ArchivePatch(typeof(Dam_EnemyDamageBase), nameof(Dam_EnemyDamageBase.ProcessReceivedDamage))]
        internal static class Dam_EnemyDamageBase_ProcessReceivedDamage_Patch
        {
            public static void Postfix(Dam_EnemyDamageBase __instance)
            {
                if (!SNet.IsMaster) return;

                var data = default(pSetHealthData);
                data.health.Set(__instance.Health, __instance.HealthMax);
                __instance.m_setHealthPacket.Send(data, SNet_ChannelType.GameReceiveCritical);
            }
        }

        [ArchivePatch(typeof(EnemyAppearance), nameof(EnemyAppearance.OnDead))]
        internal static class EnemyAppearance_OnDead_Patch
        {
            public static void Prefix(EnemyAppearance __instance)
            {
                if (SNet.IsMaster) return;

                EnemyAgent owner = __instance.m_owner;
                ushort id = owner.GlobalID;
                long now = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();

                if (taggedEnemies.ContainsKey(id))
                {
                    Tag t = taggedEnemies[id];

                    if (Settings.DebugLog)
                    {
                        if (t.timestamp <= now)
                        {
                            FeatureLogger.Info($"Received kill update {now - t.timestamp} milliseconds after tag.");
                        }
                        else
                        {
                            FeatureLogger.Notice($"Received kill update for enemy that was tagged in the future? Possibly long overflow...");
                        }
                    }

                    if (t.timestamp <= now && now - t.timestamp < Settings.TagBufferPeriod)
                    {
                        if (!markers.ContainsKey(id))
                        {
                            if (Settings.DebugLog) FeatureLogger.Info($"Client side marker was not shown, showing server side one.");

                            GuiManager.CrosshairLayer.ShowDeathIndicator(owner.transform.position + t.localHitPosition);
                        }
                        else
                        {
                            if (Settings.DebugLog) FeatureLogger.Info($"Client side marker was shown, not showing server side one.");

                            markers.Remove(id);
                        }
                    }
                    else if (Settings.DebugLog) FeatureLogger.Info($"Client was no longer interested in this enemy, marker will not be shown.");

                    taggedEnemies.Remove(id);
                }
            }
        }

        [ArchivePatch(typeof(Dam_EnemyDamageBase), nameof(Dam_EnemyDamageBase.BulletDamage), new Type[]
        {
            typeof(float),
            typeof(Agent),
            typeof(Vector3),
            typeof(Vector3),
            typeof(Vector3),
            typeof(bool),
            typeof(int),
            typeof(float),
            typeof(float),
            typeof(uint)
        })]
        internal static class Dam_EnemyDamageBase_BulletDamage_Patch
        {
            public static void Prefix(Dam_EnemyDamageBase __instance, float dam, Agent sourceAgent, Vector3 position)
            {
                if (SNet.IsMaster) return;
                PlayerAgent p = sourceAgent.TryCast<PlayerAgent>();
                if (p == null) // Check damage was done by a player
                {
                    if (Settings.DebugLog) FeatureLogger.Notice($"Could not find PlayerAgent, damage was done by agent of type: {sourceAgent.m_type}.");
                    return;
                }
                if (p.Owner.IsBot) return; // Check player isnt a bot

                EnemyAgent owner = __instance.Owner;
                ushort id = owner.GlobalID;
                long now = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();

                float num = AgentModifierManager.ApplyModifier(owner, AgentModifier.ProjectileResistance, Mathf.Clamp(dam, 0, __instance.HealthMax));
                __instance.Health -= num;

                Vector3 localHit = position - owner.transform.position;
                Tag t = new Tag(now, localHit);
                if (taggedEnemies.ContainsKey(id)) taggedEnemies[id] = t;
                else taggedEnemies.Add(id, t);

                if (Settings.DebugLog)
                {
                    FeatureLogger.Info($"{num} Bullet Damage done by {p.PlayerName}. IsBot: {p.Owner.IsBot}");
                    FeatureLogger.Info($"Tracked current HP: {__instance.Health}, [{id}]");
                }
            }
        }

        [ArchivePatch(typeof(Dam_EnemyDamageBase), nameof(Dam_EnemyDamageBase.MeleeDamage), new Type[]
        {
            typeof(float),
            typeof(Agent),
            typeof(Vector3),
            typeof(Vector3),
            typeof(int),
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(bool),
            typeof(DamageNoiseLevel),
            typeof(uint)
        })]
        internal static class Dam_EnemyDamageBase_MeleeDamage_Patch
        {
            public static void Prefix(Dam_EnemyDamageBase __instance, float dam, Agent sourceAgent, Vector3 position)
            {
                if (SNet.IsMaster) return;
                PlayerAgent p = sourceAgent.TryCast<PlayerAgent>();
                if (p == null) // Check damage was done by a player
                {
                    if (Settings.DebugLog) FeatureLogger.Notice($"Could not find PlayerAgent, damage was done by agent of type: {sourceAgent.m_type}.");
                    return;
                }
                if (p.Owner.IsBot) return; // Check player isnt a bot

                EnemyAgent owner = __instance.Owner;
                ushort id = owner.GlobalID;
                long now = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();

                // Apply damage modifiers (head, occiput etc...)
                float num = AgentModifierManager.ApplyModifier(owner, AgentModifier.MeleeResistance, Mathf.Clamp(dam, 0, __instance.DamageMax));
                __instance.Health -= num;

                Vector3 localHit = position - owner.transform.position;
                Tag t = new Tag(now, localHit);
                if (taggedEnemies.ContainsKey(id)) taggedEnemies[id] = t;
                else taggedEnemies.Add(id, t);

                if (Settings.DebugLog)
                {
                    FeatureLogger.Info($"Melee Damage: {num}");
                    FeatureLogger.Info($"Tracked current HP: {__instance.Health}, [{id}]");
                }
            }
        }

        [ArchivePatch(typeof(Dam_EnemyDamageLimb), nameof(Dam_EnemyDamageLimb.ShowHitIndicator))]
        internal static class Dam_EnemyDamageLimb_ShowHitIndicator_Patch
        {
            public static void Prefix(Dam_EnemyDamageLimb __instance, bool hitWeakspot, ref bool willDie, Vector3 position, bool hitArmor)
            {
                EnemyAgent owner = __instance.m_base.Owner;
                long now = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();

                // Prevents the case where client fails to receive kill confirm from host so marker persists in dictionary
                // - Auto removes the marker if it has existed for longer than MarkerLifeTime.
                ushort[] keys = markers.Keys.ToArray();
                foreach (ushort id in keys)
                {
                    if (now - markers[id] > Settings.MarkerLifeTime) markers.Remove(id);
                }

                // Only call if GuiManager.CrosshairLayer.ShowDeathIndicator(position); is going to get called (condition is taken from source)
                if (willDie && !__instance.m_base.DeathIndicatorShown)
                {
                    ushort id = owner.GlobalID;
                    if (!markers.ContainsKey(id)) markers.Add(id, now);
                    else FeatureLogger.Notice($"Marker for enemy was already shown. This should not happen.");
                }
            }
        }
#endif

#if MONO
        // TODO
#endif
    }
}
