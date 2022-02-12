using CellMenu;
using Gear;
using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Utilities;
using TMPro;
using UnityEngine;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting("EnableLoadoutRandomizer", "LoadoutRandomizer")]
    public class CM_TimedButtonPatches
    {
        [ArchivePatch(typeof(CM_TimedButton), nameof(CM_TimedButton.SetupCMItem))]
        public class CM_TimedButton_SetupCMItemPatch
        {
            private static bool _hasFoundReadyButton = false;
            public static bool HasSetupLoadoutRandoButton { get; private set; } = false;
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

                            LoadoutRandomizerButton.gameObject.SetActive(true);

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
                LoadoutRandomizerButton.gameObject.SetActive(true);
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

            private static FieldInfo _CM_PlayerLobbyBar_m_player = typeof(CM_PlayerLobbyBar).GetField("m_player", AccessTools.all);
            private static FieldInfo _CM_PlayerLobbyBar_m_inventorySlotItems = typeof(CM_PlayerLobbyBar).GetField("m_inventorySlotItems", AccessTools.all);

            public static void OnRandomizeLoadoutButtonPressed(int _)
            {
                ArchiveLogger.Notice("Randomizer Button has been pressed!");
                CM_PlayerLobbyBar LocalCM_PlayerLobbyBar = null;
                foreach (var lobbyBar in CM_PlayerLobbyBarInstances)
                {
                    var snet_player = ((SNetwork.SNet_Player) _CM_PlayerLobbyBar_m_player.GetValue(lobbyBar));

                    var agent = (PlayerAgent) snet_player?.PlayerAgent;

                    if (snet_player?.CharacterIndex == PlayerManager.GetLocalPlayerAgent()?.CharacterID)
                    {
                        LocalCM_PlayerLobbyBar = lobbyBar;
                    }
                }

                if(LocalCM_PlayerLobbyBar == null)
                {
                    ArchiveLogger.Error($"Couldn't find the local players {nameof(CM_PlayerLobbyBar)}, aborting randomization of loadout!");
                    return;
                }

                foreach (var kvp in (Dictionary<InventorySlot, CM_InventorySlotItem>) _CM_PlayerLobbyBar_m_inventorySlotItems.GetValue(LocalCM_PlayerLobbyBar))
                {
                    var slot = kvp.Key;
                    GearIDRange[] allGearForSlot = GearManager.GetAllGearForSlot(slot);

                    var gearID = allGearForSlot.PickRandom();

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
}
