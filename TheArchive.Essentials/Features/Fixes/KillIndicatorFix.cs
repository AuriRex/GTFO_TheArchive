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
using TheArchive.Loader;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Fixes;

[EnableFeatureByDefault]
[RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
internal class KillIndicatorFix : Feature
{
    public const string KILL_INDICATOR_FIX_GUID = "randomuserhi.KillIndicatorFix";
    public const string DAMAGE_SYNC_GUID = "randomuserhi.DamageSync";

    public override bool ShouldInit()
    {
        if (LoaderWrapper.IsModInstalled(KILL_INDICATOR_FIX_GUID))
        {
            RequestDisable("Kill Indicator Fix is installed, disabling this copy.");
            return false;
        }
        
        return true;
    }
    
    public override string Name => "Kill Indicator Fix";

    public override FeatureGroup Group => FeatureGroups.Fixes;

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

        [FSHide]
        [FSDescription("Prints debug info to console.")]
        public bool DebugLog { get; set; } = false;
    }

    private static bool hasDamageSync = false;

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
    private static Dictionary<int, Tag> taggedEnemies = new Dictionary<int, Tag>();

    public override void Init()
    {
        hasDamageSync = LoaderWrapper.IsModInstalled(DAMAGE_SYNC_GUID);
        if (hasDamageSync) FeatureLogger.Notice("Damage Sync is installed, disabling damage sync component.");

        RundownManager.add_OnExpeditionGameplayStarted((Action)OnRundownStart);
    }

    private void OnRundownStart()
    {
        taggedEnemies.Clear();
    }

#if IL2CPP

#region Fix for local player 

    [ArchivePatch(typeof(Dam_EnemyDamageBase), nameof(Dam_EnemyDamageBase.ProcessReceivedDamage))]
    internal static class Dam_EnemyDamageBase_ProcessReceivedDamage_Patch
    {
        public static void Postfix(Dam_EnemyDamageBase __instance)
        {
            // Only run if TS DamageSync is not installed (prevents sending duplicate packets)
            if (!SNet.IsMaster || hasDamageSync) return;

            var data = default(pSetHealthData);
            data.health.Set(__instance.Health, __instance.HealthMax);
            __instance.m_setHealthPacket.Send(data, SNet_ChannelType.GameReceiveCritical);
        }
    }

    [ArchivePatch(typeof(EnemyBehaviour), nameof(EnemyBehaviour.ChangeState), new Type[] 
    { 
        typeof(EB_States)
    })]
    internal static class EnemyBehaviour_ChangeState_Patch
    {
        public static void Prefix(EnemyBehaviour __instance, EB_States state)
        {
            if (SNet.IsMaster) return;
            if (__instance.m_currentStateName == state || state != EB_States.Dead) return;

            try {
                EnemyAgent owner = __instance.m_ai.m_enemyAgent;
                int instanceID = owner.GetInstanceID();
                long now = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();

                if (taggedEnemies.ContainsKey(instanceID)) {
                    Tag t = taggedEnemies[instanceID];

                    if (Settings.DebugLog)
                        if (t.timestamp <= now)
                            FeatureLogger.Info($"Received kill update {now - t.timestamp} milliseconds after tag.");
                        else
                            FeatureLogger.Info($"Received kill update for enemy that was tagged in the future? Possibly long overflow...");

                    if (t.timestamp <= now && now - t.timestamp < Settings.TagBufferPeriod) {
                        if (!owner.Damage.DeathIndicatorShown) {
                            FeatureLogger.Info($"Client side marker was not shown, showing server side one.");

                            GuiManager.CrosshairLayer?.ShowDeathIndicator(owner.transform.position + t.localHitPosition);
                            owner.Damage.DeathIndicatorShown = true;
                        } else if (Settings.DebugLog) {
                            FeatureLogger.Info($"Client side marker was shown, not showing server side one.");
                        }
                    } else if (Settings.DebugLog) {
                        FeatureLogger.Info($"Client was no longer interested in this enemy, marker will not be shown.");
                    }

                    taggedEnemies.Remove(instanceID);
                }
            } catch (Exception e) { FeatureLogger.Error($"Something went wrong:\n{e}"); }
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
    }, Priority = HarmonyLib.Priority.Last)]
    internal static class Dam_EnemyDamageBase_BulletDamage_Patch
    {
        public static void Prefix(Dam_EnemyDamageBase __instance, float dam, Agent sourceAgent, Vector3 position)
        {
            if (SNet.IsMaster) return;
            PlayerAgent p = sourceAgent?.TryCast<PlayerAgent>();
            if (p == null) // Check damage was done by a player
            {
                if (Settings.DebugLog) FeatureLogger.Notice($"Could not find PlayerAgent.");
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
            PlayerAgent p = sourceAgent?.TryCast<PlayerAgent>();
            if (p == null) // Check damage was done by a player
            {
                if (Settings.DebugLog) FeatureLogger.Notice($"Could not find PlayerAgent.");
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

#endregion

#endif

#if MONO
        // TODO
#endif
}