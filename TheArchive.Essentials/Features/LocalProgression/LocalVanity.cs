using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;
using System.Reflection;
#if IL2CPP
using DropServer;
using DropServer.VanityItems;
using TheArchive.Managers;
using IL2Tasks = Il2CppSystem.Threading.Tasks;
#endif

namespace TheArchive.Features.LocalProgression;

[RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
[HideInModSettings]
[DoNotSaveToConfig]
[AutomatedFeature]
public class LocalVanity : Feature
{
    public override string Name => "Local Vanity";

    public override FeatureGroup Group => FeatureGroups.LocalProgression;

#if IL2CPP
    [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.GetInventoryPlayerDataAsync))]
    public static class DropServerClientAPI_GetInventoryPlayerDataAsync_Patch
    {
        private static PropertyInfo Boosters;
        private static PropertyInfo VanityItems;
        public static void Init()
        {
            Boosters = typeof(GetInventoryPlayerDataResult).GetProperty(nameof(GetInventoryPlayerDataResult.Boosters), AnyBindingFlagss);
            VanityItems = typeof(GetInventoryPlayerDataResult).GetProperty(nameof(GetInventoryPlayerDataResult.VanityItems), AnyBindingFlagss);
        }

        public static bool Prefix(GetInventoryPlayerDataRequest request, ref IL2Tasks.Task<GetInventoryPlayerDataResult> __result)
        {
            ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(GetInventoryPlayerDataRequest)}: EntityToken:{request.EntityToken}, MaxBackendTemplateId:{request.MaxBackendTemplateId}");

            var bipd = (DropServer.BoosterImplants.BoosterImplantPlayerData)LocalBoosterManager.Instance.GetBoosterImplantPlayerData(request.MaxBackendTemplateId);
            var vipd = (VanityItemPlayerData) LocalVanityItemManager.Instance.GetVanityItemPlayerData();

            var result = new GetInventoryPlayerDataResult();

            Boosters.SetValue(result, bipd);
            VanityItems.SetValue(result, vipd);

            __result = IL2Tasks.Task.FromResult(result);
            return ArchivePatch.SKIP_OG;
        }
    }

    [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.UpdateVanityItemPlayerDataAsync))]
    public static class DropServerClientAPI_UpdateVanityItemPlayerDataAsync_Patch
    {
        private static PropertyInfo Data;
        public static void Init()
        {
            Data = typeof(UpdateVanityItemPlayerDataResult).GetProperty(nameof(UpdateVanityItemPlayerDataResult.Data), AnyBindingFlagss);
        }

        public static bool Prefix(UpdateVanityItemPlayerDataRequest request, ref IL2Tasks.Task<UpdateVanityItemPlayerDataResult> __result)
        {
            ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(UpdateVanityItemPlayerDataResult)}: EntityToken:{request.EntityToken}, MaxBackendTemplateId:{request.BuildRev}");

            var vipd = (VanityItemPlayerData)LocalVanityItemManager.Instance.ProcessTransaction(request.Transaction);

            var result = new UpdateVanityItemPlayerDataResult();

            Data.SetValue(result, vipd);

            __result = IL2Tasks.Task.FromResult(result);
            return ArchivePatch.SKIP_OG;
        }
    }
#endif
}