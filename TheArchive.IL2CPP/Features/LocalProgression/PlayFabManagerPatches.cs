using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using TheArchive.Interfaces;
using System.IO;
using System.Collections.Generic;
#if MONO
using IL2Tasks = System.Threading.Tasks;
using IL2System = System;
using Il2ColGen = System.Collections.Generic;
#else
using IL2Tasks = Il2CppSystem.Threading.Tasks;
using IL2System = Il2CppSystem;
using Il2ColGen = Il2CppSystem.Collections.Generic;
#endif
using static TheArchive.Utilities.Utils;

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

#if MONO
        public override void OnEnable()
        {
            ReadAllFilesFromDisk();
        }

        private static Dictionary<string, string> _playerEntityData = new Dictionary<string, string>();

        public static void ReadAllFilesFromDisk()
        {
            _playerEntityData.Clear();
            foreach (string file in Directory.EnumerateFiles(LocalFiles.FilesDirectoryPath, "*"))
            {
                ArchiveLogger.Msg(ConsoleColor.Green, $"Reading playerEntityData file: {file}");
                string contents = File.ReadAllText(file);
                string fileName = Path.GetFileName(file);

                _playerEntityData.Add(fileName, contents);
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "TryGetPlayerEntityFileValue")]
        internal static class PlayFabManager_TryGetPlayerEntityFileValuePatch
        {
            public static bool Prefix(string fileName, out string value, ref bool __result)
            {
                __result = _playerEntityData.TryGetValue(fileName, out value);

                FeatureLogger.Msg(ConsoleColor.Green, $"Getting {fileName} from playerEntityData.");

                return ArchivePatch.SKIP_OG;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "SetPlayerEntityFileValue")]
        internal static class PlayFabManager_SetPlayerEntityFileValuePatch
        {
            public static bool Prefix(string fileName, string value)
            {
                FeatureLogger.Msg(ConsoleColor.Green, $"Adding {fileName} to playerEntityData.");
                if (_playerEntityData.ContainsKey(fileName))
                {
                    _playerEntityData[fileName] = value;
                    return ArchivePatch.SKIP_OG;
                }

                _playerEntityData.Add(fileName, value);
                return ArchivePatch.SKIP_OG;
            }
        }


        [ArchivePatch(typeof(PlayFabManager), "DoUploadPlayerEntityFile")]
        internal class PlayFabManager_DoUploadPlayerEntityFilePatch
        {
            // This is where the game usually uploads your Progression to the PlayFab servers.
            public static bool Prefix(string fileName)
            {
                if (PlayFabManager.TryGetPlayerEntityFileValue(fileName, out string value))
                    LocalFiles.SaveToFilesDir(fileName, value);

                MonoUtils.CallEvent<PlayFabManager>(nameof(PlayFabManager.OnFileUploadSuccess), null, fileName);

                return ArchivePatch.SKIP_OG;
            }
        }

        [RundownConstraint(RundownFlags.RundownTwo, RundownFlags.RundownFour)]
        [ArchivePatch(typeof(PlayfabMatchmakingManager), nameof(PlayfabMatchmakingManager.CancelAllTicketsForLocalPlayer))]
        internal class PlayfabMatchmakingManager_CancelAllTicketsForLocalPlayer_Patch
        {
            public static bool Prefix() => ArchivePatch.SKIP_OG;
        }
#endif

        [RundownConstraint(RundownFlags.RundownTwo, RundownFlags.Latest)]
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

        [ArchivePatch(typeof(PlayFabManager), "OnGetAuthSessionTicketResponse")]
        internal class PlayFabManager_OnGetAuthSessionTicketResponse_Patch
        {
            public static string PLAYFAB_ID = "idk_lol";

            private static string _entityId = null;
            public static string EntityID => _entityId ??= "Player_" + new System.Random().Next(int.MinValue, int.MaxValue);

            private static string _entityToken = null;
            public static string EntityToken => _entityToken ??= "EntityToken_" + new System.Random().Next(int.MinValue, int.MaxValue);

#if IL2CPP
            public static bool Prefix()
            {
                FeatureLogger.Notice("Tricking the game into thinking we're logged in ...");

                PlayFabManager.Current.m_globalTitleDataLoaded = true;
                PlayFabManager.Current.m_playerDataLoaded = true;
                PlayFabManager.Current.m_entityId = EntityID;
                PlayFabManager.Current.m_entityType = "Player";
                PlayFabManager.Current.m_entityToken = EntityToken;
                PlayFabManager.Current.m_entityLoggedIn = true;


                PlayFabManager.PlayFabId = PLAYFAB_ID;

                PlayFabManager.LoggedInDateTime = new IL2System.DateTime();
                PlayFabManager.LoggedInSeconds = Clock.Time;

                Il2CppUtils.CallEvent<PlayFabManager>("OnLoginSuccess");
                Il2CppUtils.CallEvent<PlayFabManager>("OnTitleDataUpdated");

                return ArchivePatch.SKIP_OG;
            }
#else
            [IsPrefix]
            public static bool PrefixMono(ref bool ___m_globalTitleDataLoaded, ref bool ___m_playerDataLoaded, ref bool ___m_loggedIn, ref string ___m_entityId, ref string ___m_entityType)
            {
                FeatureLogger.Notice("Tricking the game into thinking we're logged in ...");

                ___m_globalTitleDataLoaded = true;
                ___m_playerDataLoaded = true;
                ___m_loggedIn = true;
                ___m_entityId = EntityID;
                ___m_entityType = "Player";

                PlayFabManager.PlayFabId = PLAYFAB_ID;
                PlayFabManager.PlayerEntityFilesLoaded = true;

                PlayFabManager.LoggedInDateTime = new IL2System.DateTime();
                PlayFabManager.LoggedInSeconds = Clock.Time;

                MonoUtils.CallEvent<PlayFabManager>(nameof(PlayFabManager.OnAllPlayerEntityFilesLoaded));
                MonoUtils.CallEvent<PlayFabManager>(nameof(PlayFabManager.OnLoginSuccess));

                return ArchivePatch.SKIP_OG;
            }
#endif
        }

        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(typeof(PlayFabManager), "GetEntityTokenAsync")]
        internal class PlayFabManager_GetEntityTokenAsync_Patch
        {
            public static bool Prefix(ref IL2Tasks.Task<string> __result)
            {
                __result = IL2Tasks.Task.FromResult<string>(PlayFabManager_OnGetAuthSessionTicketResponse_Patch.EntityToken);

                return ArchivePatch.SKIP_OG;
            }
        }

        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(typeof(PlayFabManager), "RefreshGlobalTitleDataForKeys")]
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
                        FeatureLogger.Msg(ConsoleColor.DarkYellow, $"AddToOrUpdateLocalPlayerTitleData(OverloadMethod): Key:{kvp.Key} - Value:{kvp.Value}");
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
        internal class PlayFabManager_RefreshGlobalTitleData_Patch
        {
            public static bool Prefix(IL2System.Action OnSuccess) => SkipOGAndInvoke(OnSuccess);
        }

        [RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownThree)]
        [ArchivePatch(typeof(PlayFabManager), "RefreshStartupScreenTitelData")]
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
