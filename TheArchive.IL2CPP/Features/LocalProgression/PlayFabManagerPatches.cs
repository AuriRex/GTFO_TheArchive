using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using TheArchive.Interfaces;
#if MONO
using IL2Tasks = System.Threading.Tasks;
using IL2System = System;
using Il2ColGen = System.Collections.Generic;
#else
using IL2Tasks = Il2CppSystem.Threading.Tasks;
using IL2System = Il2CppSystem;
using Il2ColGen = Il2CppSystem.Collections.Generic;
#endif

namespace TheArchive.Features.LocalProgression
{
    [HideInModSettings]
    [DoNotSaveToConfig]
    [AutomatedFeature]
    internal class PlayFabManagerPatches : Feature
    {
        public override string Name => nameof(PlayFabManagerPatches);

        public override string Group => FeatureGroups.LocalProgression;

        public static new IArchiveLogger FeatureLogger { get; set; }

        [ArchivePatch(typeof(PlayFabManager), "TryGetRundownTimerData")]
        internal static class PlayFabManager_TryGetRundownTimerData_Patch
        {
            public static bool Prefix(ref bool __result, out RundownTimerData data)
            {
                data = new RundownTimerData();
                data.ShowScrambledTimer = true;
                data.ShowCountdownTimer = true;
                DateTime theDate = DateTime.Today.AddDays(20);
                data.UTC_Target_Day = theDate.Day;
                data.UTC_Target_Hour = theDate.Hour;
                data.UTC_Target_Minute = theDate.Minute;
                data.UTC_Target_Month = theDate.Month;
                data.UTC_Target_Year = theDate.Year;

                __result = true;
                return ArchivePatch.SKIP_OG;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "TryGetStartupScreenData")]
        internal class PlayFabManager_TryGetStartupScreenData_Patch
        {
            public static bool Prefix(eStartupScreenKey key, out StartupScreenData data, ref bool __result)
            {
                var startupScreenData = new StartupScreenData();
                startupScreenData.AllowedToStartGame = true;

                startupScreenData.IntroText = Utils.GetStartupTextForRundown(ArchiveMod.CurrentRundown);
                startupScreenData.ShowDiscordButton = false;
                startupScreenData.ShowBugReportButton = false;
                startupScreenData.ShowRoadmapButton = false;
                startupScreenData.ShowIntroText = true;

                __result = true;
                data = startupScreenData;
                return ArchivePatch.SKIP_OG;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), nameof(PlayFabManager.OnGetAuthSessionTicketResponse))]
        internal class PlayFabManager_OnGetAuthSessionTicketResponse_Patch
        {
            public static bool Prefix()
            {
                FeatureLogger.Notice("Tricking the game into thinking we're logged in ...");

                PlayFabManager.Current.m_globalTitleDataLoaded = true;
                PlayFabManager.Current.m_playerDataLoaded = true;
                PlayFabManager.Current.m_entityId = "steamplayer_" + new System.Random().Next(int.MinValue, int.MaxValue);
                PlayFabManager.Current.m_entityType = "Player";
                PlayFabManager.Current.m_entityToken = "bogus_token_" + new System.Random().Next(int.MinValue, int.MaxValue);
                PlayFabManager.Current.m_entityLoggedIn = true;


                PlayFabManager.PlayFabId = "pId_gczasftzasftqasgsahgjachjhcajh";

                PlayFabManager.LoggedInDateTime = new IL2System.DateTime();
                PlayFabManager.LoggedInSeconds = Clock.Time;

                try
                {
                    Il2CppUtils.CallEvent<PlayFabManager>("OnLoginSuccess");
                    Il2CppUtils.CallEvent<PlayFabManager>("OnTitleDataUpdated");
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Error(ex.Message);
                    ArchiveLogger.Error(ex.StackTrace);
                }

                return ArchivePatch.SKIP_OG;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), nameof(PlayFabManager.GetEntityTokenAsync))]
        internal class PlayFabManager_GetEntityTokenAsync_Patch
        {
            public static bool Prefix(ref IL2Tasks.Task<string> __result)
            {
                __result = IL2Tasks.Task.FromResult<string>(PlayFabManager.Current.m_entityToken);

                return ArchivePatch.SKIP_OG;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), nameof(PlayFabManager.RefreshGlobalTitleDataForKeys))]
        internal class PlayFabManager__RefreshGlobalTitleDataForKeys_Patch
        {
            public static bool Prefix(Il2ColGen.List<string> keys, IL2System.Action OnSuccess)
            {
                if (keys != null)
                {
                    foreach (string key in keys)
                    {
                        FeatureLogger.Msg(ConsoleColor.DarkYellow, $"RefreshGlobalTitleDataForKeys -> Key:{key}");
                    }
                }

                OnSuccess?.Invoke();

                return ArchivePatch.SKIP_OG;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "AddToOrUpdateLocalPlayerTitleData", new Type[] { typeof(string), typeof(string), typeof(IL2System.Action) })]
        internal class PlayFabManager_AddToOrUpdateLocalPlayerTitleData_Patch
        {
            public static bool Prefix(string key, string value, IL2System.Action OnSuccess)
            {
                FeatureLogger.Msg(ConsoleColor.DarkYellow, $"Canceled AddToOrUpdateLocalPlayerTitleData: Key:{key} - Value:{value}");

                OnSuccess?.Invoke();

                return ArchivePatch.SKIP_OG;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "AddToOrUpdateLocalPlayerTitleData", new Type[] { typeof(Il2ColGen.Dictionary<string, string>), typeof(IL2System.Action) })]
        internal class PlayFabManager_AddToOrUpdateLocalPlayerTitleDataOverload_Patch
        {
            public static bool Prefix(Il2ColGen.Dictionary<string, string> keys, IL2System.Action OnSuccess)
            {
                FeatureLogger.Msg(ConsoleColor.DarkYellow, $"Canceled AddToOrUpdateLocalPlayerTitleData(OverloadMethod): Count:{keys?.Count}");

                if (keys != null)
                {
                    foreach (Il2ColGen.KeyValuePair<string, string> kvp in keys)
                    {
                        FeatureLogger.Msg(ConsoleColor.DarkYellow, $"AddToOrUpdateLocalPlayerTitleData(OverloadMethod): Key:{kvp?.Key} - Value:{kvp?.Value}");
                    }
                }

                OnSuccess?.Invoke();

                return ArchivePatch.SKIP_OG;
            }
        }

