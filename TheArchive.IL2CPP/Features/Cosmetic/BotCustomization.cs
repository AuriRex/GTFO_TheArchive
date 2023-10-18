using CellMenu;
using Gear;
using Player;
using SNetwork;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Cosmetic
{
    [RundownConstraint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
    public class BotCustomization : Feature
    {
        public override string Name => "Bot Customization";

        public override string Group => FeatureGroups.Cosmetic;

        public override string Description => "Customize your bots - Change their name and Vanity\n\nAdds the Apparel button to bots if you're host.\n(Bot clothing only works if dropping from lobby atm!)";

        public override bool SkipInitialOnEnable => true;

        public new static IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static NamedBotsSettings Settings { get; set; }

        public class NamedBotsSettings
        {
            [FSMaxLength(25)]
            public string Woods { get; set; } = nameof(Woods);
            [FSMaxLength(25)]
            public string Dauda { get; set; } = nameof(Dauda);
            [FSMaxLength(25)]
            public string Hackett { get; set; } = nameof(Hackett);
            [FSMaxLength(25)]
            public string Bishop { get; set; } = nameof(Bishop);

            [FSHide]
            public VanitySettings VanityWoods { get; set; } = new VanitySettings();
            [FSHide]
            public VanitySettings VanityDauda { get; set; } = new VanitySettings();
            [FSHide]
            public VanitySettings VanityHackett { get; set; } = new VanitySettings();
            [FSHide]
            public VanitySettings VanityBishop { get; set; } = new VanitySettings();

            public VanitySettings GetVanity(int characterIndex)
            {
                switch (characterIndex % 4)
                {
                    default:
                    case 0:
                        return VanityWoods;
                    case 1:
                        return VanityDauda;
                    case 2:
                        return VanityHackett;
                    case 3:
                        return VanityBishop;
                }
            }

            public class VanitySettings
            {
                public uint Helmet { get; set; } = 0;
                public uint Torso { get; set; } = 0;
                public uint Legs { get; set; } = 0;
                public uint Backpack { get; set; } = 0;
                public uint Palette { get; set; } = 0;

#if IL2CPP
                public uint Get(ClothesType type)
                {
                    switch (type)
                    {
                        default:
                            return 0;
                        case ClothesType.Helmet:
                            return Helmet;
                        case ClothesType.Torso:
                            return Torso;
                        case ClothesType.Legs:
                            return Legs;
                        case ClothesType.Backpack:
                            return Backpack;
                        case ClothesType.Palette:
                            return Palette;
                    }
                }

                public void Set(ClothesType type, uint id)
                {
                    switch (type)
                    {
                        default:
                            return;
                        case ClothesType.Helmet:
                            Helmet = id;
                            return;
                        case ClothesType.Torso:
                            Torso = id;
                            return;
                        case ClothesType.Legs:
                            Legs = id;
                            return;
                        case ClothesType.Backpack:
                            Backpack = id;
                            return;
                        case ClothesType.Palette:
                            Palette = id;
                            return;
                    }
                }
#endif
            }
        }

#if IL2CPP
        public override void OnEnable()
        {
            SetAllBotNamesAndVanity();
        }

        public override void OnDisable()
        {
            if (IsApplicationQuitting)
                return;

            SetAllBotNamesAndVanity(setToDefault: true);
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            SetAllBotNamesAndVanity();
        }

        public static void SetAllBotNamesAndVanity(bool setToDefault = false)
        {
            if (!SNet.IsInLobby)
                return;

            foreach (var slot in SNet.Slots.PlayerSlots)
            {
                var player = slot.player;

                if (player == null || !player.HasPlayerAgent || !player.SafeIsBot())
                    continue;

                var agent = player.PlayerAgent.TryCastTo<PlayerAgent>();

                SetBotName(agent, setToDefault);
                SetVanity(agent, setToDefault);
            }
        }

        public static void SetBotName(PlayerAgent agent, bool setToDefault = false)
        {
            if (!SNet.IsMaster)
                return;

            if (!agent.Owner.SafeIsBot())
                return;

            string name;
            switch (agent.CharacterID)
            {
                default:
                case 0:
                    name = setToDefault ? nameof(NamedBotsSettings.Woods) : Settings.Woods;
                    break;
                case 1:
                    name = setToDefault ? nameof(NamedBotsSettings.Dauda) : Settings.Dauda;
                    break;
                case 2:
                    name = setToDefault ? nameof(NamedBotsSettings.Hackett) : Settings.Hackett;
                    break;
                case 3:
                    name = setToDefault ? nameof(NamedBotsSettings.Bishop) : Settings.Bishop;
                    break;
            }

            if (agent.Owner.NickName != name)
                agent.Owner.NickName = name;
        }

        private static ClothesType[] _vanity = new ClothesType[] {
            ClothesType.Helmet,
            ClothesType.Torso,
            ClothesType.Legs,
            ClothesType.Backpack,
            ClothesType.Palette
        };

        public static void SetVanity(PlayerAgent agent, bool setToDefault = false)
        {
            if (!SNet.IsMaster)
                return;

            if (!agent.Owner.SafeIsBot())
                return;

            if (!PlayerBackpackManager.TryGetBackpack(agent.Owner, out var backpack))
                return;

            var vanity = Settings.GetVanity(agent.Owner.CharacterIndex);

            foreach (var type in _vanity)
            {
                // ItemID 0 is considered default
                backpack.EquipVanityItem(type, setToDefault ? 0 : vanity.Get(type));
            }
        }

        public const string CLOTHES_BUTTON_NAME = $"{nameof(BotCustomization)}_ClothesButton";

        public static GameObject GetOrCreateClothesButton(CM_PlayerLobbyBar playerLobbyBar, bool create = true)
        {
            var newClothesButton = playerLobbyBar.m_clothesButton.transform.parent.GetChildWithExactName(CLOTHES_BUTTON_NAME)?.gameObject;

            if (!create)
                return newClothesButton;

            if (newClothesButton == null)
            {
                newClothesButton = UnityEngine.Object.Instantiate(playerLobbyBar.m_clothesButton.gameObject);
                newClothesButton.transform.SetParent(playerLobbyBar.m_clothesButton.transform.parent);
                newClothesButton.name = CLOTHES_BUTTON_NAME;
                newClothesButton.transform.position = playerLobbyBar.m_clothesButton.transform.position + new Vector3(0, 100);
                newClothesButton.transform.localScale = Vector3.one;

                var cmItem = newClothesButton.GetComponent<CM_LobbyScrollItem>();

                cmItem.SetCMItemEvents((_) => playerLobbyBar.ShowClothesSelect());
            }

            return newClothesButton;
        }


        [ArchivePatch(nameof(PlayerAIBot.Setup))]
        internal static class PlayerAIBot_Setup_Patch
        {
            public static Type Type() => typeof(PlayerAIBot);

            public static void Postfix(PlayerAIBot __instance, PlayerAgent agent)
            {
                SetBotName(agent);
                SetVanity(agent);
            }
        }

        [ArchivePatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.UpdatePlayer))]
        internal static class CM_PlayerLobbyBar_UpdatePlayer_Patch
        {
            public static void Postfix(CM_PlayerLobbyBar __instance, SNet_Player player)
            {
                var clothesButton = GetOrCreateClothesButton(__instance);

                if (player == null || !player.SafeIsBot())
                {
                    clothesButton.SetActive(false);
                    return;
                }

                clothesButton.SetActive(!GameStateManager.IsReady & SNet.IsMaster);
            }
        }

        #region cursed_workaround
        public static SNet_Player BotPlayerToSync { get; set; } = null;
        public static bool WantsToSyncBotPlayer { get; private set; } = false;

        [ArchivePatch(typeof(PlayerBackpackManager), nameof(PlayerBackpackManager.LocalBackpack), null, ArchivePatch.PatchMethodType.Getter)]
        public static class PlayerBackpackManager_LocalBackpack_Getter_Patch
        {
            public static bool Prefix(ref PlayerBackpack __result)
            {
                if (!WantsToSyncBotPlayer)
                    return ArchivePatch.RUN_OG;

                if (!BotPlayerToSync.SafeIsBot())
                    return ArchivePatch.RUN_OG;

                var player = BotPlayerToSync;
                if (!PlayerBackpackManager.Current.m_backpacks.ContainsKey(player.Lookup))
                {
                    PlayerBackpack value = new PlayerBackpack(player);
                    PlayerBackpackManager.Current.m_backpacks.Add(player.Lookup, value);
                }
                __result = PlayerBackpackManager.Current.m_backpacks[player.Lookup];
                return ArchivePatch.SKIP_OG;
            }
        }

        public class LocalPlayerOverride : IDisposable
        {
            private static LocalPlayerOverride _instance;
            private SNet_Player _localPlayer;

            public LocalPlayerOverride(SNet_Player botPlayer)
            {
                if (_instance != null)
                    FeatureLogger.Warning($"{nameof(LocalPlayerOverride)} used twice at the same time! This might cause issues!");

                _localPlayer = SNet.s_localPlayer;
                BotPlayerToSync = botPlayer;
                SNet.s_localPlayer = botPlayer;
                WantsToSyncBotPlayer = true;
                _instance = this;
            }

            public void Dispose()
            {
                //BotPlayerToSync = null;
                SNet.s_localPlayer = _localPlayer;
                WantsToSyncBotPlayer = false;
                _instance = null;
            }
        }

        public static void WithBotAsLocalInput(SNet_Player botPlayer, Action action)
        {
            // Backup local player and set the static instance of SNet.LocalPlayer's backing field to the bot player
            var localPlayer = SNet.s_localPlayer;
            BotPlayerToSync = botPlayer;
            SNet.s_localPlayer = botPlayer;
            WantsToSyncBotPlayer = true;

            action?.Invoke();

            //BotPlayerToSync = null;
            SNet.s_localPlayer = localPlayer;
            WantsToSyncBotPlayer = false;
        }

        #endregion cursed_workaround

        [ArchivePatch(typeof(PlayerBackpackManager), nameof(PlayerBackpackManager.ForceSyncBotInventory))]
        public static class PlayerBackpackManager_ForceSyncBotInventory_Patch
        {
            public static bool Prefix(SNet_Player botPlayer, SNet_Player toPlayer)
            {
                ForceSyncBotInventoryWithVanity(botPlayer, toPlayer);
                return ArchivePatch.SKIP_OG;
            }

            public static unsafe void ForceSyncBotInventoryWithVanity(SNet_Player botPlayer, SNet_Player toPlayer = null)
            {
                if (toPlayer != null && toPlayer.SafeIsBot())
                    return;

                if (!SNet.IsMaster)
                    return;

                if (botPlayer == null || !SNet.HasLocalPlayer || PlayerManager.GetLocalPlayerAgent() == null)
                    return;

                // FeatureLogger.Debug($"Attempting to sync bot player: {botPlayer.NickName}");

                WithBotAsLocalInput(botPlayer, () =>
                {
                    // A cursed workaround makes this method sync the bots inventory instead
                    PlayerBackpackManager.ForceSyncLocalInventory();
                });

                // Update player list again or else the last bot is gonna be highlighted (moved forwards) instead of you
                GuiManager.MainMenuLayer.PageLoadout.UpdatePlayerList();

                return;

                // This would work instead of the cursed patch bs above if the game would properly read (& send) the values set below ...

                /*PlayerBackpack backpack = PlayerBackpackManager.GetBackpack(botPlayer);

                pInventorySync data = new pInventorySync(LoaderWrapper.ClassInjector.DerivedConstructorPointer<pInventorySync>());

                // Turns out that the game just sends all Zeroes instead of the data we're actually trying to send here because IL2CPP bs ...
                // So we have to resort to drastic measures instead ...

                data.sourcePlayer.SetPlayer(botPlayer);

                data.gearStandard = PlayerBackpackManager.Current.GetGearRange(backpack, InventorySlot.GearStandard);
                data.gearSpecial = PlayerBackpackManager.Current.GetGearRange(backpack, InventorySlot.GearSpecial);
                data.gearClass = PlayerBackpackManager.Current.GetGearRange(backpack, InventorySlot.GearClass);
                data.gearMelee = PlayerBackpackManager.Current.GetGearRange(backpack, InventorySlot.GearMelee);
                data.gearHacking = PlayerBackpackManager.Current.GetGearRange(backpack, InventorySlot.HackingTool);

                data.vanityItemHelmet = backpack.GetBackpackVanityItem(ClothesType.Helmet);
                data.vanityItemTorso = backpack.GetBackpackVanityItem(ClothesType.Torso);
                data.vanityItemLegs = backpack.GetBackpackVanityItem(ClothesType.Legs);
                data.vanityItemBackpack = backpack.GetBackpackVanityItem(ClothesType.Backpack);
                data.vanityItemPalette = backpack.GetBackpackVanityItem(ClothesType.Palette);

                if (toPlayer != null)
                {
                    PlayerBackpackManager.Current.m_gearDetailsPacket.Send(data, SNet_ChannelType.SessionOrderCritical, toPlayer);
                }
                else
                {
                    PlayerBackpackManager.Current.m_gearDetailsPacket.Send(data, SNet_ChannelType.SessionOrderCritical);
                }

                if (backpack.TryGetBackpackItem(InventorySlot.GearClass, out var backpackItem))
                {
                    pInventoryItemStatus invItemStatus = new pInventoryItemStatus();
                    invItemStatus.sourcePlayer.SetPlayer(botPlayer);
                    invItemStatus.slot = InventorySlot.GearClass;
                    invItemStatus.status = backpackItem.Status;
                    PlayerBackpackManager.InventoryItemStatusChange(invItemStatus, toPlayer);
                }

                GuiManager.MainMenuLayer.PageLoadout.UpdatePlayerList();*/
            }
        }

        [ArchivePatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.HideLoadoutUI))]
        internal static class CM_PlayerLobbyBar_HideLoadoutUI_Patch
        {
            public static void Postfix(CM_PlayerLobbyBar __instance)
            {
                if (__instance.m_player == null || !__instance.m_player.SafeIsBot())
                    return;

                var botVanityButton = GetOrCreateClothesButton(__instance, create: false);

                botVanityButton?.gameObject?.SetActive(!GameStateManager.IsReady & SNet.IsMaster);
            }
        }

        // Patch to select the proper vanity items for bots in the popup window
        [ArchivePatch(typeof(PlayerBackpackManager), nameof(PlayerBackpackManager.GetLocalVanityItem))]
        internal static class PlayerBackpackManager_GetLocalVanityItem_Patch
        {
            public static SNet_Player PlayerToGetApparelSelectVanityFor { get; set; } = null;

            public static bool Prefix(ClothesType type, ref uint __result)
            {
                if (WantsToSyncBotPlayer)
                {
                    var backpackBot = PlayerBackpackManager.GetBackpack(BotPlayerToSync);
                    __result = backpackBot.GetBackpackVanityItem(type);
                    return ArchivePatch.SKIP_OG;
                }

                if (PlayerBackpackManager_ForceSyncLocalInventory_Patch.MethodExecuting
                    || PlayerToGetApparelSelectVanityFor == null
                    || !PlayerBackpackManager.TryGetBackpack(PlayerToGetApparelSelectVanityFor, out var backpack))
                {
                    return ArchivePatch.RUN_OG;
                }

                __result = backpack.GetBackpackVanityItem(type);
                return ArchivePatch.SKIP_OG;
            }
        }

        [ArchivePatch(typeof(PlayerBackpackManager), nameof(PlayerBackpackManager.ForceSyncLocalInventory))]
        internal static class PlayerBackpackManager_ForceSyncLocalInventory_Patch
        {
            public static bool MethodExecuting { get; private set; } = false;
            public static void Prefix()
            {
                MethodExecuting = true;
            }
            public static void Postfix()
            {
                MethodExecuting = false;
            }
        }

        [ArchivePatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.ShowClothesSelect))]
        internal static class CM_PlayerLobbyBar_ShowClothesSelect_Patch
        {
            public static bool PreventVanityItemFavoritesSaving { get; private set; }

            public static void Prefix(CM_PlayerLobbyBar __instance, out LocalPlayerOverride __state)
            {
                __state = null;

                if (__instance?.m_player == null)
                    return;

                if (__instance.m_player.SafeIsBot() || !__instance.m_player.IsLocal)
                {
                    PreventVanityItemFavoritesSaving = true;
                    __state = new LocalPlayerOverride(__instance.m_player);
                    PlayerBackpackManager_GetLocalVanityItem_Patch.PlayerToGetApparelSelectVanityFor = __instance.m_player;
                    return;
                }

                PreventVanityItemFavoritesSaving = false;
                PlayerBackpackManager_GetLocalVanityItem_Patch.PlayerToGetApparelSelectVanityFor = null;
            }

            public static void Postfix(LocalPlayerOverride __state)
            {
                if (__state != null)
                {
                    __state.Dispose();
                }
            }
        }

        [ArchivePatch(typeof(GearManager), nameof(GearManager.RegisterVanityItemAsEquipped))]
        internal static class GearManager_RegisterVanityItemAsEquipped_Patch
        {
            public static bool Prefix(uint vanityItemId, ClothesType type)
            {
                if (CM_PlayerLobbyBar_ShowClothesSelect_Patch.PreventVanityItemFavoritesSaving)
                {
                    var botPlayer = PlayerBackpackManager_GetLocalVanityItem_Patch.PlayerToGetApparelSelectVanityFor;
                    Settings.GetVanity(botPlayer.CharacterIndex).Set(type, vanityItemId);

                    return ArchivePatch.SKIP_OG;
                }

                return ArchivePatch.RUN_OG;
            }
        }

#endif
    }
}
