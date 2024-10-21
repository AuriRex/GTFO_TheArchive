using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Managers;
using TheArchive.Loader;
using TheArchive.Utilities;

namespace TheArchive.Features.LocalProgression
{
    [EnableFeatureByDefault, HideInModSettings, DoNotSaveToConfig]
    [RundownConstraint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
    internal class GTFOApi_Compat : Feature
    {
        public override string Name => nameof(GTFOApi_Compat);

        public override FeatureGroup Group => FeatureGroups.LocalProgression;

        public override bool ShouldInit()
        {
#if !BepInEx
            RequestDisable("Not needed.");
            return false;
#else

            if (!LoaderWrapper.IsModInstalled("dev.gtfomodding.gtfo-api"))
            {
                RequestDisable("GTFO-Api not installed!");
                return false;
            }

            if (IsPlayingModded)
            {
                // Force on Local Progression for modded games.
                //LocalProgressionController.ForceEnable = true;
                // Do not init feature to use the profile folder for Favorites storage etc
                RequestDisable("MTFO is installed, not using our favorites location.");
                return false;
            }

            // Apply the patch below for vanilla games (= MTFO not installed) and use our custom favorites location
            return true;
#endif
        }

#if BepInEx
        [ArchivePatch("Setup_Prefix")]
        internal static class GearManager_Patches_Setup_Prefix_Patch
        {
            public static Type Type() => ImplementationManager.FindTypeInCurrentAppDomain("GTFO.API.Patches.GearManager_Patches", exactMatch: true);

            public static bool Prefix()
            {
                return ArchivePatch.SKIP_OG;
            }
        }
#endif
    }
}
