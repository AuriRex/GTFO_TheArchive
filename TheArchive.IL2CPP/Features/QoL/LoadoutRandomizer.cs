using CellMenu;
using Gear;
using Player;
using SNetwork;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.QoL
{
    public class LoadoutRandomizer : Feature
    {
        public override string Name => "Loadout Randomizer";

        public override string Group => FeatureGroups.QualityOfLife;

        public class LoadoutRandomizerSettings
        {
            [FSDisplayName("Do not randomize")]
            public List<InventorySlots> ExcludedSlots { get; set; } = new List<InventorySlots>();
            public RandomizerMode Mode { get; set; } = RandomizerMode.NoDuplicate;

            public enum InventorySlots
            {
                Primary,
                Special,
                Tool,
                Melee
            }

            public enum RandomizerMode
            {
                True,
                NoDuplicate
            }
        }

        [FeatureConfig]
        public static LoadoutRandomizerSettings Config { get; set; }

        private static CM_TimedButton _loadoutRandomizerButton;
        private static bool _hasBeenSetup = false;
        private static eGameStateName _eGameStateName_Lobby;

        public static bool IsEnabled { get; set; }

#if MONO
        private static FieldAccessor<CM_PageLoadout, CM_TimedButton> A_CM_PageLoadout_m_readyButton;
        private static FieldAccessor<CM_PageLoadout, CM_Item> A_CM_PageLoadout_m_changeLoadoutButton;
        private static FieldAccessor<CM_PageLoadout, CM_PlayerLobbyBar[]> A_CM_PageLoadout_m_playerLobbyBars;
        private static FieldAccessor<CM_PlayerLobbyBar, SNet_Player> A_CM_PlayerLobbyBar_m_player;
        private static FieldAccessor<CM_PlayerLobbyBar, Dictionary<InventorySlot, CM_InventorySlotItem>> A_CM_PlayerLobbyBar_m_inventorySlotItems;
#endif

        public override void Init()
        {
#if MONO
            A_CM_PageLoadout_m_readyButton = FieldAccessor<CM_PageLoadout, CM_TimedButton>.GetAccessor("m_readyButton");
            if(BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownSix.ToLatest()))
            {
                A_CM_PageLoadout_m_changeLoadoutButton = FieldAccessor<CM_PageLoadout, CM_Item>.GetAccessor("m_changeLoadoutButton");
            }
            A_CM_PageLoadout_m_playerLobbyBars = FieldAccessor<CM_PageLoadout, CM_PlayerLobbyBar[]>.GetAccessor("m_playerLobbyBars");
            A_CM_PlayerLobbyBar_m_player = FieldAccessor<CM_PlayerLobbyBar, SNet_Player>.GetAccessor("m_player");
            A_CM_PlayerLobbyBar_m_inventorySlotItems = FieldAccessor <CM_PlayerLobbyBar, Dictionary<InventorySlot, CM_InventorySlotItem>>.GetAccessor("m_inventorySlotItems");
#endif
            _eGameStateName_Lobby = GetEnumFromName<eGameStateName>(nameof(eGameStateName.Lobby));
        }

        public override void OnEnable()
        {
            SharedUtils.RegisterOnGameStateChangedEvent(OnGameStateChanged);
            if (!_hasBeenSetup)
            {
                SetupViaInstance(CM_PageLoadout.Current);
            }
            if (SharedUtils.GetGameState() == _eGameStateName_Lobby)
            {
                SetButtonActive(0);
            }
        }

        public override void OnDisable()
        {
            SharedUtils.UnregisterOnGameStateChangedEvent(OnGameStateChanged);
            _loadoutRandomizerButton?.gameObject?.SetActive(false);
        }

        [ArchivePatch(typeof(CM_PageLoadout), nameof(CM_PageLoadout.Setup))]
        public static class CM_PageLoadout_SetupPatch
        {
            public static void Postfix(CM_PageLoadout __instance)
            {
                SetupViaInstance(__instance);
            }
        }

        public static void SetupViaInstance(CM_PageLoadout pageLoadout)
        {
            if (pageLoadout == null) return;
#if IL2CPP
            var readyUpButton = pageLoadout.m_readyButton;
#else
            var readyUpButton = A_CM_PageLoadout_m_readyButton.Get(pageLoadout);
#endif

            CM_Item changeLoadoutButton = null;
            if (BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownSix.ToLatest()))
                GetChangeLoadoutButtonR6Plus(pageLoadout, out changeLoadoutButton);


            SetupButton(readyUpButton, changeLoadoutButton);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GetChangeLoadoutButtonR6Plus(CM_PageLoadout pageLoadout, out CM_Item changeLoadoutButton)
        {
#if IL2CPP
            changeLoadoutButton = pageLoadout.m_changeLoadoutButton;
#else
            changeLoadoutButton = A_CM_PageLoadout_m_changeLoadoutButton.Get(pageLoadout);
#endif
        }


        private static void SetupButton(CM_TimedButton readyUpButton, CM_Item changeLoadoutButton = null)
        {
            if (_hasBeenSetup) return;

            if (BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownSix.ToLatest()))
            {
                readyUpButton.AddCMItemEvents(SetButtonInactive);
                changeLoadoutButton?.AddCMItemEvents(SetButtonActive);
            }

            CreateButton(readyUpButton);

            _hasBeenSetup = true;
        }

        private static CM_TimedButton CreateButton(CM_TimedButton prefab)
        {
            _loadoutRandomizerButton = GameObject.Instantiate(prefab.gameObject, prefab.transform.parent).GetComponent<CM_TimedButton>();

            _loadoutRandomizerButton.SetCMItemEvents(OnRandomizeLoadoutButtonPressed, OnButtonHoverChanged);

            _loadoutRandomizerButton.gameObject.transform.Translate(new Vector3(-500, 0, 0));

            var collider = _loadoutRandomizerButton.GetComponent<BoxCollider2D>();

            if(collider != null)
            {
                collider.offset = new Vector2(0, -40);
            }

            _loadoutRandomizerButton.SetText("Randomize Loadout");

            SharedUtils.ChangeColorTimedExpeditionButton(_loadoutRandomizerButton, new Color(1, 1, 1, 0.5f));

            SetButtonActive(0);

            return _loadoutRandomizerButton;
        }

        private static void OnGameStateChanged(eGameStateName state)
        {
            if (state == _eGameStateName_Lobby)
            {
                SetButtonActive(0);
            }
            else
            {
                SetButtonInactive(0);
            }
        }

        private static void SetButtonActive(int _)
        {
            _loadoutRandomizerButton?.gameObject?.SetActive(IsEnabled);
        }

        public static void SetButtonInactive(int _)
        {
            _loadoutRandomizerButton?.gameObject?.SetActive(false);
        }

        private static readonly Dictionary<InventorySlot, LoadoutRandomizerSettings.InventorySlots> _invSlotMap = new Dictionary<InventorySlot, LoadoutRandomizerSettings.InventorySlots>
            {
                { GetEnumFromName<InventorySlot>(nameof(InventorySlot.GearMelee)), LoadoutRandomizerSettings.InventorySlots.Melee },
                { GetEnumFromName<InventorySlot>(nameof(InventorySlot.GearStandard)), LoadoutRandomizerSettings.InventorySlots.Primary },
                { GetEnumFromName<InventorySlot>(nameof(InventorySlot.GearSpecial)), LoadoutRandomizerSettings.InventorySlots.Special },
                { GetEnumFromName<InventorySlot>(nameof(InventorySlot.GearClass)), LoadoutRandomizerSettings.InventorySlots.Tool },
            };

        public static void OnRandomizeLoadoutButtonPressed(int _)
        {
            ArchiveLogger.Notice("Randomizer Button has been pressed!");
            CM_PlayerLobbyBar LocalCM_PlayerLobbyBar = null;
#if IL2CPP
            LocalCM_PlayerLobbyBar = CM_PageLoadout.Current.m_playerLobbyBars.ToArray()
                .FirstOrDefault(plb => plb.m_player?.CharacterIndex == PlayerManager.GetLocalPlayerAgent()?.CharacterID);
#else
            LocalCM_PlayerLobbyBar = A_CM_PageLoadout_m_playerLobbyBars.Get(CM_PageLoadout.Current)
                .FirstOrDefault(plb => A_CM_PlayerLobbyBar_m_player.Get(plb)?.CharacterIndex == PlayerManager.GetLocalPlayerAgent()?.CharacterID);
#endif
            
            if (LocalCM_PlayerLobbyBar == null)
            {
                ArchiveLogger.Error($"Couldn't find the local players {nameof(CM_PlayerLobbyBar)}, aborting randomization of loadout!");
                return;
            }

#if IL2CPP
            foreach (var kvp in LocalCM_PlayerLobbyBar.m_inventorySlotItems)
#else
            foreach (var kvp in A_CM_PlayerLobbyBar_m_inventorySlotItems.Get(LocalCM_PlayerLobbyBar))
#endif
            {
                var slot = kvp.Key;

                if (_invSlotMap.TryGetValue(slot, out var randoInvSlot) && Config.ExcludedSlots.Contains(randoInvSlot))
                {
                    // Skip if slot is excluded.
                    continue;
                }

                GearIDRange[] allGearForSlot = GearManager.GetAllGearForSlot(slot);

                PlayerBackpackManager.LocalBackpack.TryGetBackpackItem(slot, out var itemForSlot);

                var currentGearIdForSlot = itemForSlot.GearIDRange;

                GearIDRange gearID;
                switch (Config.Mode)
                {
                    case LoadoutRandomizerSettings.RandomizerMode.NoDuplicate:
                        gearID = allGearForSlot.PickRandomExcept((random) => {
                            return !currentGearIdForSlot.GetChecksum().Equals(random.GetChecksum());
                        });
                        break;
                    default:
                    case LoadoutRandomizerSettings.RandomizerMode.True:
                        gearID = allGearForSlot.PickRandom();
                        break;
                }


                if (gearID == null)
                {
                    ArchiveLogger.Error($"Tried to randomize Gear for slot {slot} but received null!");
                    continue;
                }

                ArchiveLogger.Notice($"Picked random gear \"{gearID.PublicGearName}\" for slot {slot}!");

                PlayerBackpackManager.ResetLocalAmmoStorage();
                PlayerBackpackManager.EquipLocalGear(gearID);
                GearManager.RegisterGearInSlotAsEquipped(gearID.PlayfabItemInstanceId, slot);
            }

        }

        public static void OnButtonHoverChanged(int i, bool b)
        {

        }
    }
}
