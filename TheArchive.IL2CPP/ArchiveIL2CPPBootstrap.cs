using GameData;
using HarmonyLib;
using System;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;
using static TheArchive.Utilities.Utils;

namespace TheArchive
{
    public class ArchiveIL2CPPBootstrap
    {

        private static void OnGameDataInitialized(uint rundownId)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ArchiveIL2CPPModule.instance.Core.InvokeGameDataInitialized(rundownId);
#pragma warning restore CS0618 // Type or member is obsolete

            if(FlagsContain(RundownFlags.RundownFour.To(RundownFlags.RundownFive), ArchiveMod.CurrentRundown))
            {
                // Invoke DataBlocksReady on R4 & R5 instantly
                OnDataBlocksReady();
            }
        }

        private static void OnDataBlocksReady()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ArchiveIL2CPPModule.instance.Core.InvokeDataBlocksReady();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [HarmonyPatch(typeof(GameDataInit), nameof(GameDataInit.Initialize))]
        internal static class GameDataInit_InitializePatch
        {
            public static void Postfix()
            {
                try
                {
                    GameSetupDataBlock block = GameDataBlockBase<GameSetupDataBlock>.GetBlock(1u);
                    var rundownId = block.RundownIdToLoad;

                    OnGameDataInitialized(rundownId);
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Error($"Exception thrown in {nameof(ArchiveIL2CPPBootstrap)}");
                    ArchiveLogger.Exception(ex);
                }
            }
        }

        [ArchivePatch(null, "Setup", RundownFlags.RundownSix, RundownFlags.Latest)]
        internal static class LocalizationManager_SetupPatch
        {
            public static Type Type()
            {
                return typeof(LocalizationManager);
            }

            public static void Postfix()
            {
                OnDataBlocksReady();
            }
        }
    }
}
