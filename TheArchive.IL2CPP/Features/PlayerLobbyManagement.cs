using CellMenu;
using SNetwork;
using System;
using System.Collections.Generic;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Features.Dev;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features
{
    [EnableFeatureByDefault]
    public class PlayerLobbyManagement : Feature
    {
        public override string Name => "Player Lobby Management";

        public new static IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static LobbyManagementSettings Settings { get; set; }

        public class LobbyManagementSettings
        {
            [FSDisplayName("Banned Players")]
            public List<BanListEntry> BanList { get; set; } = new List<BanListEntry>();

            public class BanListEntry
            {
                [FSReadOnly]
                public string Name { get; set; }
                [FSReadOnly]
                public ulong SteamID { get; set; }
                [FSReadOnly]
                public ulong Timestamp { get; set; }
            }
        }

#if MONO
        private static MethodAccessor<SNet_SyncManager> A_SNet_SyncManager_EjectPlayer;

        public override void Init()
        {
            A_SNet_SyncManager_EjectPlayer = MethodAccessor<SNet_SyncManager>.GetAccessor("EjectPlayer");
        }
#endif

        public override void OnEnable()
        {
            foreach (var collider in CM_PlayerLobbyBar_UpdatePlayer_Patch.colliderMap.Values)
            {
                if (collider != null)
                    collider.enabled = true;
            }
            foreach (var collider in PUI_Inventory_UpdateAllSlots_Patch.colliderMap.Values)
            {
                if (collider != null)
                    collider.enabled = true;
            }
        }

        public override void OnDisable()
        {
            foreach(var collider in CM_PlayerLobbyBar_UpdatePlayer_Patch.colliderMap.Values)
            {
                if(collider != null)
                    collider.enabled = false;
            }
            foreach (var collider in PUI_Inventory_UpdateAllSlots_Patch.colliderMap.Values)
            {
                if (collider != null)
                    collider.enabled = false;
            }
        }

        private static CM_ScrollWindow _popupWindow = null;
        private static CM_ScrollWindow PopupWindow
        {
            get
            {
                if(_popupWindow == null)
                {
                    FeatureLogger.Debug("Creating PopupWindow ...");

                    _popupWindow = GameObject.Instantiate(Dev.ModSettings.PageSettingsData.PopupWindow, CM_PageLoadout.Current.m_movingContentHolder);
                    GameObject.DontDestroyOnLoad(_popupWindow);
                    _popupWindow.name = $"{nameof(PlayerLobbyManagement)}_{nameof(PopupWindow)}_PlayerManagement";

                    PopupWindow.Setup();

                    OpenSteamItem = CreatePopupItem(" Open Steam Profile", OnNameButtonPressed);
                    SharedUtils.ChangeColorCMItem(OpenSteamItem, ModSettings.GREEN);
                    OpenSteamItem.TryCastTo<CM_TimedButton>().SetHoldDuration(.5f);

                    KickPlayerItem = CreatePopupItem(" Kick player", KickPlayerButtonPressed);
                    SharedUtils.ChangeColorCMItem(KickPlayerItem, ModSettings.ORANGE);
                    KickPlayerItem.TryCastTo<CM_TimedButton>().SetHoldDuration(2);

                    BanPlayerItem = CreatePopupItem(" Ban player", BanPlayerButtonPressed);
                    SharedUtils.ChangeColorCMItem(BanPlayerItem, ModSettings.RED);
                    BanPlayerItem.TryCastTo<CM_TimedButton>().SetHoldDuration(4);


                    var spacer1 = CreatePopupItem("Spacer, should not be visible", (_) => { });

                    var list = SharedUtils.NewListForGame<iScrollWindowContent>();

                    list.Add(OpenSteamItem.GetComponentInChildren<iScrollWindowContent>());
                    list.Add(spacer1.GetComponentInChildren<iScrollWindowContent>());
                    list.Add(KickPlayerItem.GetComponentInChildren<iScrollWindowContent>());
                    list.Add(BanPlayerItem.GetComponentInChildren<iScrollWindowContent>());

                    PopupWindow.SetContentItems(list, 5f);

                    spacer1.gameObject.SetActive(false);
                    spacer1.transform.position = new Vector3(-5000, 0, 0);

                    PopupWindow.SetSize(new Vector2(350, 240));
                    PopupWindow.SetVisible(false);
                    PopupWindow.SetHeader("Player options");
                }
                return _popupWindow;
            }
        }

        internal static void OnNameButtonPressed(int id)
        {
            if (!SharedUtils.TryGetPlayerByCharacterIndex(id - 1, out var player))
            {
                FeatureLogger.Debug($"No player found for index {id - 1}.");
                return;
            }

            FeatureLogger.Info($"Opening Steam profile for player \"{player.NickName}\" ({player.Lookup})");
            Application.OpenURL($"https://steamcommunity.com/profiles/{player.Lookup}");
        }

        internal static void KickPlayerButtonPressed(int playerID)
        {
            if (!SNet.IsMaster) return;

            if (!SharedUtils.TryGetPlayerByCharacterIndex(playerID - 1, out var player))
            {
                return;
            }

            FeatureLogger.Notice($"Kicking player \"{player.GetName()}\" Nick:\"{player.NickName}\" ...");

#if IL2CPP
            SNet.Sync.EjectPlayer(player, SNet_PlayerEventReason.Kick_ByVote);
#else
            A_SNet_SyncManager_EjectPlayer.Invoke(SNet.Sync, player, SNet_PlayerEventReason.Kick_ByVote);
#endif
        }

        internal static void BanPlayerButtonPressed(int playerID)
        {
            FeatureLogger.Notice("Banning player ... (Not implemented yet!)");

#warning TODO: Implement ban list or something
        }

        internal static CM_Item OpenSteamItem { get; set; }
        internal static CM_Item KickPlayerItem { get; set; }
        internal static CM_Item BanPlayerItem { get; set; }

        private static CM_Item CreatePopupItem(string text, Action<int> onClick, int id = 0)
        {
            var item = GOUtil.SpawnChildAndGetComp<iScrollWindowContent>(ModSettings.UIHelper.PopupItemPrefab, _popupWindow.transform);

            var cm_Item = item.TryCastTo<CM_Item>();

            cm_Item.SetupCMItem();

            if (id != 0)
                cm_Item.ID = id;

            cm_Item.SetText(text);

            cm_Item.SetCMItemEvents(onClick);

            cm_Item.ForcePopupLayer(true);

            return cm_Item;
        }

        public static void SetupAndPlaceWindow(int playerID, Transform pos)
        {
            if (!SharedUtils.TryGetPlayerByCharacterIndex(playerID - 1, out var player))
            {
                return;
            }

            var name = player.GetName();

#if IL2CPP
            PopupWindow.SetupFromButton(new iCellMenuPopupController(CM_PageLoadout.Current.Pointer), CM_PageLoadout.Current);
#else
            PopupWindow.SetupFromButton(CM_PageLoadout.Current, CM_PageLoadout.Current);
#endif

            KickPlayerItem.SetText($" Kick {name}");
            BanPlayerItem.SetText($" Ban {name}");

            if (player.IsMaster || !SNet.IsMaster)
            {
                KickPlayerItem.GetComponent<Collider2D>().enabled = false;
                SharedUtils.ChangeColorCMItem(KickPlayerItem, ModSettings.DISABLED);

                BanPlayerItem.GetComponent<Collider2D>().enabled = false;
                SharedUtils.ChangeColorCMItem(BanPlayerItem, ModSettings.DISABLED);
            }
            else
            {
                KickPlayerItem.GetComponent<Collider2D>().enabled = true;
                SharedUtils.ChangeColorCMItem(KickPlayerItem, ModSettings.ORANGE);

                BanPlayerItem.GetComponent<Collider2D>().enabled = true;
                SharedUtils.ChangeColorCMItem(BanPlayerItem, ModSettings.RED);
            }

            PopupWindow.SetHeader(name);

            PopupWindow.ID = playerID;
            PopupWindow.transform.position = pos.position + new Vector3(200, 0, 0);

            OpenSteamItem.ID = playerID;
            KickPlayerItem.ID = playerID;
            BanPlayerItem.ID = playerID;

            PopupWindow.SetVisible(true);
        }

        // Lobby button
        [ArchivePatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.UpdatePlayer))]
        internal static class CM_PlayerLobbyBar_UpdatePlayer_Patch
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
                    FeatureLogger.Debug($"Setting up player name button for index {player.CharacterSlot.index}");
                    var collider = nameGO.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2(447.2f, 52.8f);
                    collider.offset = new Vector2(160f, 1.6f);

                    if (colliderMap.ContainsKey(charIndex))
                        colliderMap.Remove(charIndex);
                    colliderMap.Add(charIndex, collider);

                    CM_Item = nameGO.AddComponent<CM_Item>();
                    CM_Item.ID = charIndex + 1; // +1

                    CM_Item.SetCMItemEvents((id) =>
                    {
                        if(!PopupWindow.IsVisible || PopupWindow.ID != id)
                        {
                            SetupAndPlaceWindow(id, CM_Item.transform);
                        }
                        else
                        {
                            PopupWindow.SetVisible(false);
                        }
                    });

                }
            }
        }

        // In expedition map buttons
        [ArchivePatch(typeof(PUI_Inventory), nameof(PUI_Inventory.UpdateAllSlots))]
        internal static class PUI_Inventory_UpdateAllSlots_Patch
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
                    FeatureLogger.Debug($"Setting up player name button (map) for index {player.CharacterSlot.index}");
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
    }
}
