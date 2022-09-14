using DropServer;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Managers;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;
#if MONO
using IL2Tasks = System.Threading.Tasks;
using IL2System = System;
#else
using IL2Tasks = Il2CppSystem.Threading.Tasks;
using IL2System = Il2CppSystem;
#endif

namespace TheArchive.Features.LocalProgression
{
    [EnableFeatureByDefault]
    internal class LocalProgressionCore : Feature
    {
        public override string Name => "Local Progression Core";

        public override string Group => FeatureGroups.LocalProgression;

        public override bool RequiresRestart => true;

        public new static IArchiveLogger FeatureLogger { get; set; }

#region comments
        // Rundown 4
        // + public Task<ExpeditionSuccessResult> ExpeditionSuccessAsync(ExpeditionSuccessRequest request, [Optional] RequestContext context)
        // + public Task<LayerProgressionResult> LayerProgressionAsync(LayerProgressionRequest request, [Optional] RequestContext context)
        // + public Task<NewSessionResult> NewSessionAsync(NewSessionRequest request, [Optional] RequestContext context)
        // + public Task<ClearRundownProgressionResult> ClearRundownProgressionAsync(ClearRundownProgressionRequest request, [Optional] RequestContext context)
        // + public Task<RundownProgressionResult> RundownProgressionAsync(RundownProgressionRequest request, [Optional] RequestContext context)
        // + public Task<IsTesterResult> IsTesterAsync(IsTesterRequest request, [Optional] RequestContext context)
        // + public Task<AddResult> AddAsync(AddRequest request, [Optional] RequestContext context)

        // Rundown 5
        //   public unsafe Task<ExpeditionSuccessResult> ExpeditionSuccessAsync(ExpeditionSuccessRequest request)
        //   public unsafe Task<LayerProgressionResult> LayerProgressionAsync(LayerProgressionRequest request)
        //   public unsafe Task<NewSessionResult> NewSessionAsync(NewSessionRequest request)
        //   public unsafe Task<ClearRundownProgressionResult> ClearRundownProgressionAsync(ClearRundownProgressionRequest request)
        //   public unsafe Task<RundownProgressionResult> RundownProgressionAsync(RundownProgressionRequest request)
        //   public unsafe Task<IsTesterResult> IsTesterAsync(IsTesterRequest request)
        //   public unsafe Task<AddResult> AddAsync(AddRequest request)
        // + public unsafe Task<DebugBoosterImplantResult> DebugBoosterImplantAsync(DebugBoosterImplantRequest request)
        // + public unsafe Task<GetBoosterImplantPlayerDataResult> GetBoosterImplantPlayerDataAsync(GetBoosterImplantPlayerDataRequest request)
        // + public unsafe Task<UpdateBoosterImplantPlayerDataResult> UpdateBoosterImplantPlayerDataAsync(UpdateBoosterImplantPlayerDataRequest request)
        // + public unsafe Task<ConsumeBoostersResult> ConsumeBoostersAsync(ConsumeBoostersRequest request)
        // + public unsafe Task<EndSessionResult> EndSessionAsync(EndSessionRequest request)

        // Rundown 6
        // - public Task<ExpeditionSuccessResult> ExpeditionSuccessAsync(ExpeditionSuccessRequest request, [Optional] RequestContext context)
        //   public Task<LayerProgressionResult> LayerProgressionAsync(LayerProgressionRequest request)
        //   public Task<NewSessionResult> NewSessionAsync(NewSessionRequest request)
        //   public Task<ClearRundownProgressionResult> ClearRundownProgressionAsync(ClearRundownProgressionRequest request)
        //   public Task<RundownProgressionResult> RundownProgressionAsync(RundownProgressionRequest request)
        //   public Task<IsTesterResult> IsTesterAsync(IsTesterRequest request)
        //   public Task<AddResult> AddAsync(AddRequest request)
        //   public Task<DebugBoosterImplantResult> DebugBoosterImplantAsync(DebugBoosterImplantRequest request)
        //   public Task<GetBoosterImplantPlayerDataResult> GetBoosterImplantPlayerDataAsync(GetBoosterImplantPlayerDataRequest request)
        //   public Task<UpdateBoosterImplantPlayerDataResult> UpdateBoosterImplantPlayerDataAsync(UpdateBoosterImplantPlayerDataRequest request)
        //   public Task<ConsumeBoostersResult> ConsumeBoostersAsync(ConsumeBoostersRequest request)
        //   public Task<EndSessionResult> EndSessionAsync(EndSessionRequest request)
        // + public Task<GetInventoryPlayerDataResult> GetInventoryPlayerDataAsync(GetInventoryPlayerDataRequest request)
        // + public Task<UpdateVanityItemPlayerDataResult> UpdateVanityItemPlayerDataAsync(UpdateVanityItemPlayerDataRequest request)
        // + public Task<DebugVanityItemResult> DebugVanityItemAsync(DebugVanityItemRequest request)

