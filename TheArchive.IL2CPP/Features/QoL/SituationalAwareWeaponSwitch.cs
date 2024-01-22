using Player;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Features.QoL
{
    [EnableFeatureByDefault]
    public class SituationalAwareWeaponSwitch : Feature
    {
        public override string Name => "Situation Aware Weapon Switch";

        public override FeatureGroup Group => FeatureGroups.QualityOfLife;

        public override string Description => "Switch to either your Melee weapon or Primary depending on if you're sneaking around or in combat after depleting all of your throwables, exit a ladder or place down a sentry gun etc.";

        public new static IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static SituationalAwareWeaponSwitchSettings Settings { get; set; }

        public class SituationalAwareWeaponSwitchSettings
        {
            internal static readonly List<DramaState> defaultOptions = new List<DramaState>
            {
                DramaState.ElevatorIdle,
                DramaState.ElevatorGoingDown,
                DramaState.Exploration,
                DramaState.Alert,
                DramaState.Sneaking,
            };

            [FSHide]
            public bool IsFirstTime { get; set; } = true;

            [FSDisplayName("Log Weapon Switches to Console")]
            [FSDescription("Used to see what this Feature is doing in real time (check console!)")]
            public bool LogWeaponSwitchesToConsole { get; set; } = false;

            [FSDisplayName("Prefer Melee")]
            [FSDescription("The states in which the game should switch to Melee instead of your Primary weapon.")]
            public List<DramaState> PreferMeleeStates { get; set; } = new List<DramaState>();

            [Localized]
            public enum DramaState
            {
                ElevatorIdle,
                ElevatorGoingDown,
                Exploration,
                Alert,
                Sneaking,
                Encounter,
                Combat,
                Survival,
                IntentionalCombat,
            }
        }

        public override void OnEnable()
        {
            if(Settings.IsFirstTime)
            {
                Settings.PreferMeleeStates = new(SituationalAwareWeaponSwitchSettings.defaultOptions);
                Settings.IsFirstTime = false;
            }
            RecreatePreferMeleeStatesList();
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            RecreatePreferMeleeStatesList();
        }

        private void RecreatePreferMeleeStatesList()
        {
            PlayerBackpackManager_WieldFirstLocalGear_Patch.preferMeleeStates = new List<DRAMA_State>();
            foreach (var state in Settings.PreferMeleeStates)
            {
                if (Utils.TryGetEnumFromName<DRAMA_State>(state.ToString(), out var gameDramaState))
                {
                    PlayerBackpackManager_WieldFirstLocalGear_Patch.preferMeleeStates.Add(gameDramaState);
                }
            }

            if(Settings.LogWeaponSwitchesToConsole)
            {
                FeatureLogger.Debug("DramaStates that prefer melee:");
                foreach (var dramaState in PlayerBackpackManager_WieldFirstLocalGear_Patch.preferMeleeStates)
                {
                    FeatureLogger.Debug($"> {dramaState}");
                }
            }
        }

        [ArchivePatch(typeof(PlayerBackpackManager), nameof(PlayerBackpackManager.WieldFirstLocalGear))]
        internal static class PlayerBackpackManager_WieldFirstLocalGear_Patch
        {
            internal static List<DRAMA_State> preferMeleeStates = new List<DRAMA_State>
            {
                Utils.GetEnumFromName<DRAMA_State>(nameof(DRAMA_State.ElevatorIdle)),
                Utils.GetEnumFromName<DRAMA_State>(nameof(DRAMA_State.ElevatorGoingDown)),
                Utils.GetEnumFromName<DRAMA_State>(nameof(DRAMA_State.Exploration)),
                Utils.GetEnumFromName<DRAMA_State>(nameof(DRAMA_State.Sneaking)),
                Utils.GetEnumFromName<DRAMA_State>(nameof(DRAMA_State.Alert)),
            };

            public static bool Prefix(ref bool preferMelee)
            {
                if (PlayerBackpackManager.LocalBackpack.HasBackpackItem(InventorySlot.InLevelCarry))
                {
                    FeatureLogger.Debug($"Player has {InventorySlot.InLevelCarry}, re-equipping that!");
                    PlayerManager.GetLocalPlayerAgent().Sync.WantsToWieldSlot(InventorySlot.InLevelCarry, false);
                    return ArchivePatch.SKIP_OG;
                }

                if (Settings.LogWeaponSwitchesToConsole)
                    FeatureLogger.Notice($"Current drama state: {DramaManager.CurrentStateEnum}");

                if (preferMeleeStates.Any(state => state == DramaManager.CurrentStateEnum))
                {
                    preferMelee = true;
                }
                else
                {
                    preferMelee = false;
                }

                if (Settings.LogWeaponSwitchesToConsole)
                    FeatureLogger.Notice($"Prefer melee: {preferMelee}");

                return ArchivePatch.RUN_OG;
            }
        }
    }
}
