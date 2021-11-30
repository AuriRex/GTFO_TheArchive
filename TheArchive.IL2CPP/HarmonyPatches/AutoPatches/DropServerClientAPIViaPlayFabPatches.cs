using DropServer;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Managers;
using TheArchive.Models;
using TheArchive.Utilities;
using UnhollowerRuntimeLib;
using static TheArchive.Core.ArchivePatcher;
using static TheArchive.Utilities.Il2CppUtils;
using static TheArchive.Utilities.Utils;
using IL2Tasks = Il2CppSystem.Threading.Tasks;

namespace TheArchive.HarmonyPatches.AutoPatches
{
    public class DropServerClientAPIViaPlayFabPatches
    {
        public static bool EnableCustomProgressionPatch { get; set; } = true;
        public static bool EnableCustomBoosterProgressionPatch { get; set; } = true;

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

        // public unsafe Task<GetBoosterImplantPlayerDataResult> GetBoosterImplantPlayerDataAsync(GetBoosterImplantPlayerDataRequest request)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.GetBoosterImplantPlayerDataAsync), RundownFlags.RundownFive)]
        public static class DropServerClientAPI_GetBoosterImplantPlayerDataAsyncPatch
        {
            public static bool Prefix(GetBoosterImplantPlayerDataRequest request, ref IL2Tasks.Task<GetBoosterImplantPlayerDataResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(GetBoosterImplantPlayerDataRequest)}: EntityToken:{request.EntityToken}, MaxBackendTemplateId:{request.MaxBackendTemplateId}");

                if(EnableCustomBoosterProgressionPatch)
                {
                    var bipd = CustomBoosterManager.Instance.GetBoosterImplantPlayerData(request.MaxBackendTemplateId);

                    var result = new GetBoosterImplantPlayerDataResult();

                    // NativeFieldInfoPtr_Data
                    Utilities.Il2CppUtils.SetFieldUnsafe(result, bipd, nameof(GetBoosterImplantPlayerDataResult.Data));
                    //result.Data = bipd;

                    __result = IL2Tasks.Task.FromResult(result);
                    return false;
                }

                __result = NullTask<GetBoosterImplantPlayerDataResult>();
                return true;
            }

            public static void Postfix(ref IL2Tasks.Task<GetBoosterImplantPlayerDataResult> __result)
            {
                var result = __result.Result;
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> received {nameof(GetBoosterImplantPlayerDataResult)}: Data:{result.Data}");
            }
        }

        // public unsafe Task<UpdateBoosterImplantPlayerDataResult> UpdateBoosterImplantPlayerDataAsync(UpdateBoosterImplantPlayerDataRequest request)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.UpdateBoosterImplantPlayerDataAsync), RundownFlags.RundownFive)]
        public static class DropServerClientAPI_UpdateBoosterImplantPlayerDataAsyncPatch
        {
            // called everytime a new booster is selected for the first time to update the value / missed boosters are aknowledged / a booster has been dropped
            public static bool Prefix(UpdateBoosterImplantPlayerDataRequest request, ref IL2Tasks.Task<UpdateBoosterImplantPlayerDataResult> __result)
            {
                var str = BoosterJustPrintThatShit.Transaction(request.Transaction);
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(UpdateBoosterImplantPlayerDataRequest)}: Transaction:<{str}>");

                if(EnableCustomBoosterProgressionPatch)
                {
                    var result = new UpdateBoosterImplantPlayerDataResult();

                    var value = CustomBoosterManager.Instance.UpdateBoosterImplantPlayerData(request.Transaction);
                    Utilities.Il2CppUtils.SetFieldUnsafe(result, value, nameof(UpdateBoosterImplantPlayerDataResult.Data));
                    // result.Data = 

                    __result = IL2Tasks.Task.FromResult(result);
                    return false;
                }

                __result = NullTask<UpdateBoosterImplantPlayerDataResult>();
                return true;
            }

