using CellMenu;
using Gear;
using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Core;
using TheArchive.Core.Settings;
using TheArchive.Utilities;
using TMPro;
using UnityEngine;
using static TheArchive.Utilities.Utils;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableLoadoutRandomizer), "LoadoutRandomizer")]
    public class LoadoutRandomizerPatches
    {
        [ArchivePatch(typeof(CM_TimedButton), nameof(CM_TimedButton.SetupCMItem))]
        public class CM_TimedButton_SetupCMItemPatch
        {
            private static bool _hasFoundReadyButton = false;
            public static bool HasSetupLoadoutRandoButton { get; private set; } = false;
            private static LoadoutRandomizerSettings Settings { get; set; }
            public static CM_TimedButton LoadoutRandomizerButton { get; private set; }
            public static List<CM_PlayerLobbyBar> CM_PlayerLobbyBarInstances { get; private set; } = new List<CM_PlayerLobbyBar>();
            public static void Postfix(CM_TimedButton __instance)
            {
                try
                {
                    if (!_hasFoundReadyButton)
                    {
                        if (__instance.transform.parent != null && __instance.transform.parent.name == "ReadyButtonAlign")
                        {
                            ArchiveLogger.Notice("CM_TimedButton for readying up has been found!");
                            _hasFoundReadyButton = true;

                            var PlayerMovement = __instance.transform.parent.parent;

                            var playerPillars = PlayerMovement.GetChildWithExactName("PlayerPillars");

                            if(playerPillars == null)
                            {
                                ArchiveLogger.Error("PlayerPillars is null, this shouldn't happen!");
                            }

                            var localPlayerAgent = PlayerManager.GetLocalPlayerAgent();

                            playerPillars?.ForEachFirstChildDo((child) => {
                                var lobbyBar = child.GetComponentInChildren<CM_PlayerLobbyBar>(includeInactive: true);

                                CM_PlayerLobbyBarInstances.Add(lobbyBar);
                            });

                            LoadoutRandomizerButton = GameObject.Instantiate(__instance.gameObject, __instance.transform.parent).GetComponent<CM_TimedButton>();

                            MonoUtils.RemoveAllEventHandlers<CM_Item>(nameof(CM_TimedButton.OnBtnHoverChanged), LoadoutRandomizerButton);
                            MonoUtils.RemoveAllEventHandlers<CM_Item>(nameof(CM_TimedButton.OnBtnPressCallback), LoadoutRandomizerButton);
                            LoadoutRandomizerButton.OnBtnHoverChanged += OnButtonHoverChanged;
                            LoadoutRandomizerButton.OnBtnPressCallback += OnRandomizeLoadoutButtonPressed;

                            LoadoutRandomizerButton.gameObject.transform.Translate(new Vector3(-500,0,0));

                            LoadoutRandomizerButton.SetText("Randomize Loadout");

                            ChangeColor(LoadoutRandomizerButton, new Color(1, 1, 1, 0.5f));

                            Settings = LocalFiles.LoadConfig<LoadoutRandomizerSettings>();

                            LoadoutRandomizerButton.gameObject.SetActive(Settings.Enable);

                            HasSetupLoadoutRandoButton = true;
                        }
                    }
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }

            private static void OnUnreadyButtonPressed(int _)
            {
                LoadoutRandomizerButton.gameObject.SetActive(Settings.Enable);
            }

            public static void OnReadyUpButtonPressed(int _)
            {
                LoadoutRandomizerButton.gameObject.SetActive(false);
            }

            public static void ChangeColor(CM_TimedButton button, Color col)
            {
                button.transform.ForEachChildDo((child) => {
                    if (child.name.StartsWith("ProgressFill")) return;
                    var spriteRenderer = child.GetComponent<SpriteRenderer>();
                    if(spriteRenderer != null)
                    {
                        spriteRenderer.color = col;
                    }
                    var tmp = child.GetComponent<TextMeshPro>();
                    if(tmp != null)
                    {
                        tmp.color = col;
                    }
                });
            }

            private static readonly Dictionary<InventorySlot, LoadoutRandomizerSettings.InventorySlots> _invSlotMap = new Dictionary<InventorySlot, LoadoutRandomizerSettings.InventorySlots>
            {
                { GetEnumFromName<InventorySlot>(nameof(InventorySlot.GearMelee)), LoadoutRandomizerSettings.InventorySlots.Melee },
                { GetEnumFromName<InventorySlot>(nameof(InventorySlot.GearStandard)), LoadoutRandomizerSettings.InventorySlots.Primary },
                { GetEnumFromName<InventorySlot>(nameof(InventorySlot.GearSpecial)), LoadoutRandomizerSettings.InventorySlots.Special },
                { GetEnumFromName<InventorySlot>(nameof(InventorySlot.GearClass)), LoadoutRandomizerSettings.InventorySlots.Tool },
            };

            private static FieldInfo _CM_PlayerLobbyBar_m_player = typeof(CM_PlayerLobbyBar).GetField("m_player", AccessTools.all);
            private static FieldInfo _CM_PlayerLobbyBar_m_inventorySlotItems = typeof(CM_PlayerLobbyBar).GetField("m_inventorySlotItems", AccessTools.all);

            public static void OnRandomizeLoadoutButtonPressed(int _)
            {
                ArchiveLogger.Notice("Randomizer Button has been pressed!");
                try
                {
                    CM_PlayerLobbyBar LocalCM_PlayerLobbyBar = null;
                    foreach (var lobbyBar in CM_PlayerLobbyBarInstances)
                    {
                        var snet_player = ((SNetwork.SNet_Player)_CM_PlayerLobbyBar_m_player.GetValue(lobbyBar));

                        var agent = (PlayerAgent)snet_player?.PlayerAgent;

                        if (snet_player?.CharacterIndex == PlayerManager.GetLocalPlayerAgent()?.CharacterID)
                        {
                            LocalCM_PlayerLobbyBar = lobbyBar;
                        }
                    }

                    if (LocalCM_PlayerLobbyBar == null)
                    {
                        ArchiveLogger.Error($"Couldn't find the local players {nameof(CM_PlayerLobbyBar)}, aborting randomization of loadout!");
                        return;
                    }

                    foreach (var kvp in (Dictionary<InventorySlot, CM_InventorySlotItem>)_CM_PlayerLobbyBar_m_inventorySlotItems.GetValue(LocalCM_PlayerLobbyBar))
                    {
                        var slot = kvp.Key;

                        if (_invSlotMap.TryGetValue(slot, out var randoInvSlot) && Settings.ExcludedSlots.Contains(randoInvSlot))
                        {
                            // Skip if slot is excluded.
                            continue;
                        }

                        GearIDRange[] allGearForSlot = GearManager.GetAllGearForSlot(slot);

                        PlayerBackpackManager.LocalBackpack.TryGetBackpackItem(slot, out var itemForSlot);

                        var currentGearIdForSlot = itemForSlot?.GearIDRange;

                        GearIDRange gearID;
                        switch (Settings.Mode)
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
                catch(Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }

            public static void OnButtonHoverChanged(int i, bool b)
            {

            }
        }

    }
}
