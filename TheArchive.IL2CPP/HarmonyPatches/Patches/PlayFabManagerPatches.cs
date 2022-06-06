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
    public class PlayFabManagerPatches
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
        internal static class TryGetRundownTimerDataPatch
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
        internal class SetupPatch
        {
            public static void Prefix(PlayFabManager __instance)
            {
                ArchiveLogger.Msg(ConsoleColor.Yellow, "Setting up PlayFabManager ... ");

                ArchiveLogger.Msg(ConsoleColor.DarkRed, $"SNet.Core Type = {SNet.Core.GetType()}");
            }
        }

        public static StartupScreenData StartupScreenData { get; private set; } = null;

        [ArchivePatch(typeof(PlayFabManager), "TryGetStartupScreenData")]
        internal class TryGetStartupScreenDataPatch
        {
            public static bool Prefix(eStartupScreenKey key, out StartupScreenData data, ref bool __result)
            {
                if (StartupScreenData == null)
                {
                    StartupScreenData = new StartupScreenData();
                    StartupScreenData.AllowedToStartGame = true;

                    StartupScreenData.IntroText = Utils.GetStartupTextForRundown(ArchiveMod.CurrentRundown);
                    StartupScreenData.ShowDiscordButton = false;
                    StartupScreenData.ShowBugReportButton = false;
                    StartupScreenData.ShowRoadmapButton = false;
                    StartupScreenData.ShowIntroText = true;
                }

                __result = true;
                data = StartupScreenData;
                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), nameof(PlayFabManager.OnGetAuthSessionTicketResponse))]
        internal class OnGetAuthSessionTicketResponsePatch
        {
            public static bool Prefix(/*PlayFabManager __instance*/)
            {
                ArchiveLogger.Msg(ConsoleColor.Yellow, "Reading Playfab files ... ");
                ReadAllFilesFromDisk();

                ArchiveLogger.Msg(ConsoleColor.Yellow, "Skipping PlayFab entirely ...");

                PlayFabManager.Current.m_globalTitleDataLoaded = true;
                PlayFabManager.Current.m_playerDataLoaded = true;
                PlayFabManager.Current.m_entityId = "steamplayer_" + new System.Random().Next(int.MinValue, int.MaxValue);
                PlayFabManager.Current.m_entityType = "Player";
                PlayFabManager.Current.m_entityToken = "bogus_token_" + new System.Random().Next(int.MinValue, int.MaxValue);
                PlayFabManager.Current.m_entityLoggedIn = true;


                PlayFabManager.PlayFabId = "pId_gczasftzasftqasgsahgjachjhcajh";

                PlayFabManager.LoggedInDateTime = new Il2CppSystem.DateTime();
                PlayFabManager.LoggedInSeconds = Clock.Time;

                IsLoggedIn = true;

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
        internal class GetEntityTokenAsyncPatch
        {
            public static bool Prefix(ref IL2Tasks.Task<string> __result)
            {
                __result = IL2Tasks.Task.FromResult<string>(PlayFabManager.Current.m_entityToken);

                return false;
            }
        }

        //RefreshGlobalTitleDataForKeys
        [ArchivePatch(typeof(PlayFabManager), nameof(PlayFabManager.RefreshGlobalTitleDataForKeys))]
        internal class RefreshGlobalTitleDataForKeysPatch
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
        internal class AddToOrUpdateLocalPlayerTitleDataPatch
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
        internal class AddToOrUpdateLocalPlayerTitleDataOverloadPatch
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
        internal class CloudGiveAlwaysInInventoryPatch
        {
            public static bool Prefix(Il2CppSystem.Action onSucess)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled CloudGiveAlwaysInInventory");

                onSucess?.Invoke();

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "CloudGiveItemToLocalPlayer")]
        internal class CloudGiveItemToLocalPlayerPatch
        {
            public static bool Prefix(string ItemId, Il2CppSystem.Action onSucess)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"Canceled CloudGiveItemToLocalPlayer - ItemId:{ItemId}");

                onSucess?.Invoke();

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "JSONTest")]
        internal class JSONTestPatch
        {
            public static bool Prefix()
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled JSONTest - why is this being run in the first place?");
                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshItemCatalog")]
        internal class RefreshItemCatalogPatch
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
        internal class RefreshStartupScreenTitelDataPatch
        {
            public static bool Prefix(Il2CppSystem.Action OnSuccess)
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled RefreshGlobalTitleData");

                OnSuccess?.Invoke();

                return false;
            }
        }

        [ArchivePatch(typeof(PlayFabManager), "RefreshStoreItems")]
        internal class RefreshStoreItemsPatch
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
