using GameData;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive
{
    [HideInModSettings]
    [DisallowInGameToggle]
    [DoNotSaveToConfig]
    [EnableFeatureByDefault]
    public class ArchiveIL2CPPBootstrap : Feature
    {
        public override string Name => nameof(ArchiveIL2CPPBootstrap);
        public override string Group => FeatureGroups.Dev;
        public override bool RequiresRestart => true;

        private static void OnGameDataInitialized()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ArchiveMod.InvokeGameDataInitialized();
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
            ArchiveMod.InvokeDataBlocksReady();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [ArchivePatch(typeof(GameDataInit), nameof(GameDataInit.Initialize))]
        internal static class GameDataInit_Initialize_Patch
        {
            public static void Postfix()
            {
                try
                {
                    OnGameDataInitialized();
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

        [RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
        [ArchivePatch("Setup")]
        internal static class LocalizationManager_Setup_Patch
        {
            public static Type Type() => typeof(LocalizationManager);

            public static void Postfix() => OnDataBlocksReady();
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
