using Agents;
using Enemies;
using Gear;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Dev;

[HideInModSettings]
internal class DamageLog : Feature
{
    // TODO: Fix on R1 build maybe?
    public override string Name => "Damage Log";

    public override FeatureGroup Group => FeatureGroups.Dev;

    public override string Description => "Log what all hitboxes/enemies were hit by your shot to console.\n\n(Probably only works correctly as master)\n(Disable for normal gameplay)";

    public new static IArchiveLogger FeatureLogger { get; set; }

    public static bool BulletWeaponFired { get; private set; } = false;
    public static bool ShotgunWeaponFired { get; private set; } = false;

    private static uint _shotID = 0;
    public static uint ShotID
    {
        get => _shotID;
        set
        {
            if (_shotID >= uint.MaxValue)
            {
                _shotID = 0;
                return;
            }
            _shotID = value;
        }
    }

    public static bool AnyWeaponFired => BulletWeaponFired || ShotgunWeaponFired;

    [ArchivePatch(typeof(BulletWeapon), nameof(BulletWeapon.Fire))]
    public static class BulletWeapon_Fire_Patch
    {
        public static void Prefix()
        {
            BulletWeaponFired = true;
            ShotID++;
            FeatureLogger.Debug("BulletWeapon shot start");
        }

        public static void Postfix()
        {
            BulletWeaponFired = false;
            FeatureLogger.Debug("BulletWeapon shot end");
        }
    }

    [ArchivePatch(typeof(Shotgun), nameof(Shotgun.Fire))]
    public static class Shotgun_Fire_Patch
    {
        public static void Prefix()
        {
            ShotgunWeaponFired = true;
            ShotID++;
            FeatureLogger.Debug("Shotgun shot start");
        }

        public static void Postfix()
        {
            ShotgunWeaponFired = false;
            FeatureLogger.Debug("Shotgun shot end");
        }
    }

    [ArchivePatch(typeof(BulletWeapon), nameof(BulletWeapon.BulletHit))]
    public static class BulletWeapon_BulletHit_Patch
    {
        public static uint currentShotID;
        public static int penCount = 0;

        public static bool isFirstRay = false;

        public static GameObject hitGO;
        public static IDamageable damageable;
        public static Agent hitAgent;
        public static EnemyAgent hitEnemyAgent;
        public static float enemyHealth;

        public static bool HasHitAnyAgent => hitAgent != null;
        public static bool HasHitEnemy => hitEnemyAgent != null;

        [IsPrefix]
        [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownTwo)]
        public static void PrefixPreR3(Weapon.WeaponHitData weaponRayData) => PrefixR3(weaponRayData, 0);

        [IsPrefix]
        [RundownConstraint(Utils.RundownFlags.RundownThree, Utils.RundownFlags.Latest)]
        public static void PrefixR3(Weapon.WeaponHitData weaponRayData, float additionalDis)
        {
            // additionalDis is distance from previous shot for pierce
            isFirstRay = additionalDis == 0;

            if (currentShotID != ShotID || isFirstRay)
            {
                penCount = 0;
            }
            currentShotID = ShotID;


            hitGO = weaponRayData?.rayHit.collider?.gameObject;
            damageable = hitGO?.GetComponent<ColliderMaterial>()?.Damageable
                         ?? hitGO?.GetComponent<IDamageable>();
            hitAgent = damageable?.GetBaseAgent();

            hitEnemyAgent = hitAgent?.TryCastTo<EnemyAgent>();

            if (HasHitEnemy)
            {
                enemyHealth = hitEnemyAgent?.Damage?.Health ?? -1f;
            }
            else
            {
                enemyHealth = -1f;
            }
        }

        public static void Postfix(Weapon.WeaponHitData weaponRayData, bool doDamage)
        {
            FeatureLogger.Debug($"HitGO Name: {hitGO?.name}");

            if (!doDamage)
                return;

            var penInfo = $"| ({(penCount > 0 ? $"Pen: {penCount}" : "Origin")})";

            if (!HasHitAnyAgent)
            {
                FeatureLogger.Info($"Hit World Geometry! {penInfo}");
                return;
            }

            if (!HasHitEnemy)
            {
                FeatureLogger.Info($"Hit Other Agent! | AgentGOName: {hitAgent?.gameObject?.name} {penInfo}");
                return;
            }

            FeatureLogger.Msg(isFirstRay ? ConsoleColor.Cyan : ConsoleColor.DarkCyan, $"Hit: {hitEnemyAgent?.gameObject?.name}: HP_Before: {enemyHealth} | BaseDMG: {weaponRayData.damage} | HP_After: {hitEnemyAgent.Damage.Health} {penInfo}");

            penCount++;
        }
    }
}