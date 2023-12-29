using Player;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.QoL
{
    public class LastUsedGearSwitcher : Feature
    {
        public override string Name => "Last Used Gear Switcher";

        public override FeatureGroup Group => FeatureGroups.QualityOfLife;

        public override string Description => "Allows you to swap between the last two used weapons via a keypress";

        public new static IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static LastUsedGearSwitcherSettings Settings { get; set; }

        public class LastUsedGearSwitcherSettings
        {
            [FSDisplayName("Quick Swap Key")]
            [FSDescription("Press this key to switch to the previously wielded gear.")]
            public KeyCode QuickSwitchKey { get; set; } = KeyCode.X;

            [FSHide]
            [FSDescription("Prints debug info to the console")]
            public bool DebugLog { get; set; } = false;
        }

        private static readonly eFocusState _eFocusState_FPS = Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.FPS));
        private static eFocusState _eFocusState_FPS_CommunicationDialog;

        public override void Init()
        {
#if IL2CPP
            if(Is.R6OrLater)
            {
                Utils.TryGetEnumFromName(nameof(eFocusState.FPS_CommunicationDialog), out _eFocusState_FPS_CommunicationDialog);
            }
#endif
        }

        public override void Update()
        {
            if (FocusStateManager.CurrentState != _eFocusState_FPS)
            {
                if (!Is.R6OrLater)
                    return;

                if(FocusStateManager.CurrentState != _eFocusState_FPS_CommunicationDialog)
                    return;
            }

            if (!Input.GetKeyDown(Settings.QuickSwitchKey))
                return;

            SwitchToPreviousSlot();
        }

        private static InventorySlot _previousInventorySlot = InventorySlot.GearMelee;

        public static void SwitchToPreviousSlot()
        {
            var player = PlayerManager.GetLocalPlayerAgent();

            var previousSlot = _previousInventorySlot;
            var currentSlot = player.Inventory.WieldedSlot;

            if (Settings.DebugLog)
                FeatureLogger.Debug($"Switching to previous slot: {currentSlot} -> {previousSlot}");

            player.Sync.WantsToWieldSlot(previousSlot, false);
        }

        //WantsToWieldSlot(InventorySlot slot, bool broadcastOnly = false)
        [ArchivePatch(typeof(PlayerSync), nameof(PlayerSync.WantsToWieldSlot))]
        internal static class PlayerSync_WantsToWieldSlot_Patch
        {
            public static void Prefix(PlayerSync __instance, InventorySlot slot)
            {
                if (!(__instance.Replicator?.OwningPlayer?.IsLocal ?? false))
                    return;

                var currentSlot = __instance.GetWieldedSlot();

                if (Settings.DebugLog)
                    FeatureLogger.Debug($"PlayerSync.WantsToWieldSlot: currentSlot:{currentSlot} -> slot:{slot} | _previousInventorySlot:{_previousInventorySlot}");

                if (currentSlot != slot)
                    _previousInventorySlot = currentSlot;
            }
        }
    }
}
