using DropServer;
using HarmonyLib;
using Il2CppSystem.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TenCC.Utils;
using TheArchive.Models;
using TheArchive.Utilities;
using IL2Tasks = Il2CppSystem.Threading.Tasks;

namespace TheArchive.HarmonyPatches.AutoPatches
{
    public class DropServerPatches
    {

        

        /*private static RundownProgression RundownProgression {
            get
            {
                return CustomRundownProgression.ToBaseGameProgression();
            }
        }*/
        private static CustomRundownProgression _customRundownProgression = null;
        public static CustomRundownProgression CustomRundownProgression {
            get
            {
                if(_customRundownProgression == null)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"CoolJSONInput:{LocalFiles.LocalRundownProgressionJSON}");
                    _customRundownProgression = CustomRundownProgression.FromJSON(LocalFiles.LocalRundownProgressionJSON);
                }
                    
                return _customRundownProgression;
            }
        }


        public static IL2Tasks.Task<RundownProgression> GetRundownProgressionTask()
        {
            return IL2Tasks.Task.FromResult(CustomRundownProgression.ToBaseGameProgression());
        }

        public static void SaveLocalRundownProgression()
        {
            // Crashes game here somewhere
            //string json = JsonConvert.SerializeObject(RundownProgression, Formatting.Indented);

            string json = JsonConvert.SerializeObject(CustomRundownProgression, Formatting.Indented);

            LocalFiles.SaveRundownFourAndUpLocalRundownProgression(json);

            ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"Saved Rundown Progression to disk!");
        }

        /*public static RundownProgression TestTaskMeth()
        {
            var task = GetRundownProgressionTask(LocalFiles.LocalRundownProgressionJSON);
            task.Wait();
            return task.Result;
        }*/

        //DropServerClientAPIViaPlayFab
        //private async Task<TResult> MakeRequestAsync<TRequest, TResult>(TRequest request, string funcName)
        // Doesn't work because Harmony can't find the IL2CPP Method for this one ;-;
        /*[HarmonyPatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.MakeRequestAsync))]
        public static class DropServerClientAPIViaPlayFab_MakeRequestAsyncPatch
        {
            public static bool Prefix(string funcName)
            {
                ArchiveLogger.Msg(ConsoleColor.Cyan, $"{nameof(DropServerClientAPIViaPlayFab)}.{nameof(DropServerClientAPIViaPlayFab.MakeRequestAsync)}() called. FuncName: {funcName}");
                return false;
            }
        }*/


        //m_titleDataSettings
        [HarmonyPatch(typeof(DropServerManager), nameof(DropServerManager.Setup))]
        public static class DropServerManager_SetupPatch
        {
            public static void Postfix(ref DropServerManager __instance)
            {
                if(DropServerClientAPIViaPlayFabPatches.EnableCustomProgressionPatch)
                {
                    ArchiveLogger.Msg(ConsoleColor.Magenta, $"{nameof(DropServerManager)}.{nameof(DropServerManager.Setup)}() called, setting TitleDataSettings to localhost.");
                    var dstds = new DropServerManager.DropServerTitleDataSettings();
                    dstds.ClientApiEndPoint = "https://localhost/api";
                    __instance.ApplyTitleDataSettings(dstds);
                    //__instance.m_titleDataSettings = dstds;
                    //Utils.SetPropertyValue<DropServerManager, CustomDropServerClientAPI>(nameof(DropServerManager.ClientApi), new CustomDropServerClientAPI(__instance.ClientApi.Pointer), __instance);
                }
            }
        }

        //public unsafe Task<RundownProgression> GetRundownProgressionAsync(string rundownName, CancellationToken cancellationToken, [Optional] Il2CppSystem.Action<Task<RundownProgression>> callback)
        // TODO: UNCOMMENT THIS PATCH!!!!
        [HarmonyPatch(typeof(DropServerManager), nameof(DropServerManager.GetRundownProgressionAsync))]
        public static class DropServerManager_GetRundownProgressionAsyncPatch
        {
            public static bool Prefix(ref IL2Tasks.Task<RundownProgression> __result, string rundownName, CancellationToken cancellationToken, [Optional] Il2CppSystem.Action<Il2CppSystem.Threading.Tasks.Task<RundownProgression>> callback)
            {
                ArchiveLogger.Msg(ConsoleColor.Yellow, "GetRundownProgressionAsync called ...");
                if (DropServerClientAPIViaPlayFabPatches.EnableCustomProgressionPatch)
                {
                    ArchiveLogger.Msg(ConsoleColor.Magenta, "Getting new RundownProgression (from local files)");
                    LocalFiles.LoadRundownFourAndUpLocalRundownProgressionIfNecessary();


                    SaveLocalRundownProgression();


                    var task = GetRundownProgressionTask();



                    __result = task;

                    if (callback != null)
                    {
                        TaskUtils.ContinueOnCurrentContext<RundownProgression>(task, callback);
                    }

                    return false;
                }


                __result = IL2Tasks.Task.FromResult<RundownProgression>(new RundownProgression());
                ArchiveLogger.Msg(ConsoleColor.Yellow, "GetRundownProgressionAsync BLEP");

                //__result = Utils.IL2CPP.NullTask<RundownProgression>();
                return true;
            }
        }

        /*[HarmonyPatch(typeof(DropServerManager), nameof(DropServerManager.PostSetup))]
        public static class DropServerManager_PostSetupPatch
        {
            public static void Postfix(DropServerManager __instance)
            {
                ArchiveLogger.Msg(ConsoleColor.Yellow, $"{nameof(DropServerManager)}.{nameof(DropServerManager.PostSetup)} called ... Instance:{__instance}");
                async void Test()
                {
                    var rundownProgression = __instance.GetRundownProgressionAsync("Local_26", new CancellationToken()).GetAwaiter().GetResult();
                    ArchiveLogger.Msg(ConsoleColor.Yellow, $"RundownProgression: {rundownProgression}");
                }

                _ = Il2CppSystem.Threading.Tasks.Task.Run((Il2CppSystem.Action) Test);
            }
        }*/

        // Throws error
        /*[HarmonyPatch(typeof(DropServerManager), nameof(DropServerManager.GetRundownProgressionAsync))]
        public static class DropServerManager_GetRundownProgressionAsyncPatch
        {
            public static void Prefix()
            {
                ArchiveLogger.Msg(ConsoleColor.Yellow, $"GetRundownProgressionAsync called ...");
            }
        }*/

    }
}
