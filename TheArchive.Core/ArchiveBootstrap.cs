using GameData;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive;

[HideInModSettings]
[DisallowInGameToggle]
[DoNotSaveToConfig]
[EnableFeatureByDefault]
internal class ArchiveBootstrap : Feature
{
    public override string Name => nameof(ArchiveBootstrap);
    public override GroupBase Group => GroupManager.Dev;
    public override string Description => "Hooks into a bunch of important game code in order for this mod to work.";
    public override bool RequiresRestart => true;

    private static void InvokeGameDataInitialized()
    {
        ArchiveMod.InvokeGameDataInitialized();

        if (ArchiveMod.CurrentRundown.IsIncludedIn(RundownFlags.RundownFour | RundownFlags.RundownFive))
        {
            // Invoke DataBlocksReady on R4 & R5 instantly
            InvokeDataBlocksReady();
        }
    }

    private static void InvokeDataBlocksReady()
    {
        if(SharedUtils.TryGetRundownDataBlock(out var block))
        {
            ArchiveMod.CurrentlySelectedRundownKey = $"Local_{block.persistentID}";
        }

        ArchiveMod.InvokeDataBlocksReady();
    }

    [ArchivePatch(typeof(GameDataInit), nameof(GameDataInit.Initialize))]
    internal static class GameDataInit__Initialize__Patch
    {
        public static void Postfix()
        {
            try
            {
                InvokeGameDataInitialized();
            }
            catch(System.Reflection.ReflectionTypeLoadException typeLoadException)
            {
                ArchiveLogger.Error($"Exception thrown in {nameof(ArchiveBootstrap)}");
                ArchiveLogger.Msg(ConsoleColor.Green, "Oh no, seems like someone's referencing game types from an older/newer game version that do not exist anymore! :c");
                ArchiveLogger.Exception(typeLoadException);
                ArchiveLogger.Warning($"{typeLoadException.Types.Length} Types loaded.");
                ArchiveLogger.Notice("Exceptions:");
                foreach(var exception in typeLoadException.LoaderExceptions)
                {
                    if (exception == null)
                        continue;
                    
                    ArchiveLogger.Error(exception.Message);
                }
            }
            catch (Exception ex)
            {
                ArchiveLogger.Error($"Exception thrown in {nameof(ArchiveBootstrap)}");
                ArchiveLogger.Exception(ex);
                if(ex.InnerException != null)
                    ArchiveLogger.Exception(ex?.InnerException);
            }
        }
    }

    [RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
    [ArchivePatch("Setup")]
    internal static class LocalizationManager__Setup__Patch
    {
        public static Type Type() => typeof(LocalizationManager);

        public static void Postfix() => InvokeDataBlocksReady();
    }

    [ArchivePatch(typeof(GameStateManager), nameof(GameStateManager.ChangeState))]
    internal static class GameStateManager__ChangeState__Patch
    {
        public static void Postfix(eGameStateName nextState)
        {
            ArchiveMod.InvokeGameStateChanged((int)nextState);
        }
    }

    [RundownConstraint(RundownFlags.RundownAltOne, RundownFlags.Latest)]
    [ArchivePatch(nameof(CellMenu.CM_Item.OnBtnPress))]
    internal static class CM_RundownSelection__OnBtnPress__Patch
    {
        public static Type Type() => typeof(CM_RundownSelection);
        public static void Postfix(CM_RundownSelection __instance)
        {
            ArchiveMod.CurrentlySelectedRundownKey = __instance.RundownKey;
        }
    }

    [RundownConstraint(RundownFlags.RundownAltOne, RundownFlags.Latest)]
    [ArchivePatch(typeof(CellMenu.CM_PageRundown_New), nameof(CellMenu.CM_PageRundown_New.Setup))]
    internal static class CM_PageRundown_New__Setup__Patch
    {
        public static void Postfix(CellMenu.CM_PageRundown_New __instance)
        {
            __instance.m_selectRundownButton.AddCMItemEvents((_) => {
                ArchiveMod.CurrentlySelectedRundownKey = string.Empty;
            });
        }
    }

    [ArchivePatch(typeof(InControl.InControlManager), UnityMessages.OnApplicationFocus)]
    internal static class EventSystem__OnApplicationFocus__Patch
    {
        public static void Postfix(bool focusState)
        {
            ArchiveMod.InvokeApplicationFocusChanged(focusState);
        }
    }
}