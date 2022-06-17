using CellMenu;
using SNetwork;
using System;
using System.Collections.Generic;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features
{
    [EnableFeatureByDefault]
    public class PlayerInfo : Feature
    {
        public override string Name => "Steam Profile on Name";

        public override void OnEnable()
        {
            foreach (var kvp in CM_PlayerLobbyBar_UpdatePlayerPatch.colliderMap)
            {
                var collider = kvp.Value;

                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
        }

        public override void OnDisable()
        {
            foreach(var kvp in CM_PlayerLobbyBar_UpdatePlayerPatch.colliderMap)
            {
                var collider = kvp.Value;

                if(collider != null)
                {
                    collider.enabled = false;
                }
            }
        }

        // Lobby button
        [ArchivePatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.UpdatePlayer))]
        internal static class CM_PlayerLobbyBar_UpdatePlayerPatch
        {
            internal static Dictionary<int, BoxCollider2D> colliderMap = new Dictionary<int, BoxCollider2D>();

            public static void Postfix(CM_PlayerLobbyBar __instance, SNet_Player player)
            {
                if (player?.CharacterSlot?.index == null) return;

                var charIndex = player.CharacterSlot.index;

                var nameGO = __instance.m_nickText.gameObject;

                var CM_Item = nameGO.GetComponent<CM_Item>();
                if (CM_Item == null)
                {
                    ArchiveLogger.Debug($"Setting up player name button for index {player.CharacterSlot.index}");
                    var collider = nameGO.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2(447.2f, 52.8f);
                    collider.offset = new Vector2(160f, 1.6f);

                    if (colliderMap.ContainsKey(charIndex))
                        colliderMap.Remove(charIndex);
                    colliderMap.Add(charIndex, collider);

                    CM_Item = nameGO.AddComponent<CM_Item>();
                    CM_Item.ID = charIndex + 1; // +1

                    CM_Item.SetCMItemEvents(OnNameButtonPressed);
                }
            }
        }

        // In expedition map buttons
        [ArchivePatch(typeof(PUI_Inventory), nameof(PUI_Inventory.UpdateAllSlots))]
        internal static class PUI_Inventory_UpdateAllSlotsPatch
        {
            internal static Dictionary<int, BoxCollider2D> colliderMap = new Dictionary<int, BoxCollider2D>();

            public static void Postfix(PUI_Inventory __instance, SNet_Player player)
            {
                if (player?.CharacterSlot?.index == null) return;

                var charIndex = player.CharacterSlot.index;

                var headerRootGO = __instance.m_headerRoot;

                var CM_Item = headerRootGO.GetComponent<CM_Item>();
                if (CM_Item == null)
                {
                    ArchiveLogger.Debug($"Setting up player name button (map) for index {player.CharacterSlot.index}");
                    var collider = headerRootGO.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2(400f, 40f);
                    collider.offset = new Vector2(-200f, 0f);

                    if (colliderMap.ContainsKey(charIndex))
                        colliderMap.Remove(charIndex);
                    colliderMap.Add(charIndex, collider);

                    CM_Item = headerRootGO.AddComponent<CM_Item>();
                    CM_Item.ID = player.CharacterSlot.index + 1; // +1

                    CM_Item.SetCMItemEvents(OnNameButtonPressed);
                }
            }
        }

        internal static void OnNameButtonPressed(int id)
        {
            if (!SharedUtils.TryGetPlayerByCharacterIndex(id - 1, out var player))
            {
                ArchiveLogger.Debug($"No player found for index {id - 1}.");
                return;
            }

            ArchiveLogger.Info($"Opening Steam profile for player \"{player.NickName}\" ({player.Lookup})");
            Application.OpenURL($"https://steamcommunity.com/profiles/{player.Lookup}");
        }
    }
}
