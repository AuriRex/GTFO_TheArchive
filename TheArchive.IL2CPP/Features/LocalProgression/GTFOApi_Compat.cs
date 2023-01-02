using System;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Managers;
using TheArchive.Loader;
using TheArchive.Utilities;

namespace TheArchive.Features.LocalProgression
{
    [EnableFeatureByDefault, HideInModSettings, DoNotSaveToConfig]
    [RundownConstraint(Utils.RundownFlags.Latest)]
    internal class GTFOApi_Compat : Feature
    {
        public override string Name => nameof(GTFOApi_Compat);

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

            // Force on Local Progression for modded games.
            LocalProgressionController.ForceEnable = true;

            return true;
#endif
        }
    }
}
