using Gear;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Dev;

[EnableFeatureByDefault]
internal class SmartFavoritesSaving : Feature
{
    public override string Name => "Smart Favorites Saving";

    public override FeatureGroup Group => FeatureGroups.Dev;

    public override string Description => "Only save selected weapons/vanity on drop or game quit.";

    public new static IArchiveLogger FeatureLogger { get; set; }

    private static readonly eGameStateName _eGameStateName_Generating = GetEnumFromName<eGameStateName>(nameof(eGameStateName.Generating));

    public void OnGameStateChanged(eGameStateName state)
    {
        if(state == _eGameStateName_Generating)
        {
            SaveFavoritesFiles();
        }
    }

    public override void OnQuit()
    {
        SaveFavoritesFiles();
    }

    private static void SaveFavoritesFiles()
    {
        FeatureLogger.Notice("Saving Favorites file(s)!");
        GearManager_SaveFavoritesData_Patch.InvokeOriginal();
#if IL2CPP
        if (Is.R6OrLater)
            GearManager_SaveBotFavoritesData_Patch.InvokeOriginal();
#endif
    }

#if MONO
        [ArchivePatch(typeof(GearManager), "SaveFavoritesData")]
#else
    [ArchivePatch(typeof(GearManager), nameof(GearManager.SaveFavoritesData))]
#endif
    internal static class GearManager_SaveFavoritesData_Patch
    {
        private static bool _shouldRun = false;

        private static MethodAccessor<GearManager> _A_SaveFavoritesData;

        public static void Init()
        {
            _A_SaveFavoritesData = MethodAccessor<GearManager>.GetAccessor("SaveFavoritesData");
        }

        public static bool Prefix()
        {
            if (_shouldRun)
                return ArchivePatch.RUN_OG;

            if (DevMode)
                FeatureLogger.Debug("Skipping SaveFavoritesData!");
            return ArchivePatch.SKIP_OG;
        }

        public static void InvokeOriginal()
        {
            _shouldRun = true;
            _A_SaveFavoritesData.Invoke(null);
            _shouldRun = false;
        }
    }

#if IL2CPP
    [RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
    [ArchivePatch(typeof(GearManager), nameof(GearManager.SaveBotFavoritesData))]
    internal static class GearManager_SaveBotFavoritesData_Patch
    {
        private static bool _shouldRun = false;

        public static bool Prefix()
        {
            if (_shouldRun)
                return ArchivePatch.RUN_OG;

            if (DevMode)
                FeatureLogger.Debug("Skipping SaveBotFavoritesData!");
            return ArchivePatch.SKIP_OG;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InvokeOriginal()
        {
            _shouldRun = true;
            GearManager.SaveBotFavoritesData();
            _shouldRun = false;
        }
    }
#endif
}