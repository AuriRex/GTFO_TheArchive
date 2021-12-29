using DropServer;
using HarmonyLib;
using Player;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    public class OtherPatches
    {
#warning TODO: doesn't work for producing the same voice lines as I'd have hoped it to ...
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
