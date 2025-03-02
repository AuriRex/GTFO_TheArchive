using Player;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using UnityEngine;

namespace TheArchive.Features.Fixes;

[EnableFeatureByDefault]
internal class ReloadMinusOneBugFix : Feature
{
    public override string Name => "99% Reload Fix";

    public override FeatureGroup Group => FeatureGroups.Fixes;

    public override string Description => "Fixes the bug that leaves you with one bullet short in the mag.\n(Currently only for IL2CPP builds)";

    public override bool InlineSettingsIntoParentMenu => true;

    public static new IArchiveLogger FeatureLogger { get; set; }

    [FeatureConfig]
    public static ReloadMinusOneBugFixSettings Settings { get; set; }

    public class ReloadMinusOneBugFixSettings
    {
        [FSHide]
        [FSDisplayName("Reload Debug Log")]
        [FSDescription("Logs expected values from PlayerAmmoStorage.GetClipBulletsFromPack")]
        public bool DebugLog { get; set; } = false;
    }

#if IL2CPP
    [ArchivePatch(typeof(PlayerAmmoStorage), nameof(PlayerAmmoStorage.GetClipBulletsFromPack))]
    internal static class PlayerAmmoStorage_GetClipBulletsFromPack_Patch
    {
        public static int BulletClipSize;
        public static int CurrentClip;
        public static float CostOfBullet;
        public static float AmmoInPack;
        public static float NewAmmoInPack;
        public static int NewCurrentClip;
        public static int BulletsNeeded;
        public static float NeededBulletCost;
        public static int BulletsToRefill;

        // just ignore anything that isn't Standard or Special ammo
        public static bool IsWrongAmmoType(AmmoType ammoType) => ammoType != AmmoType.Standard && ammoType != AmmoType.Special;

        public static void Prefix(PlayerAmmoStorage __instance, int currentClip, AmmoType ammoType)
        {
            if (IsWrongAmmoType(ammoType))
                return;

            InventorySlotAmmo inventorySlotAmmo = __instance.m_ammoStorage[(int)ammoType];

            BulletClipSize = inventorySlotAmmo.BulletClipSize;
            CostOfBullet = inventorySlotAmmo.CostOfBullet;
            AmmoInPack = inventorySlotAmmo.AmmoInPack;

            CurrentClip = currentClip;


            BulletsNeeded = BulletClipSize - CurrentClip;

            NeededBulletCost = BulletsNeeded * CostOfBullet;

            BulletsToRefill = (int)Math.Floor(0.0001d + Mathf.Min(NeededBulletCost, AmmoInPack) / CostOfBullet);

            NewAmmoInPack = AmmoInPack - BulletsToRefill * CostOfBullet;

            NewCurrentClip = CurrentClip + BulletsToRefill;

            if (Settings.DebugLog)
            {
                FeatureLogger.Notice($"BulletClipSize: {BulletClipSize}, CostOfBullet: {CostOfBullet}, AmmoInPack: {AmmoInPack}, CurrentClip: {CurrentClip}");
                FeatureLogger.Info($"BulletsNeeded = BulletClipSize - CurrentClip; => {BulletsNeeded}");
                FeatureLogger.Info($"NeededBulletCost = BulletsNeeded * CostOfBullet; => {NeededBulletCost}");
                FeatureLogger.Info($"BulletsToRefill = Mathf.FloorToInt(Mathf.Min(NeededBulletCost, AmmoInPack) / CostOfBullet); => {BulletsToRefill}");
                FeatureLogger.Notice($"NewAmmoInPack = AmmoInPack - BulletsToRefill * CostOfBullet; => {NewAmmoInPack}");
                FeatureLogger.Notice($"NewCurrentClip = CurrentClip + BulletsToRefill; => {NewCurrentClip}");
            }
        }

        public static void Postfix(PlayerAmmoStorage __instance, AmmoType ammoType, ref int __result)
        {
            if (IsWrongAmmoType(ammoType))
                return;

            if (Settings.DebugLog)
                FeatureLogger.Notice($"(result) returned currentClip: {__result}");

            if (__result + 1 == NewCurrentClip)
            {
                if (Settings.DebugLog)
                    FeatureLogger.Fail("Reload is wrong! Correcting ...");

                __result++;

                InventorySlotAmmo inventorySlotAmmo = __instance.m_ammoStorage[(int)ammoType];

                inventorySlotAmmo.AmmoInPack -= inventorySlotAmmo.CostOfBullet;
                __instance.UpdateSlotAmmoUI(inventorySlotAmmo, __result);
            }
        }
    }
#endif

#if MONO
        // TODO: if it's an issue on MONO transpile PlayerAmmoStorage.GetClipBulletsFromPack
        // Mathf.FloorToInt(f)  ==>  (int)Math.Floor((double)f + 0.0001d);
#endif
}