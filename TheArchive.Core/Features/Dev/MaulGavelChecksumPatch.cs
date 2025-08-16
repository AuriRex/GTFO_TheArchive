using Gear;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Dev;

[EnableFeatureByDefault, HideInModSettings]
[RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownAltSix)]
public class MaulGavelChecksumPatch : Feature
{
    public override string Name => "Maul/Gavel Checksum Patch";

    public override FeatureGroup Group => FeatureGroups.Dev;

    public override string Description => "Fixes the Maul and Gavel hammers having the same icon.";

    // Fix for Maul and Gavel having the same Checksum for SOME reason ...
#if IL2CPP
    [ArchivePatch(typeof(GearIDRange), "GetChecksum")]
    internal static class GearIDRange_GetChecksumPatch
    {
        public static void Prefix(GearIDRange __instance)
        {
            if (__instance.m_checksum == 0U)
            {
                ChecksumGenerator_32 checksumGenerator_ = new ChecksumGenerator_32();
                for (int i = 0; i < __instance.m_comps.Length; i++)
                {
                    checksumGenerator_.Insert((uint)__instance.m_comps[i]);
                }
                checksumGenerator_.Insert("name", __instance.PublicGearName);

                __instance.m_checksum = checksumGenerator_.Checksum;
            }
        }
    }
#else
        [ArchivePatch(typeof(GearIDRange), "GetChecksum")]
        internal static class GearIDRange_GetChecksumPatch
        {
            private static PropertyAccessor<GearIDRange, uint> A_m_checksum;
            private static PropertyAccessor<GearIDRange, ushort[]> A_m_comps;

            public static void Init()
            {
                A_m_checksum = PropertyAccessor<GearIDRange, uint>.GetAccessor("m_checksum");
                A_m_comps = PropertyAccessor<GearIDRange, ushort[]>.GetAccessor("m_comps");
            }

            public static void Prefix(GearIDRange __instance)
            {
                if (A_m_checksum.Get(__instance) == 0U)
                {
                    ChecksumGenerator_32 checksumGenerator_ = new ChecksumGenerator_32();
                    for (int i = 0; i < A_m_comps.Get(__instance).Length; i++)
                    {
                        checksumGenerator_.Insert(A_m_comps.Get(__instance)[i]);
                    }
                    checksumGenerator_.Insert("name", __instance.PublicGearName);

                    A_m_checksum.Set(__instance, checksumGenerator_.Checksum);
                }
            }
        }
#endif
}