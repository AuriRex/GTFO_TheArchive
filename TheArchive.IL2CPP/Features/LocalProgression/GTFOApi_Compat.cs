using System;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Managers;
using TheArchive.Loader;
using TheArchive.Utilities;

namespace TheArchive.Features.LocalProgression
{
    [EnableFeatureByDefault, HideInModSettings, DoNotSaveToConfig]
    [RundownConstraint(Utils.RundownFlags.Latest)]
    internal class GTFOApi_Compat : Feature
    {
        public override string Name => nameof(GTFOApi_Compat);

        public override bool ShouldInit()
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
                var setupDB = GameData.GameSetupDataBlock.GetBlock(1);

                GameData.RundownDataBlock.RemoveBlockByID(1);

                uint newId = 1;
                for(int i = 0; i < setupDB.RundownIdsToLoad.Count; i++)
                {
                    var originalIdToLoad = setupDB.RundownIdsToLoad[i];

                    if(originalIdToLoad != 1)
                    {
                        GameData.RundownDataBlock.RemoveBlockByID(newId);
                        var rundownDB = GameData.RundownDataBlock.GetBlock(originalIdToLoad);

                        if(rundownDB != null)
                        {
                            rundownDB.persistentID = newId;
                            setupDB.RundownIdsToLoad[i] = newId;

                            GameData.RundownDataBlock.RemoveBlockByID(originalIdToLoad);
                            GameData.RundownDataBlock.AddBlock(rundownDB, -1);

                            newId++;
                        }
                    }
                }

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
