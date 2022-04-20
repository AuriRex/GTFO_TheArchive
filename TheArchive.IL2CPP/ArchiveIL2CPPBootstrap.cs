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
                catch(System.Reflection.ReflectionTypeLoadException rtlex)
                {
                    ArchiveLogger.Error($"Exception thrown in {nameof(ArchiveIL2CPPBootstrap)}");
                    ArchiveLogger.Msg(ConsoleColor.Green, "Oh no, seems like someone's referencing game types from an older/newer game version that do not exist anymore! :c");
                    ArchiveLogger.Exception(rtlex);
                    ArchiveLogger.Warning($"{rtlex.Types?.Length} Types loaded.");
                    ArchiveLogger.Notice("Exceptions:");
                    foreach(var expt in rtlex.LoaderExceptions)
                    {
                        ArchiveLogger.Error(expt.Message);
                    }
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Error($"Exception thrown in {nameof(ArchiveIL2CPPBootstrap)}");
                    ArchiveLogger.Exception(ex);
                    if(ex.InnerException != null)
                        ArchiveLogger.Exception(ex?.InnerException);
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

        [ArchivePatch(typeof(GameStateManager), nameof(GameStateManager.ChangeState))]
        internal static class GameStateManager_ChangeStatePatch
        {
            public static void Postfix(eGameStateName nextState)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ArchiveIL2CPPModule.instance.Core.InvokeGameStateChanged((int) nextState);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
