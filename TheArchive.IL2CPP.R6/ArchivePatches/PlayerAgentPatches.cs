using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.IL2CPP.R6.ArchivePatches
{
    public class PlayerAgentPatches
    {

        //public bool TryWarpTo(eDimensionIndex dimensionIndex, Vector3 position, Vector3 lookDir, bool withBotsIfAny = false)
        [ArchivePatch(typeof(Player.PlayerAgent), "TryWarpTo")]
        public static class PlayerAgent_TryWarpToPatch
        {
            public static bool OverrideDimensionIndex { get; set; } = false;
            public static int DimensionIndex { get; set; } = 0;
            public static void Prefix(ref eDimensionIndex dimensionIndex)
            {
                if(OverrideDimensionIndex)
                {
                    if (DimensionIndex < 0 || DimensionIndex > ((int) eDimensionIndex.MAX_COUNT - 1)) return;
                    dimensionIndex = (eDimensionIndex) DimensionIndex;
                }
            }
        }

    }
}