        // Rundown 7
        // (no changes)
#endregion comments

#region DropServerManager
        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(nameof(DropServerManager.Setup))]
        public static class DropServerManager_Setup_Patch
        {
            public static Type Type() => typeof(DropServerManager);

            public static void Postfix(ref DropServerManager __instance)
            {
                FeatureLogger.Msg(ConsoleColor.Magenta, $"Setting {nameof(DropServerManager)}s TitleDataSettings to localhost.");
                var dstds = new DropServerManager.DropServerTitleDataSettings();
                dstds.ClientApiEndPoint = "https://localhost/api";
                __instance.ApplyTitleDataSettings(dstds);
            }
        }

        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(nameof(DropServerManager.GetRundownProgressionAsync))]
        public static class DropServerManager_GetRundownProgressionAsync_Patch
        {
            public static Type Type() => typeof(DropServerManager);

            public static bool Prefix(ref IL2Tasks.Task<RundownProgression> __result, string rundownName, IL2System.Threading.CancellationToken cancellationToken, IL2System.Action<IL2Tasks.Task<RundownProgression>> callback)
            {
                FeatureLogger.Msg(ConsoleColor.Magenta, "Getting RundownProgression from local files ...");

                try
                {
                    var task = __result = IL2Tasks.Task.FromResult(LocalProgressionManager.CustomRundownProgression.ToBaseGameProgression());

                    if (callback != null)
                    {
                        TenCC.Utils.TaskUtils.ContinueOnCurrentContext<RundownProgression>(task, callback);
                    }
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }

                return ArchivePatch.SKIP_OG;
            }
        }
