using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;

namespace TheArchive.Features.Dev
{
    [EnableFeatureByDefault, HideInModSettings]
    public class UnityRandomOverrider : Feature
    {
        public override string Name => "UnityRandomOverrider";

        public override FeatureGroup Group => FeatureGroups.Dev;

        public new static IArchiveLogger FeatureLogger { get; set; }


        public static uint SetSeed { get; set; } = 0;
        public static bool ResetSeedAfterUse { get; set; } = true;

        // TODO: doesn't work for producing the same voice lines as I'd have hoped it to ...
        [ArchivePatch(typeof(UnityEngine.Random), nameof(UnityEngine.Random.Range), new Type[] { typeof(float), typeof(float) })]
        internal static class UnityEngine_Random_Range_Patch
        {
            public static void Postfix(ref float __result)
            {
                if (SetSeed != 0)
                {
                    __result = SetSeed;
                    FeatureLogger.Notice($"RNG Seed set to: {(uint)__result}");

                    if (ResetSeedAfterUse)
                        SetSeed = 0;
                }
            }
        }
    }
}