#region NotAsImportantPatches
        [ArchivePatch(typeof(PlayFabManager), "CloudGiveAlwaysInInventory")]
        internal class PlayFabManager__CloudGiveAlwaysInInventory_Patch
        {
            public static bool Prefix(IL2System.Action onSucess) => SkipOGAndInvoke(onSucess);
        }

        [ArchivePatch(typeof(PlayFabManager), "CloudGiveItemToLocalPlayer")]
        internal class PlayFabManager_CloudGiveItemToLocalPlayer_Patch
        {
            public static bool Prefix(string ItemId, IL2System.Action onSucess) => SkipOGAndInvoke(onSucess);
        }

        [ArchivePatch(typeof(PlayFabManager), "JSONTest")]
        internal class PlayFabManager_JSONTest_Patch
        {
            public static bool Prefix() => ArchivePatch.SKIP_OG;
        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshItemCatalog")]
        internal class PlayFabManager_RefreshItemCatalog_Patch
        {
            public static bool Prefix(PlayFabManager.delUpdateItemCatalogDone OnSuccess, string catalogVersion) => ArchivePatch.SKIP_OG;
        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshLocalPlayerInventory")]
        internal class PlayFabManager_RefreshLocalPlayerInventory_Patch
        {
            public static bool Prefix(PlayFabManager.delUpdatePlayerInventoryDone OnSuccess) => ArchivePatch.SKIP_OG;
        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshLocalPlayerTitleData")]
        internal class PlayFabManager_RefreshLocalPlayerTitleData_Patch
        {
            public static bool Prefix(IL2System.Action OnSuccess) => SkipOGAndInvoke(OnSuccess);
        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshGlobalTitleData")]
        internal class PlayFabManager_RefreshStartupScreenTitelData_Patch
        {
            public static bool Prefix(IL2System.Action OnSuccess) => SkipOGAndInvoke(OnSuccess);
        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshStoreItems")]
        internal class PlayFabManager_RefreshStoreItems_Patch
        {
            public static bool Prefix(string storeID, PlayFabManager.delUpdateStoreItemsDone OnSuccess)
            {
                OnSuccess?.Invoke(default);
                return ArchivePatch.SKIP_OG;
            }
        }
#endregion NotAsImportantPatches

        internal static bool SkipOGAndInvoke(IL2System.Action action)
        {
            action?.Invoke();
            return ArchivePatch.SKIP_OG;
        }
    }
}