#endregion DropServerManager

        // Does not appear to be called in R5 anymore, removed in R6
        //public unsafe Task<ExpeditionSuccessResult> ExpeditionSuccessAsync(ExpeditionSuccessRequest request, [Optional] RequestContext context)
        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.RundownFive)]
        [ArchivePatch("ExpeditionSuccessAsync")]
        public static class DropServerClientAPI_ExpeditionSuccessAsync_Patch
        {
            public static Type Type() => typeof(DropServerClientAPIViaPlayFab);

            private static Type _ExpeditionSuccessResultType;
            private static MethodInfo _Task_FromResult;
            public static void Init()
            {
                _ExpeditionSuccessResultType = ImplementationManager.FindTypeInCurrentAppDomain("ExpeditionSuccessResult");
                _Task_FromResult = typeof(IL2Tasks.Task).GetMethod("FromResult", AnyBindingFlagss).MakeGenericMethod(_ExpeditionSuccessResultType);
            }

            public static bool Prefix(object request, ref object __result)
            {
                FeatureLogger.Msg(ConsoleColor.DarkCyan, $"requested ExpeditionSuccessRequest: Request:{request}");
                try
                {
                    LocalProgressionManager.Instance.EndCurrentExpeditionSession(true);

                    var inst = Activator.CreateInstance(_ExpeditionSuccessResultType);

                    __result = _Task_FromResult.Invoke(null, new object[] { inst });
                }
                catch(Exception ex)
                {
                    FeatureLogger.Exception(ex);
                }
                return ArchivePatch.SKIP_OG;
            }
        }

        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(nameof(DropServerClientAPIViaPlayFab.NewSessionAsync))]
        public static class DropServerClientAPI_NewSessionAsync_Patch
        {
            public static Type Type() => typeof(DropServerClientAPIViaPlayFab);

            public static bool Prefix(NewSessionRequest request, ref IL2Tasks.Task<NewSessionResult> __result)
            {
                FeatureLogger.Msg(ConsoleColor.DarkCyan, $"requested {nameof(NewSessionRequest)}: Expedition:{request.Expedition}, Rundown:{request.Rundown}, SessionId:{request.SessionId}");

                if (BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownFive.ToLatest()))
                {
                    StartBoostersSession(request);
                }

                LocalProgressionManager.Instance.StartNewExpeditionSession(request.Rundown, request.Expedition, request.SessionId);

                __result = IL2Tasks.Task.FromResult(new NewSessionResult());

                return ArchivePatch.SKIP_OG;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void StartBoostersSession(NewSessionRequest request)
            {
                LocalBoosterManager.Instance.StartSession(request.BoosterIds.ToArray(), request.SessionId);
            }
        }

        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(nameof(DropServerClientAPIViaPlayFab.LayerProgressionAsync))]
        public static class DropServerClientAPI_LayerProgressionAsync_Patch
        {
            public static Type Type() => typeof(DropServerClientAPIViaPlayFab);

            public static bool Prefix(LayerProgressionRequest request, ref IL2Tasks.Task<LayerProgressionResult> __result)
            {
                FeatureLogger.Msg(ConsoleColor.DarkCyan, $"requested {nameof(LayerProgressionRequest)}: Layer:{request.Layer} ,LayerProgressionState:{request.LayerProgressionState}");

                LocalProgressionManager.Instance.IncreaseLayerProgression(request.Layer, request.LayerProgressionState);

                __result = IL2Tasks.Task.FromResult(new LayerProgressionResult());

                return ArchivePatch.SKIP_OG;
            }
        }

        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(nameof(DropServerClientAPIViaPlayFab.RundownProgressionAsync))]
        public static class DropServerClientAPI_RundownProgressionAsync_Patch
        {
            public static Type Type() => typeof(DropServerClientAPIViaPlayFab);

            public static bool Prefix(RundownProgressionRequest request, ref IL2Tasks.Task<RundownProgressionResult> __result)
            {
                FeatureLogger.Msg(ConsoleColor.DarkCyan, $"requested {nameof(RundownProgressionRequest)}: Rundown:{request.Rundown}");

                __result = IL2Tasks.Task.FromResult(new RundownProgressionResult());

                return ArchivePatch.SKIP_OG;
            }
        }

        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.ClearRundownProgressionAsync))]
        public static class DropServerClientAPI_ClearRundownProgressionAsyncPatch
        {
            public static bool Prefix(ClearRundownProgressionRequest request, ref IL2Tasks.Task<ClearRundownProgressionResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"requested {nameof(ClearRundownProgressionRequest)}: Rundown:{request.Rundown}");

                __result = IL2Tasks.Task.FromResult(new ClearRundownProgressionResult());

                return ArchivePatch.SKIP_OG;
            }
        }

        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.IsTesterAsync))]
        public static class DropServerClientAPI_IsTesterAsyncPatch
        {
            public static bool Prefix(IsTesterRequest request, ref IL2Tasks.Task<IsTesterResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"requested {nameof(IsTesterRequest)} (this should not happen I think?): SteamId:{request.SteamId}");

                var result = new IsTesterResult();

                result.IsTester = false;

                __result = IL2Tasks.Task.FromResult(result);

                return ArchivePatch.SKIP_OG;
            }
        }

        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.AddAsync))]
        public static class DropServerClientAPI_AddAsyncPatch
        {
            public static bool Prefix(AddRequest request, ref IL2Tasks.Task<AddResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkCyan, $"requested {nameof(AddRequest)}: X:{request.X}, Y:{request.Y}, Sum={(request.X + request.Y)}");

                var result = new AddResult();

                result.Sum = request.X + request.Y;

                __result = IL2Tasks.Task.FromResult(result);

                return ArchivePatch.SKIP_OG;
            }
        }

#region checkpoint
        [RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
        [ArchivePatch(nameof(CheckpointManager.StoreCheckpoint))]
        public class CheckpointManager_StoreCheckpoint_Patch
        {
            public static Type Type() => typeof(CheckpointManager);
            public static void Prefix()
            {
                FeatureLogger.Notice($"Saving checkpoint.");
            }
        }

        [RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
        [ArchivePatch(nameof(CheckpointManager.ReloadCheckpoint))]
        public class CheckpointManager_ReloadCheckpoint_Patch
        {
            public static Type Type() => typeof(CheckpointManager);
            public static void Prefix()
            {
                FeatureLogger.Notice($"Reloading checkpoint.");
            }
        }
#endregion checkpoint
    }
}
