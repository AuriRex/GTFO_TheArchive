using DropServer;
using DropServer.VanityItems;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Managers;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;
using IL2Tasks = Il2CppSystem.Threading.Tasks;

namespace TheArchive.Features.LocalProgression
{
    [RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
    [EnableFeatureByDefault]
    public class LocalVanity : Feature
    {
        public override string Name => "Local Vanity";

        public override string Group => FeatureGroups.LocalProgression;

        public override bool RequiresRestart => true;

        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.GetInventoryPlayerDataAsync))]
        public static class DropServerClientAPI_GetInventoryPlayerDataAsyncPatch
        {
            public static bool Prefix(GetInventoryPlayerDataRequest request, ref IL2Tasks.Task<GetInventoryPlayerDataResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(GetInventoryPlayerDataRequest)}: EntityToken:{request.EntityToken}, MaxBackendTemplateId:{request.MaxBackendTemplateId}");

                var bipd = (DropServer.BoosterImplants.BoosterImplantPlayerData)LocalBoosterManager.Instance.GetBoosterImplantPlayerData(request.MaxBackendTemplateId);
                var vipd = (VanityItemPlayerData) LocalVanityItemManager.Instance.GetVanityItemPlayerData();

                var result = new GetInventoryPlayerDataResult();

                Il2CppUtils.SetFieldUnsafe(result, bipd, nameof(GetInventoryPlayerDataResult.Boosters));
                Il2CppUtils.SetFieldUnsafe(result, vipd, nameof(GetInventoryPlayerDataResult.VanityItems));

                __result = IL2Tasks.Task.FromResult(result);
                return ArchivePatch.SKIP_OG;
            }
        }

        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.UpdateVanityItemPlayerDataAsync))]
        public static class DropServerClientAPI_UpdateVanityItemPlayerDataAsyncPatch
        {
            public static bool Prefix(UpdateVanityItemPlayerDataRequest request, ref IL2Tasks.Task<UpdateVanityItemPlayerDataResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(GetInventoryPlayerDataRequest)}: EntityToken:{request.EntityToken}, MaxBackendTemplateId:{request.BuildRev}");

                var vipd = (VanityItemPlayerData)LocalVanityItemManager.Instance.ProcessTransaction(request.Transaction);

                var result = new UpdateVanityItemPlayerDataResult();

                Il2CppUtils.SetFieldUnsafe(result, vipd, nameof(UpdateVanityItemPlayerDataResult.Data));

                __result = IL2Tasks.Task.FromResult(result);
                return ArchivePatch.SKIP_OG;
            }
        }
    }
}
