using DropServer;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Utilities;

namespace TheArchive.HarmonyPatches.AutoPatches
{
    public class RundownManagerPatches
    {
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