            public static void Postfix(ref IL2Tasks.Task<UpdateBoosterImplantPlayerDataResult> __result)
            {
                var result = __result.Result;
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> received {nameof(UpdateBoosterImplantPlayerDataResult)}: Data:{result.Data}");
                var str = BoosterJustPrintThatShit.GetJSON(result.Data);
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, str);
            }
        }

        // public unsafe Task<ConsumeBoostersResult> ConsumeBoostersAsync(ConsumeBoostersRequest request)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.ConsumeBoostersAsync), RundownFlags.RundownFive)]
        public static class DropServerClientAPI_ConsumeBoostersAsyncPatch
        {
            public static bool Prefix(ConsumeBoostersRequest request, ref IL2Tasks.Task<ConsumeBoostersResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.Red, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(ConsumeBoostersRequest)}: EntityToken:{request.EntityToken}, SessionBlob:{request.SessionBlob}");

                if(EnableCustomBoosterProgressionPatch)
                {
                    var result = new ConsumeBoostersResult();

                    CustomBoosterManager.Instance.ConsumeBoosters(request.SessionBlob);

                    result.SessionBlob = request.SessionBlob;

                    __result = IL2Tasks.Task.FromResult(result);
                    return false;
                }

                __result = NullTask<ConsumeBoostersResult>();
                return true;
            }

            public static void Postfix(ref IL2Tasks.Task<ConsumeBoostersResult> __result)
            {
                var result = __result.Result;
                ArchiveLogger.Msg(ConsoleColor.DarkRed, $"{nameof(DropServerClientAPIViaPlayFab)} -> received {nameof(ConsumeBoostersResult)}: SessionBlob:{result.SessionBlob}");
            }
        }

        // public unsafe Task<EndSessionResult> EndSessionAsync(EndSessionRequest request)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.EndSessionAsync), RundownFlags.RundownFive)]
        public static class DropServerClientAPI_EndSessionAsyncPatch
        {
            public static bool Prefix(EndSessionRequest request, ref IL2Tasks.Task<EndSessionResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(EndSessionRequest)}: EntityToken:{request.EntityToken}, SessionBlob:{request.SessionBlob}, BoosterCurrency:{request.BoosterCurrency}, MaxBackendBoosterTemplateId:{request.MaxBackendBoosterTemplateId}, Success:{request.Success}");

                if (EnableCustomProgressionPatch)
                {
                    if (request.Success)
                    {
                        CustomProgressionManager.Instance.CompleteCurrentActiveExpedition();

                        CustomProgressionManager.ProgressionMerger.MergeIntoLocalRundownProgression();
                    }
                }

                //request.BoosterCurrency
                // Add ^ those values to the Currency of the respective category
                if (EnableCustomBoosterProgressionPatch)
                {
                    var result = new EndSessionResult();

                    CustomBoosterManager.Instance.EndSession(request.BoosterCurrency, request.Success, request.SessionBlob, request.MaxBackendBoosterTemplateId, request.BuildRev);

                    __result = IL2Tasks.Task.FromResult(result);
                    return false;
                }

                __result = NullTask<EndSessionResult>();
                return true;
            }

            public static void Postfix(ref IL2Tasks.Task<EndSessionResult> __result)
            {
                var result = __result.Result;
                
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> received {nameof(EndSessionResult)}");
            }
        }

        // ---------------------------------------------------------------

        /*[HarmonyPatch(typeof(DropServerClientAPIViaPlayFab))]
        [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(string), typeof(bool) })] // Rundown 5 added a second parameter bool isDeveloper which is not present in R4 ...
        public static class Test2_DropServerClientAPIViaPlayFab
        {
            public static void Postfix()
            {
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} has been constructed!");
            }
        }*/

        [HarmonyPatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.NewSessionAsync))]
        public static class DropServerClientAPI_NewSessionAsyncPatch
        {
            public static bool Prefix(NewSessionRequest request, ref IL2Tasks.Task<NewSessionResult> __result)
            {
                if(request != null)
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(NewSessionRequest)}: Expedition:{request.Expedition}, Rundown:{request.Rundown}, SessionId:{request.SessionId}");

                
                if(ArchiveMod.CurrentRundown != RundownID.RundownFour)
                {
                    RundownFiveBoosterStartSession(request);
                }

                if(EnableCustomProgressionPatch)
                {
                    CustomProgressionManager.Instance.StartNewExpeditionSession(request.Rundown, request.Expedition, request.SessionId);
                }

                if (EnableCustomProgressionPatch || EnableCustomBoosterProgressionPatch)
                {
                    var ns = new NewSessionResult();

                    __result = IL2Tasks.Task.FromResult<NewSessionResult>(ns);

                    return false;
                }

                return true;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void RundownFiveBoosterStartSession(NewSessionRequest request)
            {
                if (EnableCustomBoosterProgressionPatch)
                {
                    CustomBoosterManager.Instance.StartSession(request.BoosterIds.ToArray(), request.SessionId);
                }
            }
        }

        [HarmonyPatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.LayerProgressionAsync))]
        public static class DropServerClientAPI_LayerProgressionAsyncPatch
        {
            public static bool Prefix(LayerProgressionRequest request, ref IL2Tasks.Task<LayerProgressionResult> __result)
            {
                if(request != null)
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(LayerProgressionRequest)}: Layer:{request.Layer} ,LayerProgressionState:{request.LayerProgressionState}");

                if (EnableCustomProgressionPatch)
                {
                    Enum.TryParse(request.LayerProgressionState, out DropServer.LayerProgressionState layerProgressionState);

                    Enum.TryParse(request.Layer, out DropServer.ExpeditionLayers expeditionLayer);

                    CustomProgressionManager.Instance.SetLayeredDifficultyObjectiveState(expeditionLayer, layerProgressionState);

                    var result = new LayerProgressionResult();



                    __result = IL2Tasks.Task.FromResult<LayerProgressionResult>(result);

                    return false;
                }

                return true;
            }
        }

        //public unsafe Task<ExpeditionSuccessResult> ExpeditionSuccessAsync(ExpeditionSuccessRequest request, [Optional] RequestContext context)
        [HarmonyPatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.ExpeditionSuccessAsync))]
        public static class DropServerClientAPI_ExpeditionSuccessAsyncPatch
        {
            public static bool Prefix(ExpeditionSuccessRequest request, ref IL2Tasks.Task<ExpeditionSuccessResult> __result)
            {
                if(request != null)
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(ExpeditionSuccessRequest)}: Request:{request}");

                if (EnableCustomProgressionPatch)
                {
                    CustomProgressionManager.Instance.CompleteCurrentActiveExpedition();

                    CustomProgressionManager.ProgressionMerger.MergeIntoLocalRundownProgression();


                    var result = new ExpeditionSuccessResult();

                    __result = IL2Tasks.Task.FromResult(result);

                    return false;
                }

                return true; 
            }
        }

        //public unsafe Task<RundownProgressionResult> RundownProgressionAsync(RundownProgressionRequest request, [Optional] RequestContext context)
        [HarmonyPatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.RundownProgressionAsync))]
        public static class DropServerClientAPI_RundownProgressionAsyncPatch
        {
            public static bool Prefix(RundownProgressionRequest request, ref IL2Tasks.Task<RundownProgressionResult> __result)
            {
                if(request != null)
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(RundownProgressionRequest)}: Rundown:{request.Rundown}");

                if (EnableCustomProgressionPatch)
                {
                    var result = new RundownProgressionResult();

                    __result = IL2Tasks.Task.FromResult(result);

                    return false;
                }

                return true; 
            }
        }

        //public unsafe Task<ClearRundownProgressionResult> ClearRundownProgressionAsync(ClearRundownProgressionRequest request, [Optional] RequestContext context)
        [HarmonyPatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.ClearRundownProgressionAsync))]
        public static class DropServerClientAPI_ClearRundownProgressionAsyncPatch
        {
            public static bool Prefix(ClearRundownProgressionRequest request, ref IL2Tasks.Task<ClearRundownProgressionResult> __result)
            {
                if(request != null)
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(ClearRundownProgressionRequest)}: Rundown:{request.Rundown}");
                if (EnableCustomProgressionPatch)
                {
                    var result = new ClearRundownProgressionResult();

                    __result = IL2Tasks.Task.FromResult(result);

                    return false;
                }

                return true;
            }
        }

        //public unsafe Task<IsTesterResult> IsTesterAsync(IsTesterRequest request, [Optional] RequestContext context)
        [HarmonyPatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.IsTesterAsync))]
        public static class DropServerClientAPI_IsTesterAsyncPatch
        {
            public static bool Prefix(IsTesterRequest request, ref IL2Tasks.Task<IsTesterResult> __result)
            {
                if(request != null)
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(IsTesterRequest)} (this should not happen I think?): SteamId:{request.SteamId}");

                if (EnableCustomProgressionPatch)
                {
                    var result = new IsTesterResult();

                    result.IsTester = false;

                    __result = IL2Tasks.Task.FromResult(result);

                    return false;
                }

                return true; 
            }
        }

        //public unsafe Task<AddResult> AddAsync(AddRequest request, [Optional] RequestContext context)
        [HarmonyPatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.AddAsync))]
        public static class DropServerClientAPI_AddAsyncPatch
        {
            public static bool Prefix(AddRequest request, ref IL2Tasks.Task<AddResult> __result)
            {
                if(request != null)
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(AddRequest)}: X:{request.X}, Y:{request.Y}, Sum={(request.X+request.Y)}");

                if (EnableCustomProgressionPatch)
                {
                    var result = new AddResult();

                    result.Sum = request.X + request.Y;

                    __result = IL2Tasks.Task.FromResult(result);

                    return false;
                }

                return true;
            }
        }
    }
}
