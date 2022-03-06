using MelonLoader;
using SNetwork;
using System;
using System.IO;
using TheArchive.Core;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;
using Il2ColGen = Il2CppSystem.Collections.Generic;
using IL2Tasks = Il2CppSystem.Threading.Tasks;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableLocalProgressionPatches), "LocalProgression")]
    public class PlayFabPatches
    {
        public static bool IsLoggedIn { get; private set; } = false;

        public static Il2ColGen.Dictionary<string, string> playerEntityData = new Il2ColGen.Dictionary<string, string>();

        /*[ArchivePatch(typeof(PlayFabManager), "TryGetPlayerEntityFileValue")]*/
        /*[ArchivePatch(typeof(PlayFabManager), "SetPlayerEntityFileValue")]*/
        /*[ArchivePatch(typeof(PlayFabManager), "DoUploadPlayerEntityFile")]*/
        // has been inlined
        /*[ArchivePatch(typeof(PlayFabManager), nameof(PlayFabManager.LoginWithSteam))]*/
        // Will never be called because it's private and it's caller is patched
        /*[ArchivePatch(typeof(PlayFabManager), "RefreshGlobalTitleData")]*/

        [ArchivePatch(typeof(PlayFabManager), "TryGetRundownTimerData")]
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

        public static void ReadAllFilesFromDisk()
        {
            playerEntityData.Clear();
            foreach (string file in Directory.EnumerateFiles(LocalFiles.FilesDirectoryPath, "*"))
            {
                ArchiveLogger.Msg(ConsoleColor.Green, $"Reading playerEntityData file: {file}");
                string contents = File.ReadAllText(file);
                string fileName = Path.GetFileName(file);

                playerEntityData.Add(fileName, contents);
            }
        }

        [ArchivePatch(typeof(PlayFabManager), nameof(PlayFabManager.Setup))]
        internal class PlayFabManager_SetupPatch
        {
            public static void Prefix(PlayFabManager __instance)
            {
                ArchiveLogger.Msg(ConsoleColor.Yellow, "Setting up PlayFabManager ... ");

                ArchiveLogger.Msg(ConsoleColor.DarkRed, $"SNet.Core Type = {SNet.Core.GetType()}");
            }
        }

        public static StartupScreenData StartupScreenData { get; private set; } = null;

        [ArchivePatch(typeof(PlayFabManager), "TryGetStartupScreenData")]
        internal class PlayFabManager_TryGetStartupScreenDataPatch
        {
            public static bool Prefix(eStartupScreenKey key, out StartupScreenData data, ref bool __result)
            {
                if (StartupScreenData == null)
                {
                    StartupScreenData = new StartupScreenData();
                    StartupScreenData.AllowedToStartGame = true;

                    StartupScreenData.IntroText = Utils.GetStartupTextForRundown((int) ArchiveMod.CurrentRundownInt);
                    StartupScreenData.ShowDiscordButton = true;
                    StartupScreenData.ShowBugReportButton = false;
                    StartupScreenData.ShowRoadmapButton = true;
                    StartupScreenData.ShowIntroText = true;
                }

                __result = true;
                data = StartupScreenData;
                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), nameof(PlayFabManager.OnGetAuthSessionTicketResponse))]
        internal class LoginWithSteamPatch_WhyTheFuckAreYouNotWorking
        {
            public static bool Prefix(/*PlayFabManager __instance*/)
            {
                ArchiveLogger.Msg(ConsoleColor.Yellow, "OnGetAuthSessionTicketResponse() was called!");

                ArchiveLogger.Msg(ConsoleColor.Yellow, "Reading Files ... ");
                ReadAllFilesFromDisk();

                ArchiveLogger.Msg(ConsoleColor.Yellow, "Trying to fake a PlayFab login ...");

                PlayFabManager.Current.m_globalTitleDataLoaded = true;
                PlayFabManager.Current.m_playerDataLoaded = true;
                PlayFabManager.Current.m_entityId = "steamplayer_" + new System.Random().Next(int.MinValue, int.MaxValue);
                PlayFabManager.Current.m_entityType = "Player";
                PlayFabManager.Current.m_entityToken = "bogus_token_ " + new System.Random().Next(int.MinValue, int.MaxValue);
                PlayFabManager.Current.m_entityLoggedIn = true;


                PlayFabManager.PlayFabId = "pId_gczasftzasftqasgsahgjachjhcajh";

                PlayFabManager.LoggedInDateTime = new Il2CppSystem.DateTime();
                PlayFabManager.LoggedInSeconds = Clock.Time;

                IsLoggedIn = true;

                ArchiveLogger.Msg("Starting one second timer.");
                MelonCoroutines.Start(Il2CppUtils.DoAfter(1f, () => {
                    ArchiveLogger.Msg("One second has elapsed. - calling events!");
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
                }));

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "Awake")]
        internal class AwakePatch
        {
            public static void Prefix(PlayFabManager __instance)
            {
                ArchiveLogger.Msg("PlayFabManager has awoken!");
            }
        }

        //GetEntityTokenAsync
        [ArchivePatch(typeof(PlayFabManager), nameof(PlayFabManager.GetEntityTokenAsync))]
        internal class PlayFabManager_GetEntityTokenAsyncPatch
        {
            public static bool Prefix(ref IL2Tasks.Task<string> __result)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"Something's calling GetEntityTokenAsync.");

                __result = IL2Tasks.Task.FromResult<string>("bogus_token_hfdztfc6873e2witgf78rw_42069_768dftw3768ft76fte78fet76ft67");

                return false;
            }
        }

        //RefreshGlobalTitleDataForKeys
        [ArchivePatch(typeof(PlayFabManager), nameof(PlayFabManager.RefreshGlobalTitleDataForKeys))]
        internal class PlayFabManager_RefreshGlobalTitleDataForKeysPatch
        {
            public static bool Prefix(Il2ColGen.List<string> keys, Il2CppSystem.Action OnSuccess)
            {
                if (keys != null)
                {
                    foreach (string key in keys)
                    {
                        ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"RefreshGlobalTitleDataForKeys -> Key:{key}");
                    }
                }

                OnSuccess?.Invoke();

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "AddToOrUpdateLocalPlayerTitleData", new Type[] { typeof(string), typeof(string), typeof(Il2CppSystem.Action) })]
        internal class PlayFabManager_AddToOrUpdateLocalPlayerTitleDataPatch
        {
            public static bool Prefix(string key, string value, Il2CppSystem.Action OnSuccess)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"Canceled AddToOrUpdateLocalPlayerTitleData: Key:{key} - Value:{value}");

                OnSuccess?.Invoke();

                return false;
            }
        }

        //AddToOrUpdateLocalPlayerTitleData(Dictionary<string, string> keys, Action OnSuccess)
        [ArchivePatch(typeof(PlayFabManager), "AddToOrUpdateLocalPlayerTitleData", new Type[] { typeof(Il2ColGen.Dictionary<string, string>), typeof(Il2CppSystem.Action) })]
        internal class PlayFabManager_AddToOrUpdateLocalPlayerTitleDataOverloadPatch
        {
            public static bool Prefix(Il2ColGen.Dictionary<string, string> keys, Il2CppSystem.Action OnSuccess)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"Canceled AddToOrUpdateLocalPlayerTitleData(OverloadMethod): Count:{keys?.Count}");

                if (keys != null)
                {
                    foreach (Il2ColGen.KeyValuePair<string, string> kvp in keys)
                    {
                        ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"AddToOrUpdateLocalPlayerTitleData(OverloadMethod): Key:{kvp?.Key} - Value:{kvp?.Value}");
                    }
                }

                OnSuccess?.Invoke();

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "CloudGiveAlwaysInInventory")]
        internal class PlayFabManager_CloudGiveAlwaysInInventoryPatch
        {
            public static bool Prefix(Il2CppSystem.Action onSucess)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled CloudGiveAlwaysInInventory");

                onSucess?.Invoke();

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "CloudGiveItemToLocalPlayer")]
        internal class PlayFabManager_CloudGiveItemToLocalPlayerPatch
        {
            public static bool Prefix(string ItemId, Il2CppSystem.Action onSucess)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"Canceled CloudGiveItemToLocalPlayer - ItemId:{ItemId}");

                onSucess?.Invoke();

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

        [ArchivePatch(typeof(PlayFabManager), "RefreshItemCatalog")]
        internal class PlayFabManager_RefreshItemCatalogPatch
        {
            public static bool Prefix(PlayFabManager.delUpdateItemCatalogDone OnSuccess, string catalogVersion)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"Canceled RefreshItemCatalog - catalogVersion:{catalogVersion}");

                OnSuccess?.Invoke(default);

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshLocalPlayerInventory")]
        internal class RefreshLocalPlayerInventoryPatch
        {
            public static bool Prefix(PlayFabManager __instance, PlayFabManager.delUpdatePlayerInventoryDone OnSuccess)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled RefreshLocalPlayerInventory");

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshLocalPlayerTitleData")]
        internal class RefreshLocalPlayerTitleDataPatch
        {
            public static bool Prefix(Il2CppSystem.Action OnSuccess, PlayFabManager __instance/*, ref Dictionary<string, string> ___m_localPlayerData*/)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled RefreshLocalPlayerTitleData");

                OnSuccess?.Invoke();

                return false;
            }

        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshGlobalTitleData")]
        internal class PlayFabManager_RefreshStartupScreenTitelDataPatch
        {
            public static bool Prefix(Il2CppSystem.Action OnSuccess)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled RefreshGlobalTitleData");

                OnSuccess?.Invoke();

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshStoreItems")]
        internal class PlayFabManager_RefreshStoreItemsPatch
        {
            public static bool Prefix(string storeID, PlayFabManager.delUpdateStoreItemsDone OnSuccess)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"Canceled RefreshStoreItems - storeID:{storeID}");

                OnSuccess?.Invoke(default);

                return false;
            }
        }

    }
}
