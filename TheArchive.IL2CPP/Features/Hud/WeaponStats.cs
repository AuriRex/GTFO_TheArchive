using CellMenu;
using GameData;
using Gear;
using Player;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.Hud
{
    public class WeaponStats : Feature
    {
        public override string Name => "Show Weapon Stats";

        public override string Group => FeatureGroups.Hud;

        public override string Description => "Adds weapon statistics such as damage, clip size and reload speed (and more if applicable) on the weapon select screen.";

        private static PlayerDataBlock _playerDataBlock;
        public override void OnDatablocksReady()
        {
            _playerDataBlock = PlayerDataBlock.GetBlock(1U);
        }

        public static int GetAmmoMax(ItemDataBlock itemDataBlock)
        {
            var ammoType = PlayerAmmoStorage.GetAmmoTypeFromSlot(itemDataBlock.inventorySlot);
            switch (ammoType)
            {
                case AmmoType.Standard:
                    return _playerDataBlock.AmmoStandardMaxCap;
                case AmmoType.Special:
                    return _playerDataBlock.AmmoSpecialMaxCap;
                case AmmoType.Class:
                    return _playerDataBlock.AmmoClassMaxCap;
                case AmmoType.CurrentConsumable:
                    return itemDataBlock.ConsumableAmmoMax;
            }
            return -1;
        }

        public static int GetTotalAmmo(ArchetypeDataBlock archetypeDataBlock, ItemDataBlock itemDataBlock, bool isSentryGun = false)
        {
            var max = GetAmmoMax(itemDataBlock);

            var costOfBullet = archetypeDataBlock.CostOfBullet;

            if (isSentryGun)
            {
                costOfBullet = costOfBullet * itemDataBlock.ClassAmmoCostFactor;

                if (archetypeDataBlock.ShotgunBulletCount > 0f)
                {
                    costOfBullet *= archetypeDataBlock.ShotgunBulletCount;
                }
            }

            var maxBullets = (int)(max / costOfBullet);

            if (isSentryGun)
                return maxBullets;

            return maxBullets + archetypeDataBlock.DefaultClipSize;
        }

        public const string DIVIDER = " | ";
        public const string CLOSE_COLOR_TAG = "</color>";

        public static string Short_MeleeLight { get; } = ".Lgt";
        public static string Short_MeleeCharged { get; } = ".Hvy";

        public static string Short_MeleeCanRunWhileCharging { get; } = "Run";
        public static string Short_MeleeSleepingEnemiesMultiplier { get; } = "Slp";
        public static string Short_EnvironmentMultiplier { get; } = "Env";

        public static string Short_Damage { get; } = "Dmg";
        public static string Short_Clip { get; } = "Clp";
        public static string Short_MaxAmmo { get; } = "Max";
        public static string Short_Reload { get; } = "Rld";
        public static string Short_Stagger { get; } = "Stgr";
        public static string Short_Precision { get; } = "Pcsn";
        public static string Short_PierceCount { get; } = "Prc";
        public static string Short_ShotgunPelletCount { get; } = "Sh.Plts";
        public static string Short_ShotgunSpread { get; } = "Sh.Sprd";
        public static string Short_BurstShotCount { get; } = "Brst.Sht";
        public static string Short_BurstDelay { get; } = "Brst.Dly";
        public static string Short_FalloffDistanceClose { get; } = "Dst.C";
        public static string Short_FalloffDistanceFar { get; } = "Dst.F";

        //public void LoadData(GearIDRange idRange, bool clickable, bool detailedInfo)
#if IL2CPP
        [RundownConstraint(Utils.RundownFlags.RundownFive, Utils.RundownFlags.Latest)]
        [ArchivePatch(nameof(CM_InventorySlotItem.LoadData))]
        internal static class CM_InventorySlotItem_LoadData_Patch
        {
            public static Type Type() => typeof(CM_InventorySlotItem);
            public static void Postfix(CM_InventorySlotItem __instance, GearIDRange idRange)
            {
                uint categoryID = idRange.GetCompID(eGearComponent.Category);

                GearCategoryDataBlock gearCatBlock = GameDataBlockBase<GearCategoryDataBlock>.GetBlock(categoryID);

                ItemDataBlock itemDataBlock = ItemDataBlock.GetBlock(gearCatBlock.BaseItem);

                if (itemDataBlock.inventorySlot == InventorySlot.GearMelee)
                {
                    if (BuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownSix.ToLatest()))
                        R6PlusOnly(__instance, gearCatBlock, itemDataBlock);
                }
                else
                {
                    eWeaponFireMode weaponFireMode = (eWeaponFireMode)idRange.GetCompID(eGearComponent.FireMode);

                    bool isSentryGun = categoryID == 12; // => PersistentID of the Sentry Gun Category

                    ArchetypeDataBlock archetypeDataBlock = isSentryGun
                        ? SentryGunInstance_Firing_Bullets.GetArchetypeDataForFireMode(weaponFireMode)
                        : ArchetypeDataBlock.GetBlock(GearBuilder.GetArchetypeID(gearCatBlock, weaponFireMode));



                    __instance.GearDescription = __instance.GearDescription + "\n\n" + GetFormatedWeaponStats(archetypeDataBlock, itemDataBlock, isSentryGun);
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void R6PlusOnly(CM_InventorySlotItem __instance, GearCategoryDataBlock gearCatBlock, ItemDataBlock itemDataBlock)
            {
                MeleeArchetypeDataBlock meleeArchetypeDataBlock = MeleeArchetypeDataBlock.GetBlock(GearBuilder.GetMeleeArchetypeID(gearCatBlock));

                __instance.GearDescription = __instance.GearDescription + "\n\n" + OnlyTouchInR6OrHigher.GetFormatedWeaponStats(meleeArchetypeDataBlock, itemDataBlock);
            }
        }
#endif
        private static class OnlyTouchInR6OrHigher
        {
#if IL2CPP
            public static string GetFormatedWeaponStats(MeleeArchetypeDataBlock archeTypeDataBlock, ItemDataBlock itemDataBlock)
            {
                if (archeTypeDataBlock == null) return string.Empty;

                StringBuilder builder = new StringBuilder();

                builder.Append("<#9D2929>");
                builder.Append($"{Short_Damage}{Short_MeleeLight} ");
                builder.Append(archeTypeDataBlock.LightAttackDamage);
                builder.Append(CLOSE_COLOR_TAG);

                builder.Append(DIVIDER);

                builder.Append("<color=orange>");
                builder.Append($"{Short_Damage}{Short_MeleeCharged} ");
                builder.Append(archeTypeDataBlock.ChargedAttackDamage);
                builder.Append(CLOSE_COLOR_TAG);

                int count = 2;

                if (!archeTypeDataBlock.AllowRunningWhenCharging)
                {
                    builder.Append(DIVIDER);

                    builder.Append("<#FFD306>");
                    builder.Append($"{Short_MeleeCanRunWhileCharging} ");
                    builder.Append(archeTypeDataBlock.AllowRunningWhenCharging);
                    builder.Append(CLOSE_COLOR_TAG);
                    count++;
                }

                if (archeTypeDataBlock.LightStaggerMulti != 1f)
                {
                    Divider(ref count, builder);

                    builder.Append("<#C0FF00>");
                    builder.Append($"{Short_Stagger}{Short_MeleeLight} ");
                    builder.Append(archeTypeDataBlock.LightStaggerMulti);
                    builder.Append(CLOSE_COLOR_TAG);
                }

                if (archeTypeDataBlock.ChargedStaggerMulti != 1f)
                {
                    Divider(ref count, builder);

                    builder.Append("<color=green>");
                    builder.Append($"{Short_Stagger}{Short_MeleeCharged} ");
                    builder.Append(archeTypeDataBlock.ChargedStaggerMulti);
                    builder.Append(CLOSE_COLOR_TAG);
                }

                if (archeTypeDataBlock.LightPrecisionMulti != 1f)
                {
                    Divider(ref count, builder);

                    builder.Append("<#004E2C>");
                    builder.Append($"{Short_Precision}{Short_MeleeLight} ");
                    builder.Append(archeTypeDataBlock.LightPrecisionMulti);
                    builder.Append(CLOSE_COLOR_TAG);
                }

                if (archeTypeDataBlock.ChargedPrecisionMulti != 1f)
                {
                    Divider(ref count, builder);

                    builder.Append("<#55022B>");
                    builder.Append($"{Short_Precision}{Short_MeleeCharged} ");
                    builder.Append(archeTypeDataBlock.ChargedPrecisionMulti);
                    builder.Append(CLOSE_COLOR_TAG);
                }

                if (archeTypeDataBlock.LightSleeperMulti != 1f)
                {
                    Divider(ref count, builder);

                    builder.Append("<#A918A7>");
                    builder.Append($"{Short_MeleeSleepingEnemiesMultiplier}{Short_MeleeLight} ");
                    builder.Append(archeTypeDataBlock.LightSleeperMulti);
                    builder.Append(CLOSE_COLOR_TAG);
                }

                if (archeTypeDataBlock.ChargedSleeperMulti != 1f)
                {
                    Divider(ref count, builder);

                    builder.Append("<#025531>");
                    builder.Append($"{Short_MeleeSleepingEnemiesMultiplier}{Short_MeleeCharged} ");
                    builder.Append(archeTypeDataBlock.ChargedSleeperMulti);
                    builder.Append(CLOSE_COLOR_TAG);
                }

                if (archeTypeDataBlock.LightEnvironmentMulti != 1f)
                {
                    Divider(ref count, builder);

                    builder.Append("<#18A4A9>");
                    builder.Append($"{Short_EnvironmentMultiplier}{Short_MeleeLight} ");
                    builder.Append(archeTypeDataBlock.LightEnvironmentMulti);
                    builder.Append(CLOSE_COLOR_TAG);
                }

                if (archeTypeDataBlock.ChargedEnvironmentMulti != 1f)
                {
                    Divider(ref count, builder);

                    builder.Append("<#75A2AA>");
                    builder.Append($"{Short_EnvironmentMultiplier}{Short_MeleeCharged} ");
                    builder.Append(archeTypeDataBlock.ChargedEnvironmentMulti);
                    builder.Append(CLOSE_COLOR_TAG);
                }

                return builder.ToString();
            }
#endif
        }

        private static void Divider(ref int count, StringBuilder builder, int maxPerLine = 3)
        {
            if (count >= maxPerLine)
            {
                builder.Append("\n");
                count = 0;
            }
            else if (count > 0)
                builder.Append(DIVIDER);

            count++;
        }

        public static string GetFormatedWeaponStats(ArchetypeDataBlock archeTypeDataBlock, ItemDataBlock itemDataBlock, bool isSentryGun = false)
        {
            if (archeTypeDataBlock == null) return string.Empty;

            StringBuilder builder = new StringBuilder();

            var count = 1;

            builder.Append("<#9D2929>");
            builder.Append($"{Short_Damage} ");
            builder.Append(archeTypeDataBlock.Damage);
            builder.Append(CLOSE_COLOR_TAG);

            if (!isSentryGun)
            {
                Divider(ref count, builder, 4);

                builder.Append("<color=orange>");
                builder.Append($"{Short_Clip} ");
                builder.Append(archeTypeDataBlock.DefaultClipSize);
                builder.Append(CLOSE_COLOR_TAG);
            }

            Divider(ref count, builder, 4);

            builder.Append("<#FFD306>");
            builder.Append($"{Short_MaxAmmo} ");
            builder.Append(GetTotalAmmo(archeTypeDataBlock, itemDataBlock, isSentryGun));
            builder.Append(CLOSE_COLOR_TAG);

            if (!isSentryGun)
            {
                Divider(ref count, builder, 4);

                builder.Append("<#C0FF00>");
                builder.Append($"{Short_Reload} ");
                builder.Append(archeTypeDataBlock.DefaultReloadTime);
                builder.Append(CLOSE_COLOR_TAG);
            }

#if IL2CPP
            if (archeTypeDataBlock.StaggerDamageMulti != 1f)
            {
                Divider(ref count, builder, 3);

                builder.Append("<color=green>");
                builder.Append($"{Short_Stagger} ");
                builder.Append(archeTypeDataBlock.StaggerDamageMulti);
                builder.Append(CLOSE_COLOR_TAG);
            }
#endif

            if (archeTypeDataBlock.PiercingBullets)
            {
                Divider(ref count, builder, 4);

                builder.Append("<#004E2C>");
                builder.Append($"{Short_PierceCount} ");
                builder.Append(archeTypeDataBlock.PiercingDamageCountLimit);
                builder.Append(CLOSE_COLOR_TAG);
            }

            bool isShotgun = archeTypeDataBlock.ShotgunBulletCount > 0;

            if (isShotgun)
            {
                Divider(ref count, builder, 4);

                builder.Append("<#55022B>");
                builder.Append($"{Short_ShotgunPelletCount} ");
                builder.Append(archeTypeDataBlock.ShotgunBulletCount);
                builder.Append(CLOSE_COLOR_TAG);

                Divider(ref count, builder, 4);

                builder.Append("<#A918A7>");
                builder.Append($"{Short_ShotgunSpread} ");
                builder.Append(archeTypeDataBlock.ShotgunBulletSpread);
                builder.Append(CLOSE_COLOR_TAG);
            }

            if (archeTypeDataBlock.BurstShotCount > 1 && archeTypeDataBlock.FireMode == eWeaponFireMode.Burst)
            {
                Divider(ref count, builder, 3);

                builder.Append("<#025531>");
                builder.Append($"{Short_BurstShotCount} ");
                builder.Append(archeTypeDataBlock.BurstShotCount);
                builder.Append(CLOSE_COLOR_TAG);

                Divider(ref count, builder, 4);

                builder.Append("<#18A4A9>");
                builder.Append($"{Short_BurstDelay} ");
                builder.Append(archeTypeDataBlock.BurstDelay);
                builder.Append(CLOSE_COLOR_TAG);

                count++; // yes
            }

            Divider(ref count, builder, 3);

            builder.Append("<#00FFA2>");
            builder.Append($"{Short_FalloffDistanceClose} ");
            builder.Append(archeTypeDataBlock.DamageFalloff.x);
            builder.Append(" m");
            builder.Append(CLOSE_COLOR_TAG);

            Divider(ref count, builder, 3);

            builder.Append("<#1D82FF>");
            builder.Append($"{Short_FalloffDistanceFar} ");
            builder.Append(archeTypeDataBlock.DamageFalloff.y);
            builder.Append(" m");
            builder.Append(CLOSE_COLOR_TAG);

            return builder.ToString();
        }
    }
}
