using Globals;
using PlayFab.ClientModels;
using SNetwork;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    public class PlayFabPatches
    {

        private static Dictionary<string, string> _playerEntityData = new Dictionary<string, string>();


        [ArchivePatch(typeof(PlayFabManager), "TryGetRundownTimerData", Utils.RundownFlags.RundownTwo, Utils.RundownFlags.RundownThree)]
        internal static class PlayFabManager_TryGetRundownTimerDataPatch
        {
            public static bool Prefix(ref bool __result, out RundownTimerData data)
            {
                data = new RundownTimerData();
                data.ShowScrambledTimer = true;
                data.ShowCountdownTimer = true;
                DateTime theDate = DateTime.Today.AddDays(5);
                data.UTC_Target_Day = theDate.Day;
                data.UTC_Target_Hour = theDate.Hour;
                data.UTC_Target_Minute = theDate.Minute;
                data.UTC_Target_Month = theDate.Month;
                data.UTC_Target_Year = theDate.Year;

                __result = true;
                return false;
            }
        }

        [ArchivePatch(typeof(PlayfabMatchmakingManager), "CancelAllTicketsForLocalPlayer", Utils.RundownFlags.RundownTwo, Utils.RundownFlags.RundownThree)]
        internal static class PlayfabMatchmakingManager_CancelAllTicketsForLocalPlayerPatch
        {
            public static bool Prefix()
            {
                return false;
            }
        }


        [ArchivePatch(typeof(PlayFabManager), "TryGetPlayerEntityFileValue")]
        internal static class PlayFabManager_TryGetPlayerEntityFileValuePatch
        {
            public static bool Prefix(string fileName, out string value, ref bool __result)
            {
                __result = _playerEntityData.TryGetValue(fileName, out value);

                ArchiveLogger.Msg(ConsoleColor.Green, $"Getting {fileName} from playerEntityData.");

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "SetPlayerEntityFileValue")]
        internal static class PlayFabManager_SetPlayerEntityFileValuePatch
        {
            public static bool Prefix(string fileName, string value)
            {
                ArchiveLogger.Msg(ConsoleColor.Green, $"Adding {fileName} to playerEntityData.");
                if (_playerEntityData.ContainsKey(fileName))
                {
                    _playerEntityData[fileName] = value;
                    return false;
                }

                _playerEntityData.Add(fileName, value);
                return false;
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

                MonoUtils.CallEvent<PlayFabManager>("OnFileUploadSuccess", null, fileName);

                return false;
            }
        }

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

        [ArchivePatch(typeof(PlayFabManager), nameof(PlayFabManager.Setup))]
        internal class PlayFabManager_SetupPatch
        {
            public static bool Prefix(PlayFabManager __instance)
            {
                ArchiveLogger.Msg(ConsoleColor.Yellow, "Setting up PlayFabManager ... ");

                ArchiveLogger.Msg(ConsoleColor.DarkRed, $"SNet.Core Type = {SNet.Core.GetType()}");

                ArchiveLogger.Msg(ConsoleColor.Yellow, "Reading Files ... ");
                ReadAllFilesFromDisk();

                return true;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "TryGetStartupScreenData")]
        internal class PlayFabManager_TryGetStartupScreenDataPatch
        {
            public static bool Prefix(eStartupScreenKey key, out StartupScreenData data, ref bool __result)
            {
                __result = true;
                var sud = new StartupScreenData();
                sud.AllowedToStartGame = true;

                sud.IntroText = Utils.GetStartupTextForRundown((int) Global.RundownIdToLoad);
                sud.ShowDiscordButton = true;
                sud.ShowBugReportButton = false;
                sud.ShowRoadmapButton = true;
                //sud.ShowOvertoneButton = false;
                sud.ShowIntroText = true;
                data = sud;
                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "LoginWithSteam")]
        internal class LoginWithSteamPatch
        {
            public static bool Prefix(PlayFabManager __instance, ref bool ___m_globalTitleDataLoaded, ref bool ___m_playerDataLoaded, ref bool ___m_loggedIn, ref string ___m_entityId, ref string ___m_entityType)
            {
                ArchiveLogger.Msg(ConsoleColor.Yellow, "Skipping PlayFab entirely ...");

                ___m_globalTitleDataLoaded = true;
                ___m_playerDataLoaded = true;
                ___m_entityId = "steam_user_" + new System.Random().Next(int.MinValue, int.MaxValue);
                ___m_entityType = "Player";

                PlayFabManager.PlayFabId = "pId_whateveradghf638zd79238zr893zr829rzagtet" + new System.Random().Next(int.MinValue, int.MaxValue);
                PlayFabManager.PlayerEntityFilesLoaded = true;
                PlayFabManager.LoggedInDateTime = new DateTime();
                PlayFabManager.LoggedInSeconds = Clock.Time;
                ___m_loggedIn = true;

                MonoUtils.CallEvent<PlayFabManager>("OnAllPlayerEntityFilesLoaded");
                MonoUtils.CallEvent<PlayFabManager>("OnLoginSuccess");

                /*ArchiveLogger.Info("Starting point one second timer.");
                ArchiveMONOModule.CoroutineHelper.StartCoroutine(MonoUtils.DoAfter(.1f, () => {
                    ArchiveLogger.Info("Calling event OnAllPlayerEntityFilesLoaded() ...");
                    MonoUtils.CallEvent<PlayFabManager>("OnAllPlayerEntityFilesLoaded");
                }));

                ArchiveLogger.Msg("Starting point two second timer.");
                ArchiveMONOModule.CoroutineHelper.StartCoroutine(MonoUtils.DoAfter(.2f, () => {
                    ArchiveLogger.Info("Calling event OnLoginSucess() ...");
                    MonoUtils.CallEvent<PlayFabManager>("OnLoginSuccess");
                }));*/
                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "Awake")]
        internal class AwakePatch
        {
            public static void Prefix(PlayFabManager __instance)
            {
                ArchiveLogger.Msg("PlayFabManager has awoken!");
                PlayFabManager.OnAllPlayerEntityFilesLoaded += __instance_OnAllPlayerEntityFilesLoaded;
            }

            private static void __instance_OnAllPlayerEntityFilesLoaded()
            {
                PlayFabManager pfm = PlayFabManager.Current;

                ArchiveLogger.Msg(ConsoleColor.DarkRed, "__instance_OnAllPlayerEntityFilesLoaded");

                /*Dictionary<string, string> playerEntityFiles = typeof(PlayFabManager).GetField("m_playerEntityFiles", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pfm) as Dictionary<string, string>;

                ArchiveLogger.Error("__instance_OnAllPlayerEntityFilesLoaded:START");
                foreach (KeyValuePair<string, string> kvp in playerEntityFiles)
                {
                    ArchiveLogger.Error($"{kvp.Key} : {kvp.Value}");
                }
                ArchiveLogger.Error("__instance_OnAllPlayerEntityFilesLoaded:END");*/
            }
        }


        [ArchivePatch(typeof(PlayFabManager), "AddToOrUpdateLocalPlayerTitleData", new Type[] { typeof(Dictionary<string, string>), typeof(Action) })]
        internal class PlayFabManager_AddToOrUpdateLocalPlayerTitleDataPatch
        {
            public static bool Prefix(Action OnSuccess)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled AddToOrUpdateLocalPlayerTitleData");

                OnSuccess?.Invoke();

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "CloudGiveAlwaysInInventory")]
        internal class PlayFabManager_CloudGiveAlwaysInInventoryPatch
        {
            public static bool Prefix(Action onSucess)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled CloudGiveAlwaysInInventory");

                onSucess?.Invoke();

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "CloudGiveItemToLocalPlayer")]
        internal class PlayFabManager_CloudGiveItemToLocalPlayerPatch
        {
            public static bool Prefix()
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled CloudGiveItemToLocalPlayer");
                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "JSONTest")]
        internal class PlayFabManager_JSONTestPatch
        {
            public static bool Prefix()
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled JSONTest - why is this being run in the first place?");
                return false;
            }
        }

        // Is never going to be called because it's private and it's caller is patched
        [ArchivePatch(typeof(PlayFabManager), "RefreshItemCatalog")]
        internal class PlayFabManager_RefreshItemCatalogPatch
        {
            public static bool Prefix()
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled RefreshItemCatalog");
                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshLocalPlayerInventory")]
        internal class RefreshLocalPlayerInventoryPatch
        {
            public static bool Prefix(PlayFabManager __instance, PlayFabManager.delUpdatePlayerInventoryDone OnSuccess)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled RefreshLocalPlayerInventory");
                OnSuccess?.Invoke(new List<ItemInstance>());
                return false;
            }
        }


        [ArchivePatch(typeof(PlayFabManager), "RefreshLocalPlayerTitleData")]
        internal class RefreshLocalPlayerTitleDataPatch
        {
            public static bool Prefix(Action OnSuccess = null)
            {
                OnSuccess?.Invoke();

                return false;
            }

            public static void Postfix(PlayFabManager __instance, Dictionary<string, string> ___m_localPlayerData)
            {
                ArchiveLogger.Error("LocalPlayerTitleData:START");
                foreach (KeyValuePair<string, string> kvp in ___m_localPlayerData)
                {
                    ArchiveLogger.Msg($"{kvp.Key} : {kvp.Value}");
                }
                ArchiveLogger.Error("LocalPlayerTitleData:END");

            }
        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshStartupScreenTitelData")]
        internal class PlayFabManager_RefreshStartupScreenTitelDataPatch
        {
            public static bool Prefix(Action OnSuccess)
            {
                OnSuccess?.Invoke();

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshStoreItems")]
        internal class PlayFabManager_RefreshStoreItemsPatch
        {
            public static bool Prefix()
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled RefreshStoreItems");
                return false;
            }
        }

    }
}
