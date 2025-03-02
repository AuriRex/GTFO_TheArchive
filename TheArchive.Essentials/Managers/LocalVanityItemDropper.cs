using System;
using System.Linq;
using TheArchive.Core;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.DataBlocks;
using TheArchive.Models.Vanity;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Managers;

public class LocalVanityItemDropper : InitSingletonBase<LocalVanityItemDropper>, IInitAfterDataBlocksReady, IInitCondition, IInjectLogger
{
    public IArchiveLogger Logger { get; set; }

    public CustomVanityItemsGroupDataBlock[] ItemGroups { get; private set; }
    public CustomVanityItemsTemplateDataBlock[] ItemTemplates { get; private set; }
    public CustomVanityItemsLayerDropsDataBlock[] ItemDropData { get; private set; }


    public bool InitCondition()
    {
        return ArchiveMod.CurrentBuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownSix.ToLatest());
    }

    public void Init()
    {
        ItemGroups = ImplementationManager.GetAllCustomDataBlocksFor<CustomVanityItemsGroupDataBlock>();
        ItemTemplates = ImplementationManager.GetAllCustomDataBlocksFor<CustomVanityItemsTemplateDataBlock>();
        ItemDropData = ImplementationManager.GetAllCustomDataBlocksFor<CustomVanityItemsLayerDropsDataBlock>();

        Logger.Msg(ConsoleColor.Magenta, $"{nameof(LocalVanityItemDropper)}.{nameof(Init)}() complete, retrieved {ItemTemplates.Length} Templates, {ItemGroups.Length} Groups and {ItemDropData.Length} Layer Drops. Layer Drops:");
        foreach(var dd in ItemDropData)
        {
            Logger.Info($" > {dd.Name}: #Drops: {dd.LayerDrops?.Count ?? 0}, Enabled: {dd.InternalEnabled}");
        }
    }

    public bool TryGetGroup(uint persistentID, out CustomVanityItemsGroupDataBlock itemGroup)
    {
        foreach(var group in ItemGroups)
        {
            if(group.PersistentID == persistentID)
            {
                itemGroup = group;
                return true;
            }
        }

        itemGroup = null;
        return false;
    }

    /// <summary>
    /// Drop (Add) an item from group with id <paramref name="groupID"/> into the players inventory <paramref name="playerData"/>
    /// </summary>
    /// <param name="groupID">The group to pick from</param>
    /// <param name="playerData">The players inventory data</param>
    /// <param name="silentDrop">If the game should announce that something new dropped</param>
    /// <returns>True if anything dropped</returns>
    public bool DropRandomFromGroup(uint groupID, LocalVanityItemStorage playerData, bool silentDrop = false)
    {
        if (TryGetGroup(groupID, out var itemGroup) && !itemGroup.HasAllOwned(playerData))
        {
            Logger.Msg(ConsoleColor.Magenta, $"Attempting drop of 1 Vanity Item from group \"{itemGroup.Name}\"");

            if(!itemGroup.GetNonOwned(playerData).TryPickRandom(out var itemId))
            {
                Logger.Info($"All items in group already in local player inventory, not dropping!");
                return false;
            }

            if(!TryGetTemplate(itemId, out var template))
            {
                Logger.Warning($"Template with ID {itemId} wasn't found!");
            }

            var item = new LocalVanityItemStorage.LocalVanityItem
            {
                ItemID = itemId,
                Flags = silentDrop ? LocalVanityItemStorage.VanityItemFlags.ALL : LocalVanityItemStorage.VanityItemFlags.None
            };

            playerData.Items.Add(item);
            Logger.Info($"Dropped Vanity Item \"{template?.PublicName ?? $"ID:{itemId}"}\"!");
            return true;
        }
        return false;
    }

    public bool TryGetTemplate(uint persistentID, out CustomVanityItemsTemplateDataBlock template)
    {
        template = ItemTemplates.Where(t => t.PersistentID == persistentID).FirstOrDefault();
        return template != null;
    }

    public bool HasAllItemsInGroup(uint groupID, LocalVanityItemStorage playerData)
    {
        if (TryGetGroup(groupID, out var itemGroup))
        {
            return itemGroup.HasAllOwned(playerData);
        }
        return false;
    }

    internal void DropFirstTimePlayingItems(LocalVanityItemStorage playerData)
    {
        Logger.Warning("Dropping initial Vanity Items ...");
        DropRandomFromGroup(3, playerData, true);
        DropRandomFromGroup(4, playerData, true);
        DropRandomFromGroup(5, playerData, true);
        DropRandomFromGroup(6, playerData, true);
        DropRandomFromGroup(7, playerData, true);
    }
}