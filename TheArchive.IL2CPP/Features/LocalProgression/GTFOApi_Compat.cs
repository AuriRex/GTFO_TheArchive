using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Managers;
using TheArchive.Loader;
using TheArchive.Utilities;

namespace TheArchive.Features.LocalProgression
{
    [EnableFeatureByDefault, HideInModSettings, DoNotSaveToConfig]
    internal class GTFOApi_Compat : Feature
    {
        public override string Name => nameof(GTFOApi_Compat);

        public override bool PreInit()
        {
#if !BepInEx
            RequestDisable("Not needed.");
            return false;
#else

            if (!LoaderWrapper.IsModInstalled("dev.gtfomodding.gtfo-api"))
            {
                RequestDisable("GTFO-Api not installed!");
                return false;
            }

            // Force on Local Progression for modded games.
            LocalProgressionController.ForceEnable = true;

            return true;
#endif
        }

        //GTFO.API.Patches.GameDataInit_Patches
        //public static void Initialize_Postfix()
        [ArchivePatch("Initialize_Postfix")]
        public static class GTFOApi_Initialize_Postfix_Patch
        {
            public static Type Type()
            {
                return ImplementationManager.FindTypeInCurrentAppDomain("GTFO.API.Patches.GameDataInit_Patches");
            }

            private static Type _GameDataApi;
            private static MethodInfo _invokeGameDataInit;

            private static Type _APIStatus;
            private static PropertyInfo _networkApiStatus;
            private static PropertyInfo _NetworkApiCreated;

            private static Type _NetworkAPI_Impl;
            private static MethodInfo _CreateApi;

            public static void Init()
            {
                if (!LoaderWrapper.IsModInstalled("dev.gtfomodding.gtfo-api"))
                    return;

                //GTFO.API.GameDataAPI
                //internal static void InvokeGameDataInit()
                _GameDataApi = ImplementationManager.FindTypeInCurrentAppDomain("GTFO.API.GameDataAPI");
                _invokeGameDataInit = _GameDataApi.GetMethod("InvokeGameDataInit", Utils.AnyBindingFlagss);

                //GTFO.API.Resources.APIStatus
                //public static ApiStatusInfo Network { get; internal set; } = new();
                _APIStatus = ImplementationManager.FindTypeInCurrentAppDomain("GTFO.API.Resources.APIStatus");
                _networkApiStatus = _APIStatus.GetProperty("Network");
                _NetworkApiCreated = _networkApiStatus.GetValue(null, null).GetType().GetProperty("Created");

                _NetworkAPI_Impl = ImplementationManager.FindTypeInCurrentAppDomain("GTFO.API.Impl.NetworkAPI_Impl");
                _CreateApi = _APIStatus.GetMethod("CreateApi", Utils.AnyBindingFlagss);
            }

            private static bool NetworkApiCreated()
            {
                var apiInfo = _networkApiStatus.GetValue(null, null);

                return (bool) _NetworkApiCreated.GetValue(apiInfo, null);
            }

            public static bool Prefix()
            {
                GameData.GameSetupDataBlock setupDB = GameData.GameDataBlockBase<GameData.GameSetupDataBlock>.GetBlock(1);

                GameData.RundownDataBlock.RemoveBlockByID(1);

                var rundownDB = GameData.RundownDataBlock.GetBlock(setupDB.RundownIdToLoad);

                rundownDB.persistentID = 1;

                GameData.RundownDataBlock.RemoveBlockByID(setupDB.RundownIdToLoad);
                GameData.RundownDataBlock.AddBlock(rundownDB, -1);

                setupDB.RundownIdToLoad = 1;

                _invokeGameDataInit.Invoke(null, null);

                //if (APIStatus.Network.Created) return;
                //APIStatus.CreateApi<NetworkAPI_Impl>(nameof(APIStatus.Network));

                if (!NetworkApiCreated())
                {
                    var createApi = _CreateApi.MakeGenericMethod(_NetworkAPI_Impl);
                    createApi.Invoke(null, new object[] { "Network" });
                }

                return ArchivePatch.SKIP_OG;
            }
        }
    }
}
