using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Backport
{
    [EnableFeatureByDefault]
    [RundownConstraint(RundownFlags.RundownOne)]
    public class R1LadderFix : Feature
    {
        public override string Name => "R2+ Like Ladders";

        public override string Description => "Fix ladder movement so that W is always upwards and S always downwards, no matter where you're looking.";

        public override string Group => FeatureGroups.Backport;

#if MONO
        [ArchivePatch(typeof(LG_Ladder), "GetMoveVec")]
        internal static class LG_Ladder_GetMoveVecPatch
        {
            public static bool Prefix(ref Vector3 __result, Vector3 camDir, float axisVertical)
            {
                __result = Vector3.ClampMagnitude(Vector3.up * axisVertical, 1f);

                return false;
            }
        }
#endif
    }
}
