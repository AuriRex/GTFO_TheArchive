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

                OldLocalProgressionManager.Instance.CompleteCurrentActiveExpedition();

                OldLocalProgressionManager.ProgressionMerger.MergeIntoLocalRundownProgression();


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

                var bipd = (DropServer.BoosterImplantPlayerData) LocalBoosterManager.Instance.GetBoosterImplantPlayerData(request.MaxBackendTemplateId);

                var result = new GetBoosterImplantPlayerDataResult();

                Utilities.Il2CppUtils.SetFieldUnsafe(result, bipd, nameof(GetBoosterImplantPlayerDataResult.Data));

                __result = IL2Tasks.Task.FromResult(result);
                return false;
            }

            public static void Postfix(ref IL2Tasks.Task<GetBoosterImplantPlayerDataResult> __result)
            {
                var result = __result.Result;
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> received {nameof(GetBoosterImplantPlayerDataResult)}: Data:{result.Data}");
            }
        }

        // public unsafe Task<UpdateBoosterImplantPlayerDataResult> UpdateBoosterImplantPlayerDataAsync(UpdateBoosterImplantPlayerDataRequest request)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.UpdateBoosterImplantPlayerDataAsync), RundownFlags.RundownFive, RundownFlags.Latest)]
        public static class DropServerClientAPI_UpdateBoosterImplantPlayerDataAsyncPatch
        {
            // called everytime a new booster is selected for the first time to update the value / missed boosters are aknowledged / a booster has been dropped
            public static bool Prefix(UpdateBoosterImplantPlayerDataRequest request, ref IL2Tasks.Task<UpdateBoosterImplantPlayerDataResult> __result)
            {
                var str = "";// BoosterJustPrintThatShit.Transaction(request.Transaction);
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(UpdateBoosterImplantPlayerDataRequest)}: Transaction:<{str}>");

                var result = new UpdateBoosterImplantPlayerDataResult();

                var value = (DropServer.BoosterImplantPlayerData) LocalBoosterManager.Instance.UpdateBoosterImplantPlayerData(request.Transaction);
                Utilities.Il2CppUtils.SetFieldUnsafe(result, value, nameof(UpdateBoosterImplantPlayerDataResult.Data));

                __result = IL2Tasks.Task.FromResult(result);
                return false;
            }

            public static void Postfix(ref IL2Tasks.Task<UpdateBoosterImplantPlayerDataResult> __result)
            {
                var result = __result.Result;
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> received {nameof(UpdateBoosterImplantPlayerDataResult)}: Data:{result.Data}");
            }
        }

        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.EndSessionAsync), RundownFlags.RundownFive, RundownFlags.Latest)]
        public static class DropServerClientAPI_EndSessionAsyncPatch
        {
            public static bool Prefix(EndSessionRequest request, ref IL2Tasks.Task<EndSessionResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(EndSessionRequest)}: EntityToken:{request.EntityToken}, SessionBlob:{request.SessionBlob}, BoosterCurrency:{request.BoosterCurrency}, MaxBackendBoosterTemplateId:{request.MaxBackendBoosterTemplateId}, Success:{request.Success}");

                if (request.Success)
                {
                    OldLocalProgressionManager.Instance.CompleteCurrentActiveExpedition();

                    OldLocalProgressionManager.ProgressionMerger.MergeIntoLocalRundownProgression();
                }

                //request.BoosterCurrency
                // Add ^ those values to the Currency of the respective category
                var result = new EndSessionResult();

                LocalBoosterManager.Instance.EndSession(request.BoosterCurrency, request.Success, request.SessionBlob, request.MaxBackendBoosterTemplateId, request.BuildRev);

                __result = IL2Tasks.Task.FromResult(result);
                return false;
            }

            public static void Postfix(ref IL2Tasks.Task<EndSessionResult> __result)
            {
                var result = __result.Result;

                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> received {nameof(EndSessionResult)}");
            }
        }
    }
}
