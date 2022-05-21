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
                    CM_Item.OnBtnPressCallback += OnNameButtonPressed;
                }
            }

            private static void OnNameButtonPressed(int id)
            {
                id = id - 1;

                try
                {
                    var player = SNet.Lobby.Players.FirstOrDefault(ply => ply.CharacterSlot.index == id);

                    if (player == null)
                    {
                        ArchiveLogger.Debug($"No player found.");
                        return;
                    }

                    ArchiveLogger.Info($"Opening Steam profile for player \"{player.NickName}\" ({player.Lookup})");
                    Application.OpenURL($"https://steamcommunity.com/profiles/{player.Lookup}");
                }
                catch (Exception)
                {
                    ArchiveLogger.Debug($"This shouldn't happen :skull:");
                }
            }
        }

    }
}
