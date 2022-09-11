using DropServer;
using DropServer.VanityItems;
using System;
using TheArchive.Core;
using TheArchive.Managers;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;
using static TheArchive.Utilities.Utils;
using IL2Tasks = Il2CppSystem.Threading.Tasks;

namespace TheArchive.IL2CPP.R6.ArchivePatches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableLocalProgressionPatches), "LocalProgression")]
    public class DropServerClientAPIViaPlayFabPatches
    {
        // +public Task<GetInventoryPlayerDataResult> GetInventoryPlayerDataAsync(GetInventoryPlayerDataRequest request)
        // +public Task<UpdateVanityItemPlayerDataResult> UpdateVanityItemPlayerDataAsync(UpdateVanityItemPlayerDataRequest request)
        // +public Task<DebugVanityItemResult> DebugVanityItemAsync(DebugVanityItemRequest request)

        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.GetInventoryPlayerDataAsync), RundownFlags.RundownSix, RundownFlags.Latest)]
        public static class DropServerClientAPI_GetInventoryPlayerDataAsyncPatch
        {
            public static bool Prefix(GetInventoryPlayerDataRequest request, ref IL2Tasks.Task<GetInventoryPlayerDataResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(GetInventoryPlayerDataRequest)}: EntityToken:{request.EntityToken}, MaxBackendTemplateId:{request.MaxBackendTemplateId}");

                var bipd = (DropServer.BoosterImplants.BoosterImplantPlayerData) LocalBoosterManager.Instance.GetBoosterImplantPlayerData(request.MaxBackendTemplateId);
                var vipd = (VanityItemPlayerData) LocalVanityItemManager.Instance.GetVanityItemPlayerData();

                var result = new GetInventoryPlayerDataResult();

                Il2CppUtils.SetFieldUnsafe(result, bipd, nameof(GetInventoryPlayerDataResult.Boosters));
                Il2CppUtils.SetFieldUnsafe(result, vipd, nameof(GetInventoryPlayerDataResult.VanityItems));

                __result = IL2Tasks.Task.FromResult(result);
                return false;
            }
        }

        // +public Task<UpdateVanityItemPlayerDataResult> UpdateVanityItemPlayerDataAsync(UpdateVanityItemPlayerDataRequest request)
        [ArchivePatch(typeof(DropServerClientAPIViaPlayFab), nameof(DropServerClientAPIViaPlayFab.UpdateVanityItemPlayerDataAsync), RundownFlags.RundownSix, RundownFlags.Latest)]
        public static class DropServerClientAPI_UpdateVanityItemPlayerDataAsyncPatch
        {
            public static bool Prefix(UpdateVanityItemPlayerDataRequest request, ref IL2Tasks.Task<UpdateVanityItemPlayerDataResult> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkBlue, $"{nameof(DropServerClientAPIViaPlayFab)} -> requested {nameof(GetInventoryPlayerDataRequest)}: EntityToken:{request.EntityToken}, MaxBackendTemplateId:{request.BuildRev}");

                var vipd = (VanityItemPlayerData) LocalVanityItemManager.Instance.ProcessTransaction(request.Transaction);

                var result = new UpdateVanityItemPlayerDataResult();

                Il2CppUtils.SetFieldUnsafe(result, vipd, nameof(UpdateVanityItemPlayerDataResult.Data));

                __result = IL2Tasks.Task.FromResult(result);
                return false;
            }
        }
    }
}
