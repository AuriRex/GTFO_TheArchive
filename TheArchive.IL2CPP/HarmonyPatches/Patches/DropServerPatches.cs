using DropServer;
using Il2CppSystem.Threading;
using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using TenCC.Utils;
using TheArchive.Core;
using TheArchive.Models;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;
using IL2Tasks = Il2CppSystem.Threading.Tasks;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableLocalProgressionPatches), "LocalProgression")]
    public class DropServerPatches
    {

        private static LocalRundownProgression _customRundownProgression = null;
        public static LocalRundownProgression CustomRundownProgression {
            get
            {
                if(_customRundownProgression == null)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"CoolJSONInput:{LocalFiles.LocalRundownProgressionJSON}");
                    //_customRundownProgression = LocalRundownProgression.FromJSON(LocalFiles.LocalRundownProgressionJSON);
                }
                    
                return _customRundownProgression;
            }
        }

        public static void SaveLocalRundownProgression()
        {
            string json = JsonConvert.SerializeObject(CustomRundownProgression, Formatting.Indented);

            LocalFiles.SaveRundownFourAndUpLocalRundownProgression(json);

            ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"Saved Rundown Progression to disk!");
        }

        /*[ArchivePatch(typeof(DropServerManager), nameof(DropServerManager.Setup))]
        public static class DropServerManager_SetupPatch
        {
            public static void Postfix(ref DropServerManager __instance)
            {
                ArchiveLogger.Msg(ConsoleColor.Magenta, $"{nameof(DropServerManager)}.{nameof(DropServerManager.Setup)}() called, setting TitleDataSettings to localhost.");
                var dstds = new DropServerManager.DropServerTitleDataSettings();
                dstds.ClientApiEndPoint = "https://localhost/api";
                __instance.ApplyTitleDataSettings(dstds);
            }
        }*/

        //public unsafe Task<RundownProgression> GetRundownProgressionAsync(string rundownName, CancellationToken cancellationToken, [Optional] Il2CppSystem.Action<Task<RundownProgression>> callback)
        /*[ArchivePatch(typeof(DropServerManager), nameof(DropServerManager.GetRundownProgressionAsync))]
        public static class DropServerManager_GetRundownProgressionAsyncPatch
        {
            public static bool Prefix(ref IL2Tasks.Task<RundownProgression> __result, string rundownName, CancellationToken cancellationToken, [Optional] Il2CppSystem.Action<Il2CppSystem.Threading.Tasks.Task<RundownProgression>> callback)
            {
                ArchiveLogger.Msg(ConsoleColor.Magenta, "Getting new RundownProgression (from local files)");

                try
                {
                    var task = __result = IL2Tasks.Task.FromResult(Managers.LocalProgressionManager.CustomRundownProgression.ToBaseGameProgression());

                    if (callback != null)
                    {
                        TaskUtils.ContinueOnCurrentContext<RundownProgression>(task, callback);
                    }
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }

                return false;
            }
        }*/

        /*[HarmonyPatch(typeof(DropServerManager), nameof(DropServerManager.PostSetup))]*/
    }
}
