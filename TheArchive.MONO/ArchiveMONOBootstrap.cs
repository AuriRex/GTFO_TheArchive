using Globals;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;

namespace TheArchive
{
    [HideInModSettings]
    [DisallowInGameToggle]
    [DoNotSaveToConfig]
    [EnableFeatureByDefault]
    public class ArchiveMONOBootstrap : Feature
    {
        public override string Name => nameof(ArchiveMONOBootstrap);
        public override string Group => FeatureGroups.Dev;
        public override string Description => "Hooks into a bunch of important game code in order for this mod to work.";
        public override bool RequiresRestart => true;

        [ArchivePatch(typeof(StartMainGame), "Awake")]
        internal static class StartMainGame_Awake_Patch
        {
            public static void Postfix()
            {
                ArchiveMod.CurrentlySelectedRundownKey = $"Local_{Global.RundownIdToLoad}";
                ArchiveMod.InvokeGameDataInitialized();
            }
        }

        [ArchivePatch(typeof(SNet_GlobalManager), nameof(SNet_GlobalManager.Setup))]
        internal static class SNet_GlobalManager_Setup_Patch
        {
            public static void Postfix()
            {
                ArchiveMod.InvokeDataBlocksReady();
            }
        }

        [ArchivePatch(typeof(GameStateManager), nameof(GameStateManager.ChangeState))]
        internal static class GameStateManager_ChangeState_Patch
        {
            public static void Postfix(eGameStateName nextState)
            {
                ArchiveMod.InvokeGameStateChanged((int)nextState);
            }
        }
        
        [ArchivePatch(typeof(InControl.InControlManager), Utilities.UnityMessages.OnApplicationFocus)]
        internal static class EventSystem_OnApplicationFocus_Patch
        {
            public static void Postfix(bool focusState)
            {
                ArchiveMod.InvokeApplicationFocusChanged(focusState);
            }
        }
    }
}
