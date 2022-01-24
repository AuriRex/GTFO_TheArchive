using GameData;
using System;
using System.Collections.Generic;
using TheArchive.Core;
using UnhollowerRuntimeLib;
using static TheArchive.Core.ArchiveNativePatcher;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.IL2CPP.R6.ArchivePatches
{
    [BindPatchToSetting(nameof(ArchiveSettings.UnlockAllVanityItems), "VanityUnlock")]
    public class VanityItemPatches
    {
        /*[ArchivePatch(typeof(VanityItemInventory), nameof(VanityItemInventory.UpdateItems))]
        internal static class VanityItemInventory_UpdateItemsPatch
        {
            public static bool Prefix(VanityItemInventory __instance)
            {

                if(__instance.m_newItems == null || __instance.m_backednItems == null)
                {
                    __instance.m_newItems = new Il2CppSystem.Collections.Generic.List<VanityItem>(10);
                    __instance.m_backednItems = new Il2CppSystem.Collections.Generic.List<VanityItem>(10);
                }

                __instance.m_newItems.Clear();
                __instance.m_backednItems.Clear();

                foreach(var block in GameDataBlockBase<VanityItemsTemplateDataBlock>.GetAllBlocks())
                {
                    if (block == null) continue;

                    VanityItem item = new VanityItem(ClassInjector.DerivedConstructorPointer<VanityItem>());
                    item.publicName = block.publicName;
                    item.type = block.type;
                    item.prefab = block.prefab;
                    item.flags = VanityItemFlags.Touched | VanityItemFlags.Acknowledged;
                    item.id = block.persistentID;
                    __instance.m_backednItems.Add(item);
                }

                return false;
            }
        }*/

        [ArchiveNativePatch(typeof(VanityItemInventory), nameof(VanityItemInventory.UpdateItems))]
        internal static class VanityItemInventory_UpdateItemsPatch
        {
            public static NativePatchInstance NativePatchInstance { get; set; }

            public static void Replacement(IntPtr self, IntPtr vanityItemPlayerData)
            {
                VanityItemInventory __instance = new VanityItemInventory(self);

                NativePatchInstance.OriginalMethod.DynamicInvoke(self, vanityItemPlayerData);

                var backedIds = new List<uint>();

                foreach(var item in __instance.m_backednItems)
                {
                    backedIds.Add(item.id);
                }

                foreach (var block in GameDataBlockBase<VanityItemsTemplateDataBlock>.GetAllBlocks())
                {
                    if (block == null) continue;

                    if (backedIds.Contains(block.persistentID)) continue;

                    VanityItem item = new VanityItem(ClassInjector.DerivedConstructorPointer<VanityItem>());
                    item.publicName = $"<color=red>{block.publicName}</color>";
                    item.type = block.type;
                    item.prefab = block.prefab;
                    item.flags = VanityItemFlags.Touched | VanityItemFlags.Acknowledged;
                    item.id = block.persistentID;
                    __instance.m_backednItems.Add(item);
                }
            }
        }

    }
}
