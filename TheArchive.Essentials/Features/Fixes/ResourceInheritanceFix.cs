using Player;
using SNetwork;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;

namespace TheArchive.Features.Fixes;

[EnableFeatureByDefault]
public class ResourceInheritanceFix : Feature
{
    public override string Name => "Resource Inheritance Fix";

    public override string Description => "Transfers resources from players who leave the game to new players who join, preventing resource loss for the team.\n" +
        "When a player exits during gameplay, their resource pack and consumables will be inherited by the next player who joins.";

    public override GroupBase Group => GroupManager.Fixes;

    [ArchivePatch(typeof(PlayerBackpackManager), nameof(PlayerBackpackManager.MasterAddAllItemsForDropin))]
    private static class PlayerBackpackManager__MasterAddAllItemsForDropin__Prefix
    {
        private static void Prefix(SNet_Player sendToPlayer)
        {
            if (!SNet.IsMaster || sendToPlayer.IsLocal)
                return;
            if (CurrentGameState != (int)eGameStateName.InLevel)
                return;
            if (PlayerManager.Current.m_leaverBackpacks.Count == 0)
                return;
            if (!PlayerBackpackManager.TryGetBackpack(sendToPlayer, out var targetBackpack))
                return;
            bool hasResourcePack = false;
            bool hasConsumable = false;
            foreach (var leaverBackpack in PlayerManager.Current.m_leaverBackpacks)
            {
                if (hasResourcePack && hasConsumable)
                    return;
                if (!hasResourcePack && !targetBackpack.TryGetBackpackItem(InventorySlot.ResourcePack, out var resourcePack))
                {
                    if (leaverBackpack.TryGetBackpackItem(InventorySlot.ResourcePack, out var leftResourcePack))
                    {
                        PlayerBackpackManager.MasterAddItem(leftResourcePack.Instance, targetBackpack);
                        leaverBackpack.BackpackItems.Remove(leftResourcePack);
                        hasResourcePack = true;
                    }
                }
                if (!hasConsumable && !targetBackpack.TryGetBackpackItem(InventorySlot.Consumable, out var consumable))
                {
                    if (leaverBackpack.TryGetBackpackItem(InventorySlot.Consumable, out var leftConsumable))
                    {
                        PlayerBackpackManager.MasterAddItem(leftConsumable.Instance, targetBackpack);
                        leaverBackpack.BackpackItems.Remove(leftConsumable);
                        hasConsumable = true;
                    }
                }
            }
        }
    }

    [ArchivePatch(typeof(PlayerManager), nameof(PlayerManager.OnPlayerSpawned))]
    private static class PlayerManager__OnPlayerSpawned__Prefix
    {
        private static PlayerBackpack _tempBackpack;

        private static void Prefix(PlayerManager __instance, pPlayerSpawnData spawnData)
        {
            if (!SNet.IsMaster)
                return;
            if (CurrentGameState != (int)eGameStateName.InLevel)
                return;

            spawnData.snetPlayer.GetPlayer(out var player);
            if (player.IsBot && __instance.m_leaverBackpacks.Count != 0)
                _tempBackpack = __instance.m_leaverBackpacks[__instance.m_leaverBackpacks.Count - 1];
        }

        private static void Postfix(PlayerManager __instance, pPlayerSpawnData spawnData)
        {
            if (!SNet.IsMaster)
                return;
            if (CurrentGameState != (int)eGameStateName.InLevel)
                return;

            spawnData.snetPlayer.GetPlayer(out var player);
            if (player.IsBot && _tempBackpack != null && !__instance.m_leaverBackpacks.Contains(_tempBackpack))
                __instance.m_leaverBackpacks.Add(_tempBackpack);
        }
    }
}
