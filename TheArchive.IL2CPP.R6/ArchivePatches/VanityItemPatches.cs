using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core.Core;
using UnhollowerRuntimeLib;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.IL2CPP.R6.ArchivePatches
{
    [BindPatchToSetting(nameof(ArchiveSettings.UnlockAllVanityItems), "VanityUnlock")]
    public class VanityItemPatches
    {
        [ArchivePatch(typeof(VanityItemInventory), nameof(VanityItemInventory.UpdateItems))]
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
        }

    }
}
