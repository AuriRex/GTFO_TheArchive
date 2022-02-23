using Gear;
using System.Reflection;
using TheArchive.Core;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    public class OtherPatches
    {
        // Fix for Maul and Gavel having the same Checksum for SOME reason ...
        [ArchivePatch(typeof(GearIDRange), "GetChecksum")]
        internal static class GearIDRange_GetChecksumPatch
        {
            private static PropertyInfo m_checksum = typeof(GearIDRange).GetProperty("m_checksum", ArchivePatcher.AnyBindingFlags);
            private static PropertyInfo m_comps = typeof(GearIDRange).GetProperty("m_comps", ArchivePatcher.AnyBindingFlags);
            public static bool Prefix(GearIDRange __instance, ref uint __result)
            {
                uint checksum = 0;
                if (((uint) m_checksum.GetValue(__instance)) == 0U)
                {
                    ChecksumGenerator_32 checksumGenerator_ = new ChecksumGenerator_32();
                    for (int i = 0; i < ((ushort[]) m_comps.GetValue(__instance)).Length; i++)
                    {
                        checksumGenerator_.Insert((uint) ((ushort[]) m_comps.GetValue(__instance))[i]);
                    }
                    checksumGenerator_.Insert("name", __instance.PublicGearName);

                    checksum = checksumGenerator_.Checksum;
                    m_checksum.SetValue(__instance, checksum);
                }
                __result = checksum;
                return true;
            }
        }
    }
}
