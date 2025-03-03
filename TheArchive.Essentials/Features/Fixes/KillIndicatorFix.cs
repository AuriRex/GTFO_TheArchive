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

    private class Tag
    {
        public float health;
        public long timestamp;
        public Vector3 localHitPosition; // Store local position to prevent desync when enemy moves since hit position is relative to world not enemy.

        public Tag(float health) {
            this.health = health;
        }
    }
    private static Dictionary<int, Tag> taggedEnemies = new Dictionary<int, Tag>();

    public override void Init()
    {
        RundownManager.add_OnExpeditionGameplayStarted((Action)OnRundownStart);
    }

    private void OnRundownStart()
    {
        taggedEnemies.Clear();
    }

#if IL2CPP

#region Fix for local player 

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

    // Used to handle UFloat conversion of damage
    private static pFullDamageData fullDamageData = new();

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
            if (__instance.Health <= 0) return;

            EnemyAgent owner = __instance.Owner;
            ushort id = owner.GlobalID;
            long now = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();

            if (!taggedEnemies.ContainsKey(id)) taggedEnemies.Add(id, new Tag(__instance.Health));
            Tag t = taggedEnemies[id];
            t.localHitPosition = position - owner.transform.position;
            t.timestamp = now;

            fullDamageData.damage.Set(dam, __instance.HealthMax);
            float num = AgentModifierManager.ApplyModifier(owner, AgentModifier.ProjectileResistance, fullDamageData.damage.Get(__instance.HealthMax));
            t.health -= num;

            // Show indicator when tracked health assumes enemy is dead
            if (t.health <= 0 && !__instance.DeathIndicatorShown) {
                GuiManager.CrosshairLayer?.ShowDeathIndicator(position);
                __instance.DeathIndicatorShown = true;
            }

            if (Settings.DebugLog)
            {
                FeatureLogger.Info($"{num} Bullet Damage done by {p.PlayerName}. IsBot: {p.Owner.IsBot}");
                FeatureLogger.Info($"Tracked current HP: {t.health}, [{id}]");
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

            if (!taggedEnemies.ContainsKey(id)) taggedEnemies.Add(id, new Tag(__instance.Health));
            Tag t = taggedEnemies[id];
            t.localHitPosition = position - owner.transform.position;
            t.timestamp = now;

            fullDamageData.damage.Set(dam, __instance.HealthMax);
            float num = AgentModifierManager.ApplyModifier(owner, AgentModifier.MeleeResistance, fullDamageData.damage.Get(__instance.HealthMax));
            t.health -= num;

            // Show indicator when tracked health assumes enemy is dead
            if (t.health <= 0 && !__instance.DeathIndicatorShown) {
                GuiManager.CrosshairLayer?.ShowDeathIndicator(position);
            }

            if (Settings.DebugLog)
            {
                FeatureLogger.Info($"Melee Damage: {num}");
                FeatureLogger.Info($"Tracked current HP: {__instance.Health}, [{id}]");
            }
        }
    }

#endregion

#region Fix for sentries (Unfinnished)
#if false

    // Determine if the shot was performed by a sentry or player
    private static bool sentryShot = false;

    // Auto or Burst Sentry
    [ArchivePatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.FireBullet), new Type[] 
    {  
        typeof(bool),
        typeof(bool)
    })]
    internal static class SentryGunInstance_Firing_Bullets_FireBullet_Patch
    {
        public static void Prefix(SentryGunInstance_Firing_Bullets __instance, bool doDamage, bool targetIsTagged)
        {
            if (!doDamage) return;
            sentryShot = true;
        }

        public static void Postfix(SentryGunInstance_Firing_Bullets __instance, bool doDamage, bool targetIsTagged)
        {
            sentryShot = false;
        }
    }

    // Shotgun Sentry
    [ArchivePatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireShotgunSemi), new Type[] 
    {  
        typeof(bool),
        typeof(bool)
    })]
    internal static class SentryGunInstance_Firing_Bullets_UpdateFireShotgunSemi_Patch
    {
        public static void Prefix(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            if (!isMaster) return;
            sentryShot = true;
        }

        public static void Postfix(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            sentryShot = false;
        }
    }

    // Send hitmarkers to clients from sentry shots
    [ArchivePatch(typeof(Dam_EnemyDamageLimb), nameof(Dam_EnemyDamageLimb.BulletDamage), new Type[] 
    {  
        typeof(float),
        typeof(Agent),
        typeof(Vector3),
        typeof(Vector3),
        typeof(Vector3),
        typeof(bool),
        typeof(float),
        typeof(float),
        typeof(uint)
    }, Priority = HarmonyLib.Priority.Last)]
    internal static class SentryGunInstance_Firing_Bullets_UpdateFireShotgunSemi_Patch
    {
        public static void Prefix(Dam_EnemyDamageLimb __instance, float dam, Agent sourceAgent, Vector3 position, Vector3 direction, Vector3 normal, bool allowDirectionalBonus, float staggerMulti, float precisionMulti, uint gearCategoryId)
        {
            if (!SNet.IsMaster) return;

            if (!sentryShot) return; // Check that it was a sentry that shot
            PlayerAgent? p = sourceAgent.TryCast<PlayerAgent>();
            if (p == null) // Check damage was done by a player
            {
                if (Settings.DebugLog) FeatureLogger.Info($"Could not find PlayerAgent, damage was done by agent of type: {sourceAgent.m_type}.");
                return;
            }
            if (p.Owner.IsBot) return; // Check player isnt a bot
            if (sourceAgent.IsLocallyOwned) return; // Check player is someone else

            Dam_EnemyDamageBase m_base = __instance.m_base;
            EnemyAgent owner = m_base.Owner;
            float num = dam;
            if (!m_base.IsImortal) {
                num = __instance.ApplyWeakspotAndArmorModifiers(dam, precisionMulti);
                num = __instance.ApplyDamageFromBehindBonus(num, position, direction);
                bool willDie = m_base.WillDamageKill(num);

                SendHitIndicator(p, owner, (byte)__instance.m_limbID, num > dam, willDie, position, __instance.m_armorDamageMulti < 1f);
            } else {
                SendHitIndicator(p, owner, (byte)__instance.m_limbID, num > dam, willDie: false, position, true);
            }
        }

        private static void SendHitIndicator(PlayerAgent sendTo, Agent target, byte limbID, bool hitWeakspot, bool willDie, Vector3 position, bool hitArmor = false) {
            // TODO(randomuserhi): Send Network packet (Make sure not sending to a bot)
        }

        private static void ReceiveHitIndicator(Agent target, byte limbID, bool hitWeakspot, bool willDie, Vector3 position, bool hitArmor = false) {
            // TODO(randomuserhi): OnReceive, display corresponding hit marker

            EnemyAgent? targetEnemy = target.TryCast<EnemyAgent>();
            if (targetEnemy != null) {
                Dam_EnemyDamageLimb dam = targetEnemy.Damage.DamageLimbs[limbID];
                dam.ShowHitIndicator(hitWeakspot, willDie, position, hitArmor);
            }
            PlayerAgent? targetPlayer = target.TryCast<PlayerAgent>();
            if (targetPlayer != null) {
                GuiManager.CrosshairLayer.PopFriendlyTarget();
            }

            if (Settings.DebugLog) FeatureLogger.Info("Received hit indicator from host.");
        }
    }

#endif
#endregion

#endif

#if MONO
        // TODO
#endif
}