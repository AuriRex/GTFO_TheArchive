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
