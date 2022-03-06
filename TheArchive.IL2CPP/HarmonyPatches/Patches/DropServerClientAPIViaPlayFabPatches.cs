using DropServer;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using TheArchive.Core;
using TheArchive.Managers;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;
using static TheArchive.Utilities.Utils;
using IL2Tasks = Il2CppSystem.Threading.Tasks;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableLocalProgressionPatches), "LocalProgression")]
    public class DropServerClientAPIViaPlayFabPatches
    {
        // Rundown 4
        // public unsafe Task<ExpeditionSuccessResult> ExpeditionSuccessAsync(ExpeditionSuccessRequest request, [Optional] RequestContext context)
        // public unsafe Task<LayerProgressionResult> LayerProgressionAsync(LayerProgressionRequest request, [Optional] RequestContext context)
        // public unsafe Task<NewSessionResult> NewSessionAsync(NewSessionRequest request, [Optional] RequestContext context)
        // public unsafe Task<ClearRundownProgressionResult> ClearRundownProgressionAsync(ClearRundownProgressionRequest request, [Optional] RequestContext context)
        // public unsafe Task<RundownProgressionResult> RundownProgressionAsync(RundownProgressionRequest request, [Optional] RequestContext context)
        // public unsafe Task<IsTesterResult> IsTesterAsync(IsTesterRequest request, [Optional] RequestContext context)
        // public unsafe Task<AddResult> AddAsync(AddRequest request, [Optional] RequestContext context)

        // Rundown 5
        // -public unsafe Task<AddResult> AddAsync(AddRequest request)
        // -public unsafe Task<IsTesterResult> IsTesterAsync(IsTesterRequest request)
        // -public unsafe Task<RundownProgressionResult> RundownProgressionAsync(RundownProgressionRequest request)
        // -public unsafe Task<ClearRundownProgressionResult> ClearRundownProgressionAsync(ClearRundownProgressionRequest request)
        // +public unsafe Task<GetBoosterImplantPlayerDataResult> GetBoosterImplantPlayerDataAsync(GetBoosterImplantPlayerDataRequest request)
        // +public unsafe Task<UpdateBoosterImplantPlayerDataResult> UpdateBoosterImplantPlayerDataAsync(UpdateBoosterImplantPlayerDataRequest request)
        // public unsafe Task<DebugBoosterImplantResult> DebugBoosterImplantAsync(DebugBoosterImplantRequest request)
        // -public unsafe Task<NewSessionResult> NewSessionAsync(NewSessionRequest request)
        // -public unsafe Task<LayerProgressionResult> LayerProgressionAsync(LayerProgressionRequest request)
        // +public unsafe Task<ConsumeBoostersResult> ConsumeBoostersAsync(ConsumeBoostersRequest request)
        // -public unsafe Task<ExpeditionSuccessResult> ExpeditionSuccessAsync(ExpeditionSuccessRequest request)
        // +public unsafe Task<EndSessionResult> EndSessionAsync(EndSessionRequest request)

        // Rundown 6
        // -public Task<AddResult> AddAsync(AddRequest request)
        // -public Task<IsTesterResult> IsTesterAsync(IsTesterRequest request)
        // -public Task<RundownProgressionResult> RundownProgressionAsync(RundownProgressionRequest request)
        // -public Task<ClearRundownProgressionResult> ClearRundownProgressionAsync(ClearRundownProgressionRequest request)
        // -public Task<GetBoosterImplantPlayerDataResult> GetBoosterImplantPlayerDataAsync(GetBoosterImplantPlayerDataRequest request)
        // -public Task<UpdateBoosterImplantPlayerDataResult> UpdateBoosterImplantPlayerDataAsync(UpdateBoosterImplantPlayerDataRequest request)
        // -public Task<DebugBoosterImplantResult> DebugBoosterImplantAsync(DebugBoosterImplantRequest request)
        // -public Task<NewSessionResult> NewSessionAsync(NewSessionRequest request)
        // -public Task<LayerProgressionResult> LayerProgressionAsync(LayerProgressionRequest request)
        // -public Task<ConsumeBoostersResult> ConsumeBoostersAsync(ConsumeBoostersRequest request)
        // -public Task<EndSessionResult> EndSessionAsync(EndSessionRequest request)
        // REMOVED ExpeditionSuccessResult
        // +public Task<GetInventoryPlayerDataResult> GetInventoryPlayerDataAsync(GetInventoryPlayerDataRequest request)
        // +public Task<UpdateVanityItemPlayerDataResult> UpdateVanityItemPlayerDataAsync(UpdateVanityItemPlayerDataRequest request)
        // +public Task<DebugVanityItemResult> DebugVanityItemAsync(DebugVanityItemRequest request)

        // ----------------------------------------

        // public unsafe Task<GetBoosterImplantPlayerDataResult> GetBoosterImplantPlayerDataAsync(GetBoosterImplantPlayerDataRequest request)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.GetBoosterImplantPlayerDataAsync), RundownFlags.RundownSix, RundownFlags.Latest)]
        public static class DropServerClientAPI_GetBoosterImplantPlayerDataAsyncPatch
        {
            // R5 version has been moved into submodule.
            public static bool Prefix(GetBoosterImplantPlayerDataRequest request, ref IL2Tasks.Task<GetBoosterImplantPlayerDataResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(GetBoosterImplantPlayerDataRequest)}: EntityToken:{request.EntityToken}, MaxBackendTemplateId:{request.MaxBackendTemplateId}");

                var bipd = (DropServer.BoosterImplants.BoosterImplantPlayerData) CustomBoosterManager.Instance.GetBoosterImplantPlayerData(request.MaxBackendTemplateId);

                var result = new GetBoosterImplantPlayerDataResult();

                // NativeFieldInfoPtr_Data
                Il2CppUtils.SetFieldUnsafe(result, bipd, nameof(GetBoosterImplantPlayerDataResult.Data));
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

        // public unsafe Task<UpdateBoosterImplantPlayerDataResult> UpdateBoosterImplantPlayerDataAsync(UpdateBoosterImplantPlayerDataRequest request)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.UpdateBoosterImplantPlayerDataAsync), RundownFlags.RundownSix, RundownFlags.Latest)]
        public static class DropServerClientAPI_UpdateBoosterImplantPlayerDataAsyncPatch
        {
            // R5 version has been moved into submodule.
            // called everytime a new booster is selected for the first time to update the value / missed boosters are aknowledged / a booster has been dropped
            public static bool Prefix(UpdateBoosterImplantPlayerDataRequest request, ref IL2Tasks.Task<UpdateBoosterImplantPlayerDataResult> __result)
            {
                var str = "";// BoosterJustPrintThatShit.Transaction(request.Transaction);
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(UpdateBoosterImplantPlayerDataRequest)}: Transaction:<{str}>");

                var result = new UpdateBoosterImplantPlayerDataResult();

                var value = (DropServer.BoosterImplants.BoosterImplantPlayerData) CustomBoosterManager.Instance.UpdateBoosterImplantPlayerData(request.Transaction);
                Il2CppUtils.SetFieldUnsafe(result, value, nameof(UpdateBoosterImplantPlayerDataResult.Data));

                __result = IL2Tasks.Task.FromResult(result);
                return false;
            }

            public static void Postfix(ref IL2Tasks.Task<UpdateBoosterImplantPlayerDataResult> __result)
            {
                var result = __result.Result;
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> received {nameof(UpdateBoosterImplantPlayerDataResult)}: Data:{result.Data}");
            }
        }

        // public unsafe Task<ConsumeBoostersResult> ConsumeBoostersAsync(ConsumeBoostersRequest request)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.ConsumeBoostersAsync), RundownFlags.RundownFive, RundownFlags.Latest)]
        public static class DropServerClientAPI_ConsumeBoostersAsyncPatch
        {
            public static bool Prefix(ConsumeBoostersRequest request, ref IL2Tasks.Task<ConsumeBoostersResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.Red, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(ConsumeBoostersRequest)}: EntityToken:{request.EntityToken}, SessionBlob:{request.SessionBlob}");

                var result = new ConsumeBoostersResult();

                CustomBoosterManager.Instance.ConsumeBoosters(request.SessionBlob);

                result.SessionBlob = request.SessionBlob;

                __result = IL2Tasks.Task.FromResult(result);
                return false;
            }

            public static void Postfix(ref IL2Tasks.Task<ConsumeBoostersResult> __result)
            {
                var result = __result.Result;
                ArchiveLogger.Msg(ConsoleColor.DarkRed, $"{nameof(DropServerClientAPIViaPlayFab)} -> received {nameof(ConsumeBoostersResult)}: SessionBlob:{result.SessionBlob}");
            }
        }

        // public unsafe Task<EndSessionResult> EndSessionAsync(EndSessionRequest request)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.EndSessionAsync), RundownFlags.RundownSix, RundownFlags.Latest)]
        public static class DropServerClientAPI_EndSessionAsyncPatch
        {
            public static bool Prefix(EndSessionRequest request, ref IL2Tasks.Task<EndSessionResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(EndSessionRequest)}: EntityToken:{request.EntityToken}, SessionBlob:{request.SessionBlob}, BoosterCurrency:{request.BoosterCurrency}, MaxBackendBoosterTemplateId:{request.MaxBackendBoosterTemplateId}, Success:{request.Success}");

                if (request.Success)
                {
                    CustomProgressionManager.Instance.CompleteCurrentActiveExpedition();

                    CustomProgressionManager.ProgressionMerger.MergeIntoLocalRundownProgression();
                }

                //request.BoosterCurrency
                // Add ^ those values to the Currency of the respective category
                var result = new EndSessionResult();

                CustomBoosterManager.Instance.EndSession(request.BoosterCurrency, request.Success, request.SessionBlob, request.MaxBackendBoosterTemplateId, request.BuildRev);

                __result = IL2Tasks.Task.FromResult(result);
                return false;
            }

            public static void Postfix(ref IL2Tasks.Task<EndSessionResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> received {nameof(EndSessionResult)}");
            }
        }

        // ---------------------------------------------------------------

        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.NewSessionAsync))]
        public static class DropServerClientAPI_NewSessionAsyncPatch
        {
            public static bool Prefix(NewSessionRequest request, ref IL2Tasks.Task<NewSessionResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(NewSessionRequest)}: Expedition:{request.Expedition}, Rundown:{request.Rundown}, SessionId:{request.SessionId}");

                if (ArchiveMod.CurrentRundown != RundownID.RundownFour)
                {
                    StartBoostersSession(request);
                }

                CustomProgressionManager.Instance.StartNewExpeditionSession(request.Rundown, request.Expedition, request.SessionId);

                var ns = new NewSessionResult();

                __result = IL2Tasks.Task.FromResult<NewSessionResult>(ns);

                return false;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void StartBoostersSession(NewSessionRequest request)
            {
                CustomBoosterManager.Instance.StartSession(request.BoosterIds.ToArray(), request.SessionId);
            }
        }

        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.LayerProgressionAsync))]
        public static class DropServerClientAPI_LayerProgressionAsyncPatch
        {
            public static bool Prefix(LayerProgressionRequest request, ref IL2Tasks.Task<LayerProgressionResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(LayerProgressionRequest)}: Layer:{request.Layer} ,LayerProgressionState:{request.LayerProgressionState}");

                Enum.TryParse(request.LayerProgressionState, out DropServer.LayerProgressionState layerProgressionState);

                Enum.TryParse(request.Layer, out DropServer.ExpeditionLayers expeditionLayer);

                CustomProgressionManager.Instance.SetLayeredDifficultyObjectiveState(expeditionLayer, layerProgressionState);

                __result = IL2Tasks.Task.FromResult<LayerProgressionResult>(new LayerProgressionResult());

                return false;
            }
        }

        // Does not appear to be called in R5 anymore, removed in R6
        //public unsafe Task<ExpeditionSuccessResult> ExpeditionSuccessAsync(ExpeditionSuccessRequest request, [Optional] RequestContext context)
        // Moved to TheArchive.IL2CPP.R5

        //public unsafe Task<RundownProgressionResult> RundownProgressionAsync(RundownProgressionRequest request, [Optional] RequestContext context)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.RundownProgressionAsync))]
        public static class DropServerClientAPI_RundownProgressionAsyncPatch
        {
            public static bool Prefix(RundownProgressionRequest request, ref IL2Tasks.Task<RundownProgressionResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(RundownProgressionRequest)}: Rundown:{request.Rundown}");

                __result = IL2Tasks.Task.FromResult(new RundownProgressionResult());

                return false;
            }
        }

        //public unsafe Task<ClearRundownProgressionResult> ClearRundownProgressionAsync(ClearRundownProgressionRequest request, [Optional] RequestContext context)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.ClearRundownProgressionAsync))]
        public static class DropServerClientAPI_ClearRundownProgressionAsyncPatch
        {
            public static bool Prefix(ClearRundownProgressionRequest request, ref IL2Tasks.Task<ClearRundownProgressionResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(ClearRundownProgressionRequest)}: Rundown:{request.Rundown}");

                __result = IL2Tasks.Task.FromResult(new ClearRundownProgressionResult());

                return false;
            }
        }

        //public unsafe Task<IsTesterResult> IsTesterAsync(IsTesterRequest request, [Optional] RequestContext context)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.IsTesterAsync))]
        public static class DropServerClientAPI_IsTesterAsyncPatch
        {
            public static bool Prefix(IsTesterRequest request, ref IL2Tasks.Task<IsTesterResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(IsTesterRequest)} (this should not happen I think?): SteamId:{request.SteamId}");

                var result = new IsTesterResult();

                result.IsTester = false;

                __result = IL2Tasks.Task.FromResult(result);

                return false;
            }
        }

        //public unsafe Task<AddResult> AddAsync(AddRequest request, [Optional] RequestContext context)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.AddAsync))]
        public static class DropServerClientAPI_AddAsyncPatch
        {
            public static bool Prefix(AddRequest request, ref IL2Tasks.Task<AddResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(AddRequest)}: X:{request.X}, Y:{request.Y}, Sum={(request.X + request.Y)}");

                var result = new AddResult();

                result.Sum = request.X + request.Y;

                __result = IL2Tasks.Task.FromResult(result);

                return false;
            }
        }
    }
}
