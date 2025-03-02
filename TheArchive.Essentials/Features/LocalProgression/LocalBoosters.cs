using System;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using static TheArchive.Utilities.Utils;
#if IL2CPP
using TheArchive.Managers;
using BoosterImplants;
using DropServer;
using IL2Tasks = Il2CppSystem.Threading.Tasks;
#endif

namespace TheArchive.Features.LocalProgression;

[RundownConstraint(RundownFlags.RundownFive, RundownFlags.Latest)]
[HideInModSettings]
[DoNotSaveToConfig]
[AutomatedFeature]
public class LocalBoosters : Feature
{
    public override string Name => "Local Boosters";

    public override FeatureGroup Group => FeatureGroups.LocalProgression;

    public static new IArchiveLogger FeatureLogger { get; set; }

#if IL2CPP
    [ArchivePatch(nameof(ArtifactInventory.OnStateChange))]
    public static class ArtifactInventory_OnStateChange_Patch
    {
        public static Type Type() => typeof(ArtifactInventory);
        public static void Postfix(ArtifactInventory __instance)
        {
            LocalProgressionManager.Instance.ArtifactCountUpdated(__instance.CommonCount + __instance.RareCount + __instance.UncommonCount);
        }
    }

    [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.GetBoosterImplantPlayerDataAsync))]
    public static class DropServerClientAPI_GetBoosterImplantPlayerDataAsync_Patch
    {
        private static PropertyInfo Data;
        public static void Init()
        {
            Data = typeof(GetBoosterImplantPlayerDataResult).GetProperty(nameof(GetBoosterImplantPlayerDataResult.Data), AnyBindingFlagss);
        }

        public static bool Prefix(GetBoosterImplantPlayerDataRequest request, ref IL2Tasks.Task<GetBoosterImplantPlayerDataResult> __result)
        {
            FeatureLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(GetBoosterImplantPlayerDataRequest)}: EntityToken:{request.EntityToken}, MaxBackendTemplateId:{request.MaxBackendTemplateId}");

            var bipd = LocalBoosterManager.Instance.GetBoosterImplantPlayerData(request.MaxBackendTemplateId);

            var result = new GetBoosterImplantPlayerDataResult();

            Data.SetValue(result, bipd);

            __result = IL2Tasks.Task.FromResult(result);
            return ArchivePatch.SKIP_OG;
        }
    }

    [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.UpdateBoosterImplantPlayerDataAsync))]
    public static class DropServerClientAPI_UpdateBoosterImplantPlayerDataAsync_Patch
    {
        private static PropertyInfo Transaction;
        private static PropertyInfo Data;
        public static void Init()
        {
            Transaction = typeof(UpdateBoosterImplantPlayerDataRequest).GetProperty(nameof(UpdateBoosterImplantPlayerDataRequest.Transaction), AnyBindingFlagss);
            Data = typeof(UpdateBoosterImplantPlayerDataResult).GetProperty(nameof(UpdateBoosterImplantPlayerDataResult.Data), AnyBindingFlagss);
        }

        // called everytime a new booster is selected for the first time to update the value / missed boosters are aknowledged / a booster has been dropped
        public static bool Prefix(UpdateBoosterImplantPlayerDataRequest request, ref IL2Tasks.Task<UpdateBoosterImplantPlayerDataResult> __result)
        {
            try
            {
                FeatureLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(UpdateBoosterImplantPlayerDataRequest)}");

                var result = new UpdateBoosterImplantPlayerDataResult();

                // Reflection here because funny type-namespace-was-renamed-in-r6-moment :)
                var transaction = Transaction.GetValue(request);

                var bipd = LocalBoosterManager.Instance.UpdateBoosterImplantPlayerData(transaction);

                Data.SetValue(result, bipd);

                __result = IL2Tasks.Task.FromResult(result);
            }
            catch(Exception ex)
            {
                FeatureLogger.Exception(ex);
            }
                
            return ArchivePatch.SKIP_OG;
        }
    }

    [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.ConsumeBoostersAsync))]
    public static class DropServerClientAPI_ConsumeBoostersAsyncPatch
    {
        public static bool Prefix(ConsumeBoostersRequest request, ref IL2Tasks.Task<ConsumeBoostersResult> __result)
        {
            FeatureLogger.Msg(ConsoleColor.Red, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(ConsumeBoostersRequest)}: EntityToken:{request.EntityToken}, SessionBlob:{request.SessionBlob}");

            var result = new ConsumeBoostersResult();

            LocalBoosterManager.Instance.ConsumeBoosters(request.SessionBlob);

            result.SessionBlob = request.SessionBlob;

            __result = IL2Tasks.Task.FromResult(result);
            return false;
        }
    }

    [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.EndSessionAsync))]
    public static class DropServerClientAPI_EndSessionAsync_Patch
    {
        public static bool Prefix(EndSessionRequest request, ref IL2Tasks.Task<EndSessionResult> __result)
        {
            FeatureLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(EndSessionRequest)}: EntityToken:{request.EntityToken}, SessionBlob:{request.SessionBlob}, BoosterCurrency:{request.BoosterCurrency}, MaxBackendBoosterTemplateId:{request.MaxBackendBoosterTemplateId}, Success:{request.Success}");

            LocalProgressionManager.Instance.EndCurrentExpeditionSession(request.Success);

            LocalBoosterManager.Instance.EndSession(request.BoosterCurrency, request.Success, request.SessionBlob, request.MaxBackendBoosterTemplateId, request.BuildRev);

            __result = IL2Tasks.Task.FromResult(new EndSessionResult());
            return ArchivePatch.SKIP_OG;
        }
    }
#endif
}