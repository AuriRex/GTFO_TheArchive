using Globals;
using HarmonyLib;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

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
        public override bool RequiresRestart => true;

        [ArchivePatch(typeof(StartMainGame), "Awake")]
        internal static class StartMainGame_Awake_Patch
        {
            public static void Postfix()
            {
                ArchiveMod.CurrentlySelectedRundownKey = $"Local_{Global.RundownIdToLoad}";
#pragma warning disable CS0618 // Type or member is obsolete
                ArchiveMod.InvokeGameDataInitialized();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        [ArchivePatch(typeof(SNet_GlobalManager), nameof(SNet_GlobalManager.Setup))]
        internal static class SNet_GlobalManager_Setup_Patch
        {
            public static void Postfix()
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ArchiveMod.InvokeDataBlocksReady();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        [ArchivePatch(typeof(GameStateManager), nameof(GameStateManager.ChangeState))]
        internal static class GameStateManager_ChangeState_Patch
        {
            public static void Postfix(eGameStateName nextState)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ArchiveMod.InvokeGameStateChanged((int) nextState);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
