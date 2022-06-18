using Gear;
using SNetwork;
using System;
using System.Reflection;
using TheArchive.Core;
using TheArchive.Utilities;
using static GearIconRendering;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    public class OtherPatches
    {
        // TODO: doesn't work for producing the same voice lines as I'd have hoped it to ...
        [ArchivePatch(typeof(UnityEngine.Random), nameof(UnityEngine.Random.Range), new Type[] { typeof(float), typeof(float) })]
        internal static class UnityEngine_RandomPatchOne
        {
            public static uint SetSeed { get; set; } = 0;
            public static void Postfix(ref float __result)
            {
                if(SetSeed != 0)
                {
                    __result = SetSeed;

                    ArchiveLogger.Notice($"RNG Seed set to: {(uint) __result}");
                    SetSeed = 0;
                }
            }
        }

        // Fix for Maul and Gavel having the same Checksum for SOME reason ...
        [ArchivePatch(typeof(GearIDRange), "GetChecksum")]
        internal static class GearIDRange_GetChecksumPatch
        {
            public static bool Prefix(GearIDRange __instance, ref uint __result)
            {
                if (__instance.m_checksum == 0U)
                {
                    ChecksumGenerator_32 checksumGenerator_ = new ChecksumGenerator_32();
                    for (int i = 0; i < __instance.m_comps.Length; i++)
                    {
                        checksumGenerator_.Insert((uint) __instance.m_comps[i]);
                    }
                    checksumGenerator_.Insert("name", __instance.PublicGearName);

                    __instance.m_checksum = checksumGenerator_.Checksum;
                }
                __result = __instance.m_checksum;
                return true;
            }
        }

        //Packet debug stuff >.>
        /*[ArchivePatch(typeof(SNet_Replicator), nameof(SNet_Replicator.AddPacket), Utils.RundownFlags.RundownSix)]
        internal static class SNet_Replicator_AddPacketPatch
        {
            public static void Prefix(SNet_Replicator __instance, SNet_Packet packet)
            {
                
                ArchiveLogger.Notice($"AddPacket called: ({__instance?.name}) #{__instance?.m_packets?.Count} - PacketName:{packet?.GetIl2CppType()?.FullName}, packet.Index:{packet?.Index}");
            }
        }*/

        /*
                [ArchivePatch(typeof(GearIconRendering), "RenderIconPrep")]
                internal static class GearIconRendering_RenderIconPrepPatch
                {
                    public static int ResMultiplier { get; set; } = 5;
                    public static void Prefix(ref IconRenderJob job)
                    {
                        var settings = job.datas[job.currentIconIndex].settings;

                        settings.resX = settings.resX * ResMultiplier;
                        settings.resY = settings.resY * ResMultiplier;
                    }
                }*/

        /*
                public static RundownProgression RundownProgression { get; private set; } = null;

                private static RundownProgressionResult rundownProgressionResult = new RundownProgressionResult();

                public static RundownProgression JSONToRundownProgression(string json)
                {
                    rundownProgressionResult.EscapedRundownProgression = json;
                    return rundownProgressionResult.GetRundownProgression();
                }*/

        //RundownManager.RundownProgression
        /*[HarmonyPatch(typeof(RundownManager))]
        [HarmonyPatch(nameof(RundownManager.RundownProgression), MethodType.Getter)]
        internal class RundownManager_RundownProgressionPatch
        {
            public static bool Prefix(ref RundownProgression __result)
            {
                ArchiveLogger.Msg(ConsoleColor.Red , $"{nameof(RundownManager)}.{nameof(RundownManager.RundownProgression)} requested");
                if(RundownProgression == null)
                {
                    RundownProgression = JSONToRundownProgression(LocalFiles.LocalRundownProgressionJSON);
                }


                __result = RundownProgression;
                return false;
            }
        }
        //OnRundownProgressionRequestDone
        // SetPlayerRundownProgression(RundownProgression rundownProgression)
        [HarmonyPatch(typeof(RundownManager), nameof(RundownManager.OnRundownProgressionRequestDone))]
        internal class RundownManager_OnRundownProgressionRequestDonePatch
        {
            public static bool Prefix()
            {
                ArchiveLogger.Msg(ConsoleColor.Red, $"{nameof(RundownManager)}.{nameof(RundownManager.OnRundownProgressionRequestDone)}() Prefix called, setting progression!");
                if (RundownProgression == null)
                {
                    RundownProgression = JSONToRundownProgression(LocalFiles.LocalRundownProgressionJSON);
                }
                Utils.SetPropertyValue<RundownManager, RundownProgression>(nameof(RundownManager.RundownProgression), RundownProgression);

                Utils.IL2CPP.CallEvent<RundownManager>("OnRundownProgressionUpdated");

                return false;
            }
        }*/
    }
}
