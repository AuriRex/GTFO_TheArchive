using Player;
using SNetwork;
using System;
using System.Linq;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Managers;
using TheArchive.Core.Models;
using TheArchive.Utilities;
using static TheArchive.Utilities.PresenceFormatter;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features
{
    [EnableFeatureByDefault, HideInModSettings]
    public class RichPresenceCore : Feature
    {
        public override string Name => "Rich Presence Core";

        public override string Description => "Updates the Presence Game State and provides some values via patches.";

        public void OnGameStateChanged(eGameStateName nextState)
        {
            switch (nextState)
            {
                case eGameStateName.NoLobby:
                    PresenceManager.UpdateGameState(PresenceGameState.NoLobby, keepTimer: PresenceManager.CurrentState == PresenceGameState.Startup);
                    break;
#if IL2CPP
                case eGameStateName.ExpeditionAbort:
#endif
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
#if IL2CPP
            return SNet.Lobby?.Players

                ?.ToSystemList()

                ?.Where(ply => !ply.IsBot)?.Count() ?? 1;
#else
            return 1;
#endif
        }
#endregion lobby

#region expedition
        [PresenceFormatProvider(nameof(PresenceManager.ExpeditionTier))]
        public static string ExpeditionTier
        {
            get
            {
                string value = RundownManager.ActiveExpedition?.Descriptive?.Prefix ?? "?";

                if(BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownTwo | RundownFlags.RundownThree))
                {
                    if(value.Length > 2)
                        return value.Substring(2);
                }

                return value;
            }
        }

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
    }
}
