using CellMenu;
using Player;
using SNetwork;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Models;
using TheArchive.Features.Accessibility;
using TheArchive.Features.Dev;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Security
{
    [EnableFeatureByDefault]
    public class PlayerLobbyManagement : Feature
    {
        public override string Name => "Player Lobby Management";

        public override string Group => FeatureGroups.Security;

        public override string Description => "Allows you to open a players steam profile by clicking on their name as well as kick and ban players as host.";

        public new static IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static LobbyManagementSettings Settings { get; set; }

        public class LobbyManagementSettings
        {
            [FSDisplayName("Names on Map open Steam Profile")]
            [FSDescription("If clicking a players name on the map screen should open their Steam Profile in your default browser.")]
            public bool NamesOnMapOpenSteamProfile { get; set; } = false;

            [FSDisplayName("Open Profiles in Steam Overlay")]
            [FSDescription("Wheter to open profile links in the overlay or in the default OS browser.")]
            public bool PreferOpeningProfileLinksInSteamOverlay { get; set; } = true;

            [FSHeader("Lobby Color Settings")]
            public LobbyColorSettings LobbyColors { get; set; } = new LobbyColorSettings();

            [FSHeader("Other")]
            [FSDisplayName("Recently Played With")]
            public List<RecentlyPlayedWithEntry> RecentlyPlayedWith { get; set; } = new List<RecentlyPlayedWithEntry>();

            [FSDisplayName("Banned Players")]
            [FSDescription("Players who are on this list will not be able to join any games <b>you host</b>.")]
            public List<BanListEntry> BanList { get; set; } = new List<BanListEntry>();

            public class LobbyColorSettings
            {
                [FSDisplayName("Color the Square")]
                [FSDescription("Use the colors/settings below to color the square on the loadout screen next to the player names.")]
                public bool ColorizeLobbyBullet { get; set; } = true;

                [FSDisplayName("Use Nickname Color for Self")]
                public bool UseNicknameColorForSelf { get; set; } = true;

                // [FSDisplayName("Use Nickname Color for Others")]
                // public bool UseNicknameColorForOthers { get; set; } = false;

                [FSDisplayName("Random Colors for Self")]
                public bool RainbowPukeSelf { get; set; } = false;

                [FSDisplayName("Random Colors for Others")]
                public bool RainbowPukeFriends { get; set; } = false;

                [FSHeader("Colors")]
                public SColor Default { get; set; } = new SColor(1, 1, 1);
                public SColor Friends { get; set; } = new SColor(0.964f, 0.921f, 0.227f);
                public SColor Bots { get; set; } = new SColor(0.949f, 0.58f, 0f);
                public SColor Banned { get; set; } = new SColor(0.545f, 0f, 0f);
                public SColor Self { get; set; } = new SColor(1, 1, 1);

            }

            public class BanListEntry
            {
                [FSSeparator]
                [FSReadOnly]
                public string Name { get; set; }
                [FSReadOnly]
                public ulong SteamID { get; set; }
                [FSReadOnly]
                [FSTimestamp]
                [FSDisplayName("Banned on:")]
                public long Timestamp { get; set; }
            }

            public class RecentlyPlayedWithEntry
            {
                [FSSeparator]
                [FSReadOnly]
                public string Name { get; set; }
                [FSReadOnly]
                public ulong SteamID { get; set; }
                [FSReadOnly]
                [FSTimestamp]
                [FSDisplayName("First time played with:")]
                public long TimestampFirst { get; set; }
                [FSReadOnly]
                [FSTimestamp]
                [FSDisplayName("Last time played with:")]
                public long TimestampLast { get; set; }
            }
        }

        public enum PlayerRelationShip
        {
            None,
            Self,
            Friend,
            Banned,
            Bot,
        }

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

            SetupColorMap();
        }

        public override void OnDisable()
        {
            foreach (var collider in CM_PlayerLobbyBar_UpdatePlayer_Patch.colliderMap.Values)
            {
                if (collider != null)
                    collider.enabled = false;
            }
            foreach (var collider in PUI_Inventory_UpdateAllSlots_Patch.colliderMap.Values)
            {
                if (collider != null)
                    collider.enabled = false;
            }
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            SetupColorMap();
        }

        private static Dictionary<PlayerRelationShip, Color> _colorMap = new Dictionary<PlayerRelationShip, Color>();

        public static void SetupColorMap()
        {
            _colorMap.Clear();

            var rels = Enum.GetValues(typeof(PlayerRelationShip)) as PlayerRelationShip[];

            foreach (var rel in rels)
            {
                _colorMap.Add(rel, GetRelationshipColor(rel));
            }
        }

        public static PlayerRelationShip GetRelationship(PlayerAgent player) => GetRelationship(player?.Owner);
        public static PlayerRelationShip GetRelationship(SNet_Player player)
        {
            if (player == null)
                return PlayerRelationShip.None;

            if (player.SafeIsBot())
                return PlayerRelationShip.Bot;

            if (player.IsLocal)
                return PlayerRelationShip.Self;

            if (IsPlayerBanned(player.Lookup))
                return PlayerRelationShip.Banned;

            if (player.IsFriend())
                return PlayerRelationShip.Friend;

            return PlayerRelationShip.None;
        }

        public static Color GetRelationshipColor(PlayerRelationShip rel)
        {
            if (_colorMap.TryGetValue(rel, out var color))
            {
                return color;
            }

            switch (rel)
            {
                default:
                case PlayerRelationShip.None:
                    return Settings.LobbyColors.Default.ToUnityColor();
                case PlayerRelationShip.Self:
                    return Settings.LobbyColors.Self.ToUnityColor();
                case PlayerRelationShip.Bot:
                    return Settings.LobbyColors.Bots.ToUnityColor();
                case PlayerRelationShip.Friend:
                    return Settings.LobbyColors.Friends.ToUnityColor();
                case PlayerRelationShip.Banned:
                    return Settings.LobbyColors.Banned.ToUnityColor();

            }
        }

        public static bool IsPlayerBanned(ulong lookup)
        {
            return Settings.BanList.Any(bp => bp.SteamID == lookup);
        }

        private static CM_ScrollWindow _popupWindow = null;
        private static CM_ScrollWindow PopupWindow
        {
            get
            {
                if (_popupWindow == null)
                {
                    FeatureLogger.Debug("Creating PopupWindow ...");

                    _popupWindow = UnityEngine.Object.Instantiate(ModSettings.PageSettingsData.PopupWindow, CM_PageLoadout.Current.m_movingContentHolder);
                    UnityEngine.Object.DontDestroyOnLoad(_popupWindow);
                    _popupWindow.name = $"{nameof(PlayerLobbyManagement)}_{nameof(PopupWindow)}_PlayerManagement";

                    PopupWindow.Setup();

                    OpenSteamItem = CreatePopupItem(" Open Steam Profile", OnNameButtonPressed);
                    SharedUtils.ChangeColorCMItem(OpenSteamItem, ModSettings.GREEN);
                    OpenSteamItem.TryCastTo<CM_TimedButton>().SetHoldDuration(.5f);

                    IsFriendItem = CreatePopupItem(" Friends on Steam", (_) => { });
                    SharedUtils.ChangeColorCMItem(IsFriendItem, GetRelationshipColor(PlayerRelationShip.Friend));
                    IsFriendItem.TryCastTo<CM_TimedButton>().SetHoldDuration(100f);
                    IsFriendItem.GetComponent<Collider2D>().enabled = false;

                    KickPlayerItem = CreatePopupItem(" Kick player", KickPlayerButtonPressed);
                    SharedUtils.ChangeColorCMItem(KickPlayerItem, ModSettings.ORANGE);
                    KickPlayerItem.TryCastTo<CM_TimedButton>().SetHoldDuration(2);

                    BanPlayerItem = CreatePopupItem(" Ban player", BanPlayerButtonPressed);
                    SharedUtils.ChangeColorCMItem(BanPlayerItem, ModSettings.RED);
                    BanPlayerItem.TryCastTo<CM_TimedButton>().SetHoldDuration(4);


                    var spacer1 = CreatePopupItem("Spacer, should not be visible", (_) => { });

                    var list = SharedUtils.NewListForGame<iScrollWindowContent>();

                    list.Add(OpenSteamItem.GetComponentInChildren<iScrollWindowContent>());
                    list.Add(IsFriendItem.GetComponentInChildren<iScrollWindowContent>());
                    list.Add(spacer1.GetComponentInChildren<iScrollWindowContent>());
                    list.Add(KickPlayerItem.GetComponentInChildren<iScrollWindowContent>());
                    list.Add(BanPlayerItem.GetComponentInChildren<iScrollWindowContent>());

                    PopupWindow.SetContentItems(list, 5f);

                    spacer1.gameObject.SetActive(false);
                    spacer1.transform.position = new Vector3(-5000, 0, 0);

                    PopupWindow.SetSize(new Vector2(350, 58 * list.Count /*240*/));
                    PopupWindow.SetVisible(false);
                    PopupWindow.SetHeader("Player options");
                }
                return _popupWindow;
            }
        }

        internal static void OnMapNameButtonPressed(int id)
        {
            if (!Settings.NamesOnMapOpenSteamProfile)
                return;

            OnNameButtonPressed(id);
        }

        internal static void OnNameButtonPressed(int id)
        {
            PopupWindow.SetVisible(false);

            if (!SharedUtils.TryGetPlayerByCharacterIndex(id - 1, out var player))
            {
                FeatureLogger.Debug($"No player found for index {id - 1}.");
                return;
            }

            FeatureLogger.Info($"Opening Steam profile for player \"{player.NickName}\" ({player.Lookup})");
            var profileUrl = $"https://steamcommunity.com/profiles/{player.Lookup}";

            if (Settings.PreferOpeningProfileLinksInSteamOverlay && SteamUtils.IsOverlayEnabled())
            {
                SteamFriends.ActivateGameOverlayToWebPage(profileUrl);
            }
            else
            {
                Application.OpenURL(profileUrl);
            }
        }

        internal static void KickPlayerButtonPressed(int playerID)
        {
            PopupWindow.SetVisible(false);

            if (!SNet.IsMaster) return;

            if (!SharedUtils.TryGetPlayerByCharacterIndex(playerID - 1, out var player))
            {
                return;
            }

            FeatureLogger.Notice($"Kicking player \"{player.GetName()}\" Nick:\"{player.NickName}\" ...");

            KickPlayer(player);
        }

        internal static void BanPlayerButtonPressed(int playerID)
        {
            if (!SharedUtils.TryGetPlayerByCharacterIndex(playerID - 1, out var player))
            {
                return;
            }

            if (player.IsLocal)
            {
                BanPlayerItem.SetText($" You can't ban yourself, silly!");
                return;
            }

            PopupWindow.SetVisible(false);

            if (!IsPlayerBanned(player.Lookup))
            {
                BanPlayer(player);
            }
            else
            {
                UnbanPlayer(player);
            }
        }

        public static bool BanPlayer(SNet_Player player, bool kickPlayer = true)
        {
            if (player == null)
                return false;

            if (!IsPlayerBanned(player.Lookup))
            {
                Settings.BanList.Add(new LobbyManagementSettings.BanListEntry
                {
                    Name = player.GetName(),
                    SteamID = player.Lookup,
                    Timestamp = DateTime.UtcNow.Ticks
                });
                FeatureLogger.Fail($"Player has been added to list of banned players: Name:\"{player.GetName()}\" SteamID:\"{player.Lookup}\"");

                if (kickPlayer)
                    KickPlayer(player);
                return true;
            }

            return false;
        }

        public static bool UnbanPlayer(SNet_Player player) => UnbanPlayer(player?.Lookup ?? ulong.MaxValue);

        public static bool UnbanPlayer(ulong playerID)
        {
            if (playerID == ulong.MaxValue)
                return false;

            var playerToUnban = Settings.BanList.FirstOrDefault(entry => entry.SteamID == playerID);
            if (playerToUnban != null)
            {
                Settings.BanList.Remove(playerToUnban);
                FeatureLogger.Success($"Player has been removed from the list of banned players: Name:\"{playerToUnban.Name}\" SteamID:\"{playerToUnban.SteamID}\"");
                return true;
            }

            return false;
        }

        public static void KickPlayer(SNet_Player player)
        {
            if (!SNet.IsMaster)
                return;

            SNet.Sync.KickPlayer(player, SNet_PlayerEventReason.Kick_ByVote);
            //SNet.SessionHub.RemovePlayerFromSession(player, true);
        }

        internal static CM_Item OpenSteamItem { get; set; }
        internal static CM_Item IsFriendItem { get; set; }
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

            bool isFriend = SNet.Friends.TryGetFriend(player.Lookup, out _);
            bool isBanned = IsPlayerBanned(player.Lookup);

            IsFriendItem.gameObject.SetActive(isFriend || isBanned);

            if (isBanned)
            {
                IsFriendItem.SetText(" On Banned Players List");
                SharedUtils.ChangeColorCMItem(IsFriendItem, GetRelationshipColor(PlayerRelationShip.Banned));
            }
            else if (isFriend)
            {
                IsFriendItem.SetText(" Friends on Steam");
                SharedUtils.ChangeColorCMItem(IsFriendItem, GetRelationshipColor(PlayerRelationShip.Friend));
            }


            KickPlayerItem.SetText($" Kick {name}");



            if (player.IsMaster || !SNet.IsMaster)
            {
                KickPlayerItem.GetComponent<Collider2D>().enabled = false;
                SharedUtils.ChangeColorCMItem(KickPlayerItem, ModSettings.DISABLED);

                BanPlayerItem.SetText($" Ban {name}");
            }
            else
            {
                KickPlayerItem.GetComponent<Collider2D>().enabled = true;
                SharedUtils.ChangeColorCMItem(KickPlayerItem, ModSettings.ORANGE);

                BanPlayerItem.SetText($" Ban and kick {name}");
            }

            if (player.IsLocal)
            {
                SharedUtils.ChangeColorCMItem(BanPlayerItem, ModSettings.DISABLED);
            }
            else
            {
                if (isBanned)
                {
                    BanPlayerItem.SetText($" Unban {name}");
                    SharedUtils.ChangeColorCMItem(BanPlayerItem, ModSettings.GREEN);
                }
                else
                {
                    SharedUtils.ChangeColorCMItem(BanPlayerItem, ModSettings.RED);
                }
            }

            PopupWindow.SetHeader(name);

            PopupWindow.ID = playerID;
            PopupWindow.transform.position = pos.position + new Vector3(200, 0, 0);

            OpenSteamItem.ID = playerID;
            KickPlayerItem.ID = playerID;
            BanPlayerItem.ID = playerID;

            PopupWindow.SetVisible(true);
        }

        private static void AddRecentlyPlayedWith(SNet_Player player)
        {
            var entry = Settings.RecentlyPlayedWith.FirstOrDefault(entry => entry.SteamID == player.Lookup);
            if (entry != null)
            {
                FeatureLogger.Notice($"{player.NickName} joined Session, last time played with them {DateTime.UtcNow - new DateTime(entry.TimestampLast):d' day(s) and 'hh':'mm':'ss} ago.");
                entry.TimestampLast = DateTime.UtcNow.Ticks;
                return;
            }

            var ticks = DateTime.UtcNow.Ticks;
            Settings.RecentlyPlayedWith.Add(new LobbyManagementSettings.RecentlyPlayedWithEntry
            {
                Name = player.NickName,
                SteamID = player.Lookup,
                TimestampFirst = ticks,
                TimestampLast = ticks,
            });
        }

        [ArchivePatch(typeof(SNet_SessionHub), "SlaveWantsToJoin")]
        internal static class SNet_SessionHub_SlaveWantsToJoin_Patch
        {
            public static bool Prefix(SNet_Player player)
            {
                if (SNet.IsMaster)
                {
                    if (IsPlayerBanned(player.Lookup))
                    {
                        FeatureLogger.Notice($"Banned player \"{player.GetName()}\" tried to join, their join request has been ignored!");
                        KickPlayer(player);
                        return ArchivePatch.SKIP_OG;
                    }
                }

                return ArchivePatch.RUN_OG;
            }
        }

        [ArchivePatch(typeof(SNet_SyncManager), nameof(SNet_SyncManager.OnPlayerJoinedSessionHub))]
        internal static class SNet_OnPlayerJoinedSessionHub_Patch
        {
            public static void Postfix(SNet_Player player)
            {
                AddRecentlyPlayedWith(player);
            }
        }

        // Lobby button
        [ArchivePatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.UpdatePlayer))]
        internal static class CM_PlayerLobbyBar_UpdatePlayer_Patch
        {
            internal static Dictionary<int, BoxCollider2D> colliderMap = new Dictionary<int, BoxCollider2D>();

            private static MethodAccessor<GUIX_ElementSprite> A_Unregister;
            private static MethodAccessor<GUIX_ElementSprite> A_Start;
            private static IValueAccessor<GUIX_ElementSprite, GUIX_Layer> A_layer;

            public static void Init()
            {
                A_Start = MethodAccessor<GUIX_ElementSprite>.GetAccessor(UnityMessages.Start);
                A_Unregister = MethodAccessor<GUIX_ElementSprite>.GetAccessor("Unregister");
                A_layer = AccessorBase.GetValueAccessor<GUIX_ElementSprite, GUIX_Layer>("layer");
            }

            public static void ColorizeNickNameGUIX(GameObject nickNameGuix, PlayerRelationShip rel = PlayerRelationShip.None)
            {
                if (nickNameGuix == null)
                    return;

                foreach (var child in nickNameGuix.transform.DirectChildren())
                {
                    var guix = child.GetComponent<GUIX_ElementSprite>();
                    var spriteRenderer = child.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        if (!Settings.LobbyColors.ColorizeLobbyBullet)
                        {
                            spriteRenderer.color = Color.white;
                        }
                        else switch (rel)
                            {
                                case PlayerRelationShip.Self:
                                    if (Settings.LobbyColors.RainbowPukeSelf)
                                    {
                                        spriteRenderer.color = UnityEngine.Random.ColorHSV();
                                        break;
                                    }
                                    if (Settings.LobbyColors.UseNicknameColorForSelf && FeatureManager.IsFeatureEnabled<Nickname>() && Nickname.IsNicknameColorEnabled)
                                    {
                                        spriteRenderer.color = Nickname.CurrentNicknameColor;
                                        break;
                                    }
                                    goto default;
                                case PlayerRelationShip.Friend:
                                    if (Settings.LobbyColors.RainbowPukeFriends)
                                    {
                                        spriteRenderer.color = UnityEngine.Random.ColorHSV();
                                        break;
                                    }
                                    // TODO: Settings.LobbyColors.UseNicknameColorForOthers
                                    goto default;
                                default:
                                    spriteRenderer.color = GetRelationshipColor(rel);
                                    break;
                            }
                    }

                    if (A_layer.Get(guix) == null || !guix.gameObject.activeInHierarchy)
                        continue;

                    // Unregister and invoke Start again to re-grab the sprite renderers color
                    A_Unregister.Invoke(guix);
                    guix.DynamicIndex = -1;
                    A_layer.Set(guix, null);
                    A_Start.Invoke(guix);
                }
            }

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
                        if (!PopupWindow.IsVisible || PopupWindow.ID != id)
                        {
                            SetupAndPlaceWindow(id, CM_Item.transform);
                        }
                        else
                        {
                            PopupWindow.SetVisible(false);
                        }
                    });

                }

                ColorizeNickNameGUIX(__instance.m_nickNameGuix, GetRelationship(player));
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

                    CM_Item.SetCMItemEvents(OnMapNameButtonPressed);
                }
            }
        }
    }
}
