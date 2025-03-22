using Gear;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Fixes;

[EnableFeatureByDefault]
[RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownAltFive)]
public class WeaponShootForward : Feature
{
    public override string Name => "Weapons shoot forward";

    public override FeatureGroup Group => FeatureGroups.Fixes;

    public override string Description => "Patches weapons to always shoot into the center of your crosshair.\nMakes shotgun draw & insta-shoot not shoot the floor";

    public new static IArchiveLogger FeatureLogger { get; set; }

    public static Vector3 Direction = Vector3.zero;
    public static int Frame = 0;

    [ArchivePatch(typeof(Shotgun), nameof(Shotgun.Fire))]
    public static class Shotgun_Fire_Patch
    {
        public static void Prefix(Shotgun __instance)
        {
            Direction = __instance.Owner.FPSCamera.CameraRayPos - __instance.Owner.FPSCamera.Position;
            Frame = Time.frameCount;
        }
    }

    [ArchivePatch(typeof(BulletWeapon), nameof(BulletWeapon.Fire))]
    public static class BulletWeapon_Fire_Patch
    {
        public static void Prefix(BulletWeapon __instance)
        {
            Direction = __instance.Owner.FPSCamera.CameraRayPos - __instance.Owner.FPSCamera.Position;
            Frame = Time.frameCount;
        }
    }

    [ArchivePatch(typeof(Weapon), nameof(Weapon.CastWeaponRay))]
    public static class Weapon_CastWeaponRay_Patch
    {
        public static Type[] ParameterTypes()
        {
            if (BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownOne))
                return new Type[] { typeof(Transform), typeof(Weapon.WeaponHitData).MakeByRefType(), typeof(Vector3) };
            return new Type[] { typeof(Transform), typeof(Weapon.WeaponHitData).MakeByRefType(), typeof(Vector3), typeof(int) };
        }

        public static void Prefix(ref Weapon.WeaponHitData weaponRayData)
        {
            if (Frame == Time.frameCount)
                weaponRayData.fireDir = Direction;
        }
    }
}