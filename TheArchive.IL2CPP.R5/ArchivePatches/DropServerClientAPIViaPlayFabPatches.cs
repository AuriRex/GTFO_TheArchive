using DropServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core.Core;
using TheArchive.Managers;
using TheArchive.Models;
using TheArchive.Utilities;
using UnhollowerRuntimeLib;
using static TheArchive.Core.ArchivePatcher;
using static TheArchive.Utilities.Il2CppUtils;
using static TheArchive.Utilities.Utils;
using IL2Tasks = Il2CppSystem.Threading.Tasks;

namespace TheArchive.IL2CPP.R5.ArchivePatches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableLocalProgressionPatches), "LocalProgression")]
    public class DropServerClientAPIViaPlayFabPatches
    {
        // Does not appear to be called in R5 anymore, removed in R6
        //public unsafe Task<ExpeditionSuccessResult> ExpeditionSuccessAsync(ExpeditionSuccessRequest request, [Optional] RequestContext context)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.ExpeditionSuccessAsync), RundownFlags.RundownFour | RundownFlags.RundownFive)]
        public static class DropServerClientAPI_ExpeditionSuccessAsyncPatch
        {
            public static bool Prefix(ExpeditionSuccessRequest request, ref IL2Tasks.Task<ExpeditionSuccessResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(ExpeditionSuccessRequest)}: Request:{request}");

                CustomProgressionManager.Instance.CompleteCurrentActiveExpedition();

                CustomProgressionManager.ProgressionMerger.MergeIntoLocalRundownProgression();


                var result = new ExpeditionSuccessResult();

                __result = IL2Tasks.Task.FromResult(result);

                return false;
            }
        }
    }
}
