using AK;
using CellMenu;
using GameData;
using Gear;
using LevelGeneration;
using System;
using System.Reflection;
using System.Text;
using TheArchive.Core.Core;
using TheArchive.Managers;
using TheArchive.Utilities;
using UnityEngine;
using static HackingTool;
using static TheArchive.Core.ArchivePatcher;
using static TheArchive.Utilities.Utils;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableQualityOfLifeImprovements), "QOL")]
    public class QualityOfLifePatches
    {
        // add R4 / R5 to the beginning of the header text
        [ArchivePatch(typeof(PlayerGuiLayer), "UpdateObjectiveHeader")]
        internal static class PlayerGuiLayer_UpdateObjectiveHeaderPatch
        {
            public static void Prefix(ref string header)
            {
                header = $"R{ArchiveMod.CurrentRundown.GetIntValue()}{header}";
            }
        }

        // Change hacking minigame to be more in line with newest version of the game -> minigame finishes and hack disappears instantly
        [ArchivePatch(typeof(HackingTool), "UpdateHackSequence", RundownFlags.RundownFour)]
        internal static class HackingTool_UpdateHackSequencePatch
        {
            private static MethodInfo HackingTool_ClearScreen = typeof(HackingTool).GetMethod("ClearScreen", AnyBindingFlags);
            private static MethodInfo HackingTool_OnStopHacking = typeof(HackingTool).GetMethod("OnStopHacking", AnyBindingFlags);

            public static bool Prefix(ref HackingTool __instance)
            {
                try
                {

                    switch (__instance.m_state)
                    {
                        case HackSequenceState.DoneWait:
                            __instance.m_activeMinigame.EndGame();
                            __instance.m_holoSourceGFX.SetActive(value: false);
                            __instance.Sound.Post(EVENTS.BUTTONGENERICBLIPTHREE);
                            __instance.m_stateTimer = 1f;
                            if (__instance.m_currentHackable != null)
                            {
                                LG_LevelInteractionManager.WantToSetHackableStatus(__instance.m_currentHackable, eHackableStatus.Success, __instance.Owner);
                            }
                            __instance.m_state = HackSequenceState.Done;
                            return false;
                        case HackSequenceState.Done:
                            if (__instance.m_stateTimer < 0f)
                            {
                                HackingTool_ClearScreen.Invoke(__instance, null);
                                HackingTool_OnStopHacking.Invoke(__instance, null);
                                __instance.Sound.Post(EVENTS.BUTTONGENERICSEQUENCEFINISHED);
                                __instance.m_state = HackSequenceState.Idle;
                            }
                            else
                            {
                                __instance.m_stateTimer -= Clock.Delta;
                            }
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
                return true;
            }
        }

        // Remove the character restriction in chat, this also results in the user being able to use TMP tags but whatever
        [ArchivePatch(typeof(PlayerChatManager), nameof(PlayerChatManager.Setup))]
        internal static class PlayerChatManager_SetupPatch
        {
            public static void Postfix(ref PlayerChatManager __instance)
            {
                try
                {
                    typeof(PlayerChatManager).GetProperty(nameof(PlayerChatManager.m_forbiddenChars)).SetValue(__instance, new UnhollowerBaseLib.Il2CppStructArray<int>(0));
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
                
            }
        }

        #region WeaponsStuffs
        // Color by default disabled weapons red & cache last interacted weapons archetype data
        [ArchivePatch(typeof(CM_InventorySlotItem), nameof(CM_InventorySlotItem.LoadData))]
        internal static class CM_InventorySlotItem_LoadDataPatch
        {
            public static void Postfix(ref CM_InventorySlotItem __instance, ref GearIDRange idRange)
            {
                try
                {
                    CM_PlayerLobbyBar_OnWeaponSlotItemSelectedPatch.SetLastArchetypeDataBlock(idRange);

                    if (DataBlockManager.DefaultOfflineGear.Contains(idRange.PublicGearName)) return;

                    //ArchiveLogger.Notice($"CM_InventorySlotItem.LoadData: PublicGearName: {idRange.PublicGearName}");

                    __instance.m_nameText.text = $"<color=red>{idRange.PublicGearName}</color>";
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }

            }
        }

        // a second patch to cache the last interacted weapons archetype data
        [ArchivePatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.OnWeaponSlotItemSelected), RundownFlags.RundownFive, RundownFlags.Latest)]
        internal static class CM_PlayerLobbyBar_OnWeaponSlotItemSelectedPatch
        {
            public static ArchetypeDataBlock LastArchetypeDataBlock { get; private set; }

            public static void Prefix(ref CM_PlayerLobbyBar __instance, CM_InventorySlotItem slotItem)
            {
                try
                {
                    SetLastArchetypeDataBlock(slotItem.m_gearID);
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }

            public static void SetLastArchetypeDataBlock(GearIDRange idRange)
            {
                if (idRange == null)
                {
                    LastArchetypeDataBlock = null;
                    return;
                }

                uint compID = idRange.GetCompID(eGearComponent.Category);

                GearCategoryDataBlock block = GameDataBlockBase<GearCategoryDataBlock>.GetBlock(compID);
                eWeaponFireMode weaponFireMode = (eWeaponFireMode) idRange.GetCompID(eGearComponent.FireMode);

                ArchetypeDataBlock archetypeDataBlock = ((compID != 12)
                    ? GameDataBlockBase<ArchetypeDataBlock>.GetBlock(GearBuilder.GetArchetypeID(block, weaponFireMode))
                    : SentryGunInstance_Firing_Bullets.GetArchetypeDataForFireMode(weaponFireMode));

                LastArchetypeDataBlock = archetypeDataBlock;
            }
        }

        // Add weapon stats onto the info page
        [ArchivePatch(null, nameof(CM_ScrollWindowInfoBox.SetInfoBox), RundownFlags.RundownFive, RundownFlags.Latest)]
        internal static class CM_ScrollWindowInfoBox_SetInfoBoxPatch
        {
            public static Type Type()
            {
                return typeof(CM_ScrollWindowInfoBox);
            }

            public static void Prefix(ref CM_ScrollWindowInfoBox __instance, ref string mainTitle, ref string subTitle, ref string description, ref string acceptText, ref string rejectText, ref Sprite icon)
            {
                try
                {
                    if (!DataBlockManager.DefaultOfflineGear.Contains(mainTitle))
                    {
                        // Color by default disabled weapons red on the details / info box
                        mainTitle = $"<color=red>{mainTitle}</color>";
                    }

                    description = $"{description}\n\n{GetFormatedWeaponStats(CM_PlayerLobbyBar_OnWeaponSlotItemSelectedPatch.LastArchetypeDataBlock)}";
                    CM_PlayerLobbyBar_OnWeaponSlotItemSelectedPatch.SetLastArchetypeDataBlock(null);
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }

            }

            public const string kDivider = " | ";
            public const string kEndColor = "</color>";

            public static string GetFormatedWeaponStats(ArchetypeDataBlock archeTypeDataBlock)
            {
                if (archeTypeDataBlock == null) return string.Empty;

                StringBuilder builder = new StringBuilder();

                builder.Append("<color=#9D2929>");
                builder.Append("Dmg ");
                builder.Append(archeTypeDataBlock.Damage);
                builder.Append(kEndColor);

                builder.Append(kDivider);

                builder.Append("<color=orange>");
                builder.Append("Clp ");
                builder.Append(archeTypeDataBlock.DefaultClipSize);
                builder.Append(kEndColor);

                builder.Append(kDivider);

                builder.Append("<color=yellow>");
                builder.Append("Rld ");
                builder.Append(archeTypeDataBlock.DefaultReloadTime);
                builder.Append(kEndColor);

                if(archeTypeDataBlock.StaggerDamageMulti != 1f)
                {
                    builder.Append(kDivider);

                    builder.Append("<color=green>");
                    builder.Append("Stgr.Mlt ");
                    builder.Append(archeTypeDataBlock.StaggerDamageMulti);
                    builder.Append(kEndColor);
                }

                builder.Append("\n");

                if(archeTypeDataBlock.ShotgunBulletCount > 0)
                {
                    builder.Append("<color=#55022B>");
                    builder.Append("Sh.Plts ");
                    builder.Append(archeTypeDataBlock.ShotgunBulletCount);
                    builder.Append(kEndColor);

                    builder.Append(kDivider);

                    builder.Append("<color=#A918A7>");
                    builder.Append("Sh.Sprd ");
                    builder.Append(archeTypeDataBlock.ShotgunBulletSpread);
                    builder.Append(kEndColor);

                    builder.Append("\n");
                }

                if(archeTypeDataBlock.BurstShotCount > 1)
                {
                    builder.Append("<color=#025531>");
                    builder.Append("Brst.Sht ");
                    builder.Append(archeTypeDataBlock.BurstShotCount);
                    builder.Append(kEndColor);

                    builder.Append(kDivider);

                    builder.Append("<color=#18A4A9>");
                    builder.Append("Brst.Dly ");
                    builder.Append(archeTypeDataBlock.BurstDelay);
                    builder.Append(kEndColor);
                }

                return builder.ToString();
            }
        }
        #endregion

    }
}
