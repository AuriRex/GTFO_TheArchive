using CellMenu;
using Player;
using SNetwork;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Managers;
using TheArchive.Core.Models;
using TheArchive.Core.Settings;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.PresenceFormatter;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features
{
    [EnableFeatureByDefault(true)]
    public class RichPresenceCore : Feature
    {
        public class RichPresenceCoreSettings
        {
            public bool EnableDiscordRPC { get; set; } = true;
        }

        public override string Name => "Rich Presence Core";

        [FeatureConfig]
        public static RichPresenceCoreSettings Config { get; set; }

        [FeatureConfig]
        public static RichPresenceSettings DiscordRPCSettings { get; set; }

        public override void Init()
        {
            DiscordRPCSettings = DiscordRPCSettings?.FillDefaultDictValues();

            try
            {
                DiscordManager.Enable(DiscordRPCSettings);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }

            PresenceManager.UpdateGameState(PresenceGameState.Startup, false);
        }

        public override void OnEnable()
        {
            try
            {
                DiscordManager.Enable(DiscordRPCSettings);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }
        }

        public void Update()
        {
            DiscordManager.Update();
        }

        public override void OnQuit()
        {
            DiscordManager.Disable();
        }

        #region weapons
        public static string ItemNameForSlot(InventorySlot slot)
        {
            BackpackItem item = null;
            if (PlayerBackpackManager.LocalBackpack?.TryGetBackpackItem(slot, out item) ?? false)
            {
                return item?.GearIDRange?.PublicGearName;
            }
            return null;
        }

        public static string ItemIDForSlot(InventorySlot slot)
        {
            BackpackItem item = null;
            if (PlayerBackpackManager.LocalBackpack?.TryGetBackpackItem(slot, out item) ?? false)
            {
                return item?.GearIDRange?.PlayfabItemId;
            }
            return null;
        }


        [PresenceFormatProvider(nameof(PresenceManager.EquippedMeleeWeaponName))]
        public static string EquippedMeleeWeaponName => ItemNameForSlot(InventorySlot.GearMelee);

        [PresenceFormatProvider(nameof(PresenceManager.EquippedMeleeWeaponID))]
        public static string EquippedMeleeWeaponID => ItemIDForSlot(InventorySlot.GearMelee);



        [PresenceFormatProvider(nameof(PresenceManager.EquippedToolName))]
        public static string EquippedToolName => ItemNameForSlot(InventorySlot.GearClass);

        [PresenceFormatProvider(nameof(PresenceManager.EquippedToolID))]
        public static string EquippedToolID => ItemIDForSlot(InventorySlot.GearClass);
        #endregion weapons

        #region player_values
        private static PlayerAmmoStorage LocalAmmo => PlayerBackpackManager.LocalBackpack?.AmmoStorage;

        private static int GetClip(InventorySlot slot)
        {
            BackpackItem backpackItem = null;
            if(PlayerBackpackManager.LocalBackpack?.TryGetBackpackItem(slot, out backpackItem) ?? false)
            {
#if IL2CPP
                return backpackItem?.Instance?.TryCast<ItemEquippable>()?.GetCurrentClip() ?? -1;
#else
                return (backpackItem?.Instance as ItemEquippable)?.GetCurrentClip() ?? -1;
#endif
            }
            return -1;
        }

        [PresenceFormatProvider(nameof(PresenceManager.HealthRaw))]
        public static float HealthRaw => PlayerManager.GetLocalPlayerAgent()?.Damage?.Health ?? -1;

        [PresenceFormatProvider(nameof(PresenceManager.MaxHealthRaw))]
        public static float MaxHealthRaw => PlayerManager.GetLocalPlayerAgent()?.Damage?.HealthMax ?? -1;

        [PresenceFormatProvider(nameof(PresenceManager.ToolAmmo))]
        public static int ToolAmmo => LocalAmmo?.ClassAmmo?.BulletsInPack ?? -1;

        [PresenceFormatProvider(nameof(PresenceManager.MaxToolAmmo))]
        public static int MaxToolAmmo => LocalAmmo?.ClassAmmo?.BulletsMaxCap ?? -1;

        [PresenceFormatProvider(nameof(PresenceManager.PrimaryAmmo))]
        public static int PrimaryAmmo => (LocalAmmo?.StandardAmmo?.BulletsInPack ?? -1) + GetClip(InventorySlot.GearStandard);

        [PresenceFormatProvider(nameof(PresenceManager.MaxPrimaryAmmo))]
        public static int MaxPrimaryAmmo => (LocalAmmo?.StandardAmmo?.BulletsMaxCap ?? -1);

        [PresenceFormatProvider(nameof(PresenceManager.SpecialAmmo))]
        public static int SpecialAmmo => (LocalAmmo?.SpecialAmmo?.BulletsInPack ?? -1) + GetClip(InventorySlot.GearSpecial);

        [PresenceFormatProvider(nameof(PresenceManager.MaxSpecialAmmo))]
        public static int MaxSpecialAmmo => (LocalAmmo?.SpecialAmmo?.BulletsMaxCap ?? -1);
#endregion player_values

#region lobby
        [PresenceFormatProvider(nameof(PresenceManager.LobbyID))]
        public static string LobbyID => SNet.Lobby?.Identifier?.ID.ToString() ?? "0123456789";

        [PresenceFormatProvider(nameof(PresenceManager.LocalCharacterID))]
        public static int LocalCharacterID
        {
            get
            {
                try
                {
                    return PlayerManager.GetLocalPlayerSlotIndex();
                }
                catch(Exception)
                {
                    return 0;
                }
            }
        }

        [PresenceFormatProvider("OpenSlots")]
        public static int OpenSlots
        {
            get
            {
                return PresenceManager.MaxPlayerSlots - GetPlayerCount();
            }
        }

        private static int GetPlayerCount()
        {
            if (FlagsContain(RundownFlags.RundownSix.To(RundownFlags.Latest), BuildInfo.Rundown))
                return GetPlayerCountR6Plus();

            return SNet.Lobby?.Players?.Count ?? 1;
        }

        private static int GetPlayerCountR6Plus()
        {
            return SNet.Lobby?.Players
#if IL2CPP
                ?.ToSystemList()
#endif
                ?.Where(ply => !ply.IsBot)?.Count() ?? 1;
        }
#endregion lobby

#region expedition
        [PresenceFormatProvider(nameof(PresenceManager.ExpeditionTier))]
        public static string ExpeditionTier => RundownManager.ActiveExpedition?.Descriptive?.Prefix ?? "?";

        [PresenceFormatProvider(nameof(PresenceManager.ExpeditionTierIsSpecial))]
        public static bool ExpeditionTierIsSpecial => (RundownManager.ActiveExpedition?.Descriptive?.Prefix ?? "?").EndsWith("X");

        [PresenceFormatProvider(nameof(PresenceManager.ExpeditionNumber))]
        public static int ExpeditionNumber { get; set; } = 0;

        [PresenceFormatProvider(nameof(PresenceManager.ExpeditionName))]
        public static string ExpeditionName => RundownManager.ActiveExpedition?.Descriptive?.PublicName ?? "???";


        [PresenceFormatProvider(nameof(PresenceManager.ZonePrefix))]
        public static string ZonePrefix => PlayerManager.GetLocalPlayerAgent()?.CourseNode?.m_zone?.NavInfo?.PrefixShort ?? "?";

        [PresenceFormatProvider(nameof(PresenceManager.ZonePrefixLong))]
        public static string ZonePrefixLong => PlayerManager.GetLocalPlayerAgent()?.CourseNode?.m_zone?.NavInfo?.PrefixLong ?? "?";

        [PresenceFormatProvider(nameof(PresenceManager.ZoneAlias))]
        public static string ZoneAlias => PlayerManager.GetLocalPlayerAgent()?.CourseNode?.m_zone?.NavInfo?.Number.ToString() ?? "?";

        [PresenceFormatProvider(nameof(PresenceManager.AreaSuffix))]
        public static string AreaSuffix => PlayerManager.GetLocalPlayerAgent()?.CourseNode?.m_area?.m_navInfo?.Suffix ?? "?";
#endregion expedition

        // RundownManager.SetActiveExpedition(pActiveExpedition expPackage, ExpeditionInTierData expTierData) calls:
        // RundownManager.GetUniqueExpeditionKey(string rundownKey, eRundownTier tier, int expIndex)
        [ArchivePatch(typeof(RundownManager), nameof(RundownManager.GetUniqueExpeditionKey), new Type[] { typeof(string), typeof(eRundownTier), typeof(int) })]
        internal static class RundownManager_SetActiveExpeditionPatch
        {
            public static void Postfix(string rundownKey, eRundownTier tier, int expIndex)
            {
                ExpeditionNumber = expIndex + 1;
            }
        }

        public static void CopyLobbyIdToClipboard(int _)
        {
            if (BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownFive.ToLatest()) && IsExternalMatchMakingActive())
            {
                return;
            }

            if (!SNet.IsInLobby)
            {
                return;
            }

            var formatedLobbyId = PresenceFormatter.FormatPresenceString(ArchiveMod.Settings.LobbyIdFormatString);
            GUIUtility.systemCopyBuffer = formatedLobbyId;
            ArchiveLogger.Notice($"Copied lobby id to clipboard: {formatedLobbyId}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsExternalMatchMakingActive()
        {
            return SNet.IsExternalMatchMakingActive;
        }

        [ArchivePatch(typeof(CM_PageSettings), nameof(CM_PageSettings.Setup))]
        internal static class CM_PageSettings_SetupPatch
        {
            public static void Postfix(CM_PageSettings __instance)
            {
                CM_Item copyLobbyIDButton = (CM_Item) __instance.GetType().GetProperty(nameof(__instance.m_copyLobbyIdButton))?.GetValue(__instance, null);

                if(copyLobbyIDButton != null)
                {
                    ArchiveLogger.Info("Hooking CM_PageSettings Copy Lobby ID Button ...");
                    copyLobbyIDButton.OnBtnPressCallback = (Action<int>) CopyLobbyIdToClipboard;
                }
            }
        }

        [ArchivePatch(typeof(CM_PageLoadout), nameof(CM_PageLoadout.Setup))]
        internal static class CM_PageLoadout_SetupPatch
        {
            public static void Postfix(CM_PageLoadout __instance)
            {
                CM_Item copyLobbyIDButton = __instance.m_movingContentHolder?.GetChildWithExactName("ShareServerId")?.GetChildWithExactName("Button Copy Clipboard")?.GetComponent<CM_Item>();

                if(copyLobbyIDButton != null)
                {
                    ArchiveLogger.Info("Hooking CM_PageLoadout Copy Lobby ID Button ...");
                    copyLobbyIDButton.OnBtnPressCallback = (Action<int>) CopyLobbyIdToClipboard;
                }
            }
        }

        [ArchivePatch(typeof(GameStateManager), nameof(GameStateManager.ChangeState))]
        internal static class GameStateManager_Patch
        {
            public static void Postfix(eGameStateName nextState)
            {
                switch (nextState)
                {
                    case eGameStateName.NoLobby:
                        PresenceManager.UpdateGameState(PresenceGameState.NoLobby, keepTimer: PresenceManager.CurrentState == PresenceGameState.Startup);
                        break;
                    case eGameStateName.ExpeditionAbort:
                    case eGameStateName.Lobby:
                        PresenceManager.UpdateGameState(PresenceGameState.InLobby);
                        break;
                    case eGameStateName.Generating:
                        PresenceManager.UpdateGameState(PresenceGameState.Dropping);
                        break;
                    case eGameStateName.ReadyToStopElevatorRide:
                        PresenceManager.UpdateGameState(PresenceGameState.LevelGenerationFinished, keepTimer: true);
                        break;
                    case eGameStateName.InLevel:
                        PresenceManager.UpdateGameState(PresenceGameState.InLevel, keepTimer: PresenceManager.CurrentState == PresenceGameState.ExpeditionFailed);
                        break;
                    case eGameStateName.ExpeditionFail:
                        PresenceManager.UpdateGameState(PresenceGameState.ExpeditionFailed, keepTimer: true);
                        break;
                    case eGameStateName.ExpeditionSuccess:
                        PresenceManager.UpdateGameState(PresenceGameState.ExpeditionSuccess, keepTimer: true);
                        break;
                }
            }
        }
    }
}
