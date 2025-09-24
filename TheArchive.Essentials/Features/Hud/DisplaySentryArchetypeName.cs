using GameData;
using Gear;
using Player;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Utilities;

#if IL2CPP
using ColGen = Il2CppSystem.Collections.Generic;
#else
using ColGen = System.Collections.Generic;
#endif

namespace TheArchive.Features.Hud;

public class DisplaySentryArchetypeName : Feature
{
    public override string Name => "Display Sentry Type";

    public override GroupBase Group => GroupManager.Hud;

    public override string Description => "Display the Sentry Type (<color=orange>Sniper</color>, <color=orange>Burst</color>, <color=orange>Auto</color>, <color=orange>Shotgun</color>) for remote players instead of the nondescript \"<color=orange>Sentry Gun</color>\" on the map screen.";


    private static readonly IValueAccessor<ArchetypeDataBlock, string> _ArchetypeDataBlock_PublicName = AccessorBase.GetValueAccessor<ArchetypeDataBlock, string>(nameof(ArchetypeDataBlock.PublicName));

        
    [ArchivePatch(typeof(PUI_Inventory), nameof(PUI_Inventory.UpdateInfoForItem))]
    internal static class PUI_Inventory_UpdateInfoForItem_Patch
    {
        private static IValueAccessor<PUI_Inventory, ColGen.Dictionary<InventorySlot, PUI_InventoryItem>> _m_inventorySlots;
        private static IValueAccessor<Item, ItemDataBlock> _Item_ItemDataBlock;

        public static void Init()
        {
            _m_inventorySlots = AccessorBase.GetValueAccessor<PUI_Inventory, ColGen.Dictionary<InventorySlot, PUI_InventoryItem>>("m_inventorySlots");

            _Item_ItemDataBlock = AccessorBase.GetValueAccessor<Item, ItemDataBlock>("ItemDataBlock")
                .OrAlternative(() => AccessorBase.GetValueAccessor<Item, ItemDataBlock>("ItemData")); // <-- R1
        }

        public static void Postfix(PUI_Inventory __instance, BackpackItem bpItem)
        {
            if (bpItem?.Instance == null || bpItem?.GearIDRange == null)
                return;

            bool isSentryGun = bpItem.GearIDRange.GetCompID(eGearComponent.Category) == 12; // => PersistentID of the Sentry Gun Category
            if (!isSentryGun)
                return;

            Item item = bpItem.Instance;
            InventorySlot inventorySlot = _Item_ItemDataBlock.Get(item).inventorySlot;

            eWeaponFireMode weaponFireMode = (eWeaponFireMode)bpItem.GearIDRange.GetCompID(eGearComponent.FireMode);
            ArchetypeDataBlock archetypeDataBlock = SentryGunInstance_Firing_Bullets.GetArchetypeDataForFireMode(weaponFireMode);

            if (_m_inventorySlots.Get(__instance).ContainsKey(inventorySlot))
            {
                PUI_InventoryItem pui_InventoryItem = _m_inventorySlots.Get(__instance)[inventorySlot];

                var prefix = string.Empty;
                if(pui_InventoryItem.IsDeployed)
                {
                    prefix = $"<COLOR=red>{SharedUtils.GetLocalizedTextSafe(2505980868U, overrideText: "DEPLOYED")}</color> ";
                }

                pui_InventoryItem.SetArchetypeName(prefix + GetPublicName(archetypeDataBlock));
            }
        }
    }

    public static string GetPublicName(ArchetypeDataBlock archetypeDataBlock)
    {
        if (archetypeDataBlock == null)
            return string.Empty;

        if (Is.R6OrLater)
            return GetPublicNameR6(archetypeDataBlock);

        return _ArchetypeDataBlock_PublicName.Get(archetypeDataBlock);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string GetPublicNameR6(ArchetypeDataBlock archetypeDataBlock)
    {
        return archetypeDataBlock.PublicName;
    }
}