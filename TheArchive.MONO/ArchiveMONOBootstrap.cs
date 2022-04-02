using Globals;
using HarmonyLib;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive
{
    public class ArchiveMONOBootstrap
    {
        [HarmonyPatch(typeof(StartMainGame), "Awake")]
        internal static class StartMainGame_AwakePatch
        {
            public static void Postfix()
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ArchiveMONOModule.instance.Core.InvokeGameDataInitialized(Global.RundownIdToLoad);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        [HarmonyPatch(typeof(SNet_GlobalManager), nameof(SNet_GlobalManager.Setup))]
        internal static class SNet_GlobalManager_SetupPatch
        {
            public static void Postfix()
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ArchiveMONOModule.instance.Core.InvokeDataBlocksReady();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        [ArchivePatch(typeof(GameStateManager), nameof(GameStateManager.ChangeState))]
        internal static class GameStateManager_ChangeStatePatch
        {
            public static void Postfix(eGameStateName nextState)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ArchiveMONOModule.instance.Core.InvokeGameStateChanged((int) nextState);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
