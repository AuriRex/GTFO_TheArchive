using CellMenu;
using SNetwork;
using System;
using System.Linq;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    public class PlayerInfoPatches
    {
        public static bool TryGetPlayerByCharacterIndex(int id, out SNet_Player player)
        {
            try
            {
                player = SNet.Lobby.Players
#if IL2CPP
                    .ToSystemList()
#endif
                    .FirstOrDefault(ply => ply.CharacterSlot.index == id);

                return player != null;
            }
            catch (Exception)
            {
                player = null;
                ArchiveLogger.Debug($"This shouldn't happen :skull: ({nameof(TryGetPlayerByCharacterIndex)})");
            }
            return false;
        }

        internal static void OnNameButtonPressed(int id)
        {
            if (!TryGetPlayerByCharacterIndex(id - 1, out var player))
            {
                ArchiveLogger.Debug($"No player found for index {id - 1}.");
                return;
            }

            ArchiveLogger.Info($"Opening Steam profile for player \"{player.NickName}\" ({player.Lookup})");
            Application.OpenURL($"https://steamcommunity.com/profiles/{player.Lookup}");
        }

        [ArchivePatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.UpdatePlayer))]
        internal static class CM_PlayerLobbyBar_UpdatePlayerPatch
        {
            public static void Postfix(CM_PlayerLobbyBar __instance, SNet_Player player)
            {
                if (player?.CharacterSlot?.index == null) return;

                var nameGO = __instance.m_nickText.gameObject;

                var CM_Item = nameGO.GetComponent<CM_Item>();
                if (CM_Item == null)
                {
                    ArchiveLogger.Debug($"Setting up player name button for index {player.CharacterSlot.index}");
                    var collider = nameGO.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2(447.2f, 52.8f);
                    collider.offset = new Vector2(160f, 1.6f);

                    CM_Item = nameGO.AddComponent<CM_Item>();
                    CM_Item.ID = player.CharacterSlot.index + 1; // +1
                    CM_Item.m_onBtnPress = new UnityEngine.Events.UnityEvent();
#if IL2CPP
                    CM_Item.OnBtnPressCallback = (Action<int>)OnNameButtonPressed;
#else
                    CM_Item.OnBtnPressCallback += OnNameButtonPressed;
#endif
                }
            }
        }

        [ArchivePatch(typeof(PUI_Inventory), nameof(PUI_Inventory.UpdateAllSlots))]
        internal static class PUI_Inventory_UpdateAllSlotsPatch
        {
            public static void Postfix(PUI_Inventory __instance, SNet_Player player)
            {
                if (player?.CharacterSlot?.index == null) return;

                var headerRootGO = __instance.m_headerRoot;

                var CM_Item = headerRootGO.GetComponent<CM_Item>();
                if (CM_Item == null)
                {
                    ArchiveLogger.Debug($"Setting up player name button (map) for index {player.CharacterSlot.index}");
                    var collider = headerRootGO.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2(400f, 40f);
                    collider.offset = new Vector2(-200f, 0f);

                    CM_Item = headerRootGO.AddComponent<CM_Item>();
                    CM_Item.ID = player.CharacterSlot.index + 1; // +1
                    CM_Item.m_onBtnPress = new UnityEngine.Events.UnityEvent();
#if IL2CPP
                    CM_Item.OnBtnPressCallback = (Action<int>)OnNameButtonPressed;
#else
                    CM_Item.OnBtnPressCallback += OnNameButtonPressed;
#endif
                }
            }
        }
    }
}
