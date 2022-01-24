using DropServer;
using System;
using TheArchive.Core;
using TheArchive.Managers;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;
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

        // public unsafe Task<GetBoosterImplantPlayerDataResult> GetBoosterImplantPlayerDataAsync(GetBoosterImplantPlayerDataRequest request)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.GetBoosterImplantPlayerDataAsync), RundownFlags.RundownFive, RundownFlags.Latest)]
        public static class DropServerClientAPI_GetBoosterImplantPlayerDataAsyncPatch
        {
            public static bool Prefix(GetBoosterImplantPlayerDataRequest request, ref IL2Tasks.Task<GetBoosterImplantPlayerDataResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(GetBoosterImplantPlayerDataRequest)}: EntityToken:{request.EntityToken}, MaxBackendTemplateId:{request.MaxBackendTemplateId}");

                var bipd = CustomBoosterManager.Instance.GetBoosterImplantPlayerData(request.MaxBackendTemplateId);

                var result = new GetBoosterImplantPlayerDataResult();

                // NativeFieldInfoPtr_Data
#warning TODO
                //Utilities.Il2CppUtils.SetFieldUnsafe(result, bipd, nameof(GetBoosterImplantPlayerDataResult.Data));
                // old-- result.Data = bipd;

                __result = IL2Tasks.Task.FromResult(result);
                return false;
            }

            public static void Postfix(ref IL2Tasks.Task<GetBoosterImplantPlayerDataResult> __result)
            {
                var result = __result.Result;
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> received {nameof(GetBoosterImplantPlayerDataResult)}: Data:{result.Data}");
            }
        }
    }
}
