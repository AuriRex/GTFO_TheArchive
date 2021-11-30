using Globals;
using HarmonyLib;
using PlayFab.ClientModels;
using SNetwork;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TheArchive.Utilities;

namespace TheArchive.HarmonyPatches.AutoPatches
{
    public class AutoPlayFabPatches
    {

        public static bool DisableAllPlayFabInteraction { get; set; } = true;

        public static Dictionary<string, string> playerEntityData = new Dictionary<string, string>();

        [HarmonyPatch(typeof(PlayFabManager), "TryGetPlayerEntityFileValue")]
        internal static class PlayFabManager_TryGetPlayerEntityFileValuePatch
        {
            public static bool Prefix(string fileName, out string value, ref bool __result)
            {

                __result = playerEntityData.TryGetValue(fileName, out value);

                ArchiveLogger.Msg(ConsoleColor.Green, $"Getting {fileName} from playerEntityData.");

                return false;
            }
        }

        [HarmonyPatch(typeof(PlayFabManager), "SetPlayerEntityFileValue")] // SetPlayerEntityFileValue(string fileName, string value)
        internal static class PlayFabManager_SetPlayerEntityFileValuePatch
        {
            public static bool Prefix(string fileName, string value)
            {
                ArchiveLogger.Msg(ConsoleColor.Green, $"Adding {fileName} to playerEntityData.");
                if (playerEntityData.ContainsKey(fileName))
                {
                    playerEntityData[fileName] = value;
                    return false;
                }

                playerEntityData.Add(fileName, value);
                return false;
            }
        }


        [HarmonyPatch(typeof(PlayFabManager), "DoUploadPlayerEntityFile")]
        internal class PlayFabManager_DoUploadPlayerEntityFilePatch
        {
            public static bool Prefix(string fileName)
            {
                if (DisableAllPlayFabInteraction)
                {


                    // This is where the game usually uploads your Progression to the PlayFab servers.
                    // TODO: Save player progression to disk instead!
                    if (PlayFabManager.TryGetPlayerEntityFileValue(fileName, out string value))
                        LocalFiles.SaveToFilesDir(fileName, value);

                    var eventInfo = typeof(PlayFabManager).GetType().GetEvent("OnFileUploadSuccess", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    var eventDelegate = (MulticastDelegate) typeof(PlayFabManager).GetField("OnFileUploadSuccess", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
                    if (eventDelegate != null)
                    {

                        foreach (var handler in eventDelegate.GetInvocationList())
                        {
                            ArchiveLogger.Msg(ConsoleColor.Red, $"OnFileUploadSuccess: calling {handler.Method.DeclaringType.Name}.{handler.Method.Name}()");
                            handler.Method.Invoke(handler.Target, new object[] { fileName });
                        }
                    }
                    else
                    {
                        ArchiveLogger.Msg(ConsoleColor.Red, $"OnFileUploadSuccess is null! (= no event handlers)");
                    }

                    //ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled DoUploadPlayerEntityFile");
                    return false;
                }

                return true;
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


        // Doesn't exist for RD#001
        //[HarmonyPatch(typeof(PlayFabManager), "TryGetRundownTimerData")]

        [HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.Setup))]
        internal class PlayFabManager_SetupPatch
        {

            //public static Callback<GetAuthSessionTicketResponse_t> callback;

            public static bool Prefix(PlayFabManager __instance)
            {
                ArchiveLogger.Msg(ConsoleColor.Yellow, "Setting up PlayFabManager ... ");

                ArchiveLogger.Msg(ConsoleColor.DarkRed, $"SNet.Core Type = {SNet.Core.GetType()}");

                ArchiveLogger.Msg(ConsoleColor.Yellow, "Reading Files ... ");
                ReadAllFilesFromDisk();

                //SNet.Core.IsOnline();

                return true;
            }

            public static void OnGetAuthSessionTicketResponse(GetAuthSessionTicketResponse_t response)
            {
                ArchiveLogger.Msg(ConsoleColor.Red, "GetAuthSessionTicketResponse_t received");
                typeof(PlayFabManager).GetMethod("OnGetAuthSessionTicketResponse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(PlayFabManager.Current, new object[] { response });
            }
        }

        [HarmonyPatch(typeof(PlayFabManager), "TryGetStartupScreenData")]
        internal class PlayFabManager_TryGetStartupScreenDataPatch
        {
            public static bool Prefix(eStartupScreenKey key, out StartupScreenData data, ref bool __result)
            {
                if (DisableAllPlayFabInteraction)
                {
                    //ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled TryGetStartupScreenData");
                    // other stuff maybe ?
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

                __result = false;
                data = null;
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayFabManager), "LoginWithSteam")]
        internal class LoginWithSteamPatch
        {
            public static bool Prefix(PlayFabManager __instance, ref bool ___m_globalTitleDataLoaded, ref bool ___m_playerDataLoaded, ref bool ___m_loggedIn, ref string ___m_entityId, ref string ___m_entityType)
            {
                ArchiveLogger.Error("LoginWithSteam() ...");
                // TODO: fake playfab login maybe?


                if (DisableAllPlayFabInteraction)
                {
                    // other stuff maybe ?
                    ArchiveLogger.Msg(ConsoleColor.Yellow, "Trying to fake a PlayFab login ...");
                    // m_globalTitleDataLoaded && this.m_playerDataLoaded && m_loggedIn
                    // OnAllPlayerEntityFilesLoaded

                    // this.m_entityId
                    // this.m_entityType
                    ___m_globalTitleDataLoaded = true;
                    ___m_playerDataLoaded = true;
                    ___m_entityId = "steam_user_" + new System.Random().Next(int.MinValue, int.MaxValue);
                    ___m_entityType = "Player";

                    PlayFabManager.PlayFabId = "pId_whateveradghf638zd79238zr893zr829rzagtet" + new System.Random().Next(int.MinValue, int.MaxValue);
                    PlayFabManager.PlayerEntityFilesLoaded = true;
                    PlayFabManager.LoggedInDateTime = new DateTime();
                    PlayFabManager.LoggedInSeconds = Clock.Time;
                    ___m_loggedIn = true;

                    ArchiveLogger.Msg("Starting one second timer.");
                    ArchiveModule.CoroutineHelper.StartCoroutine(MonoUtils.DoAfter(1f, () => {
                        ArchiveLogger.Msg("One second has elapsed. - calling event OnAllPlayerEntityFilesLoaded()!");
                        var eventInfo = typeof(PlayFabManager).GetType().GetEvent("OnAllPlayerEntityFilesLoaded", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        var eventDelegate2 = (MulticastDelegate) typeof(PlayFabManager).GetField("OnAllPlayerEntityFilesLoaded", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
                        if (eventDelegate2 != null)
                        {
                            //ArchiveLogger.Msg(ConsoleColor.Red, $"OnAllPlayerEntityFilesLoaded is NOT null!");
                            foreach (var handler in eventDelegate2.GetInvocationList())
                            {
                                ArchiveLogger.Msg(ConsoleColor.Red, $"OnAllPlayerEntityFilesLoaded: calling {handler.Method.DeclaringType.Name}.{handler.Method.Name}()");
                                handler.Method.Invoke(handler.Target, null);
                            }
                        }
                        else
                        {
                            ArchiveLogger.Error("OnAllPlayerEntityFilesLoaded is null!");
                        }
                    }));

                    ArchiveLogger.Msg("Starting two second timer.");
                    ArchiveModule.CoroutineHelper.StartCoroutine(MonoUtils.DoAfter(2f, () => {
                        ArchiveLogger.Msg("Two seconds have elapsed. - calling event OnLoginSucess()!");
                        //typeof(PlayFabManager).GetEvent("OnAllPlayerEntityFilesLoaded", BindingFlags.Public | BindingFlags.Static);
                        //typeof(PlayFabManager).GetEvent("OnLoginSuccess").RaiseMethod.Invoke(null, null);
                        var eventInfo = typeof(PlayFabManager).GetType().GetEvent("OnLoginSuccess", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        var eventDelegate = (MulticastDelegate) typeof(PlayFabManager).GetField("OnLoginSuccess", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
                        if (eventDelegate != null)
                        {
                            //ArchiveLogger.Msg(ConsoleColor.Red, $"OnLoginSuccess is NOT null!");
                            foreach (var handler in eventDelegate.GetInvocationList())
                            {
                                ArchiveLogger.Msg(ConsoleColor.Red, $"OnLoginSuccess: calling {handler.Method.DeclaringType.Name}.{handler.Method.Name}()");
                                handler.Method.Invoke(handler.Target, null);
                            }
                        }
                        else
                        {
                            ArchiveLogger.Error("OnLoginSuccess is null!");
                        }
                    }));





                    //
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayFabManager), "Awake")]
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


        [HarmonyPatch(typeof(PlayFabManager), "AddToOrUpdateLocalPlayerTitleData", new Type[] { typeof(Dictionary<string, string>), typeof(Action) })]
        internal class PlayFabManager_AddToOrUpdateLocalPlayerTitleDataPatch
        {
            public static bool Prefix(Action OnSuccess)
            {
                if (DisableAllPlayFabInteraction)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled AddToOrUpdateLocalPlayerTitleData");
                    // other stuff maybe ?
                    OnSuccess?.Invoke();

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayFabManager), "CloudGiveAlwaysInInventory")]
        internal class PlayFabManager_CloudGiveAlwaysInInventoryPatch
        {
            public static bool Prefix(Action onSucess)
            {
                if (DisableAllPlayFabInteraction)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled CloudGiveAlwaysInInventory");
                    // other stuff maybe ?
                    onSucess?.Invoke();

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayFabManager), "CloudGiveItemToLocalPlayer")]
        internal class PlayFabManager_CloudGiveItemToLocalPlayerPatch
        {
            public static bool Prefix()
            {
                if (DisableAllPlayFabInteraction)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled CloudGiveItemToLocalPlayer");
                    // other stuff maybe ?
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayFabManager), "JSONTest")]
        internal class PlayFabManager_JSONTestPatch
        {
            public static bool Prefix()
            {
                ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled JSONTest - why is this being run in the first place?");
                return false;
            }
        }

        // Will never be called because it's private and it's caller is patched
        /*[HarmonyPatch(typeof(PlayFabManager), "RefreshGlobalTitleData")]
        internal class RefreshGlobalTitleDataPatch
        {
            public static bool Prefix(Action OnSuccess)
            {
                if (DisableAllPlayFabInteraction)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled RefreshGlobalTitleData");
                    // other stuff maybe ?
                    OnSuccess?.Invoke();

                    return false;
                }

                return true;
            }

            public static void Postfix(PlayFabManager __instance, Dictionary<string, string> ___m_globalTitleData)
            {
                ArchiveLogger.Error("RefreshGlobalTitleData:START");
                foreach (KeyValuePair<string, string> kvp in ___m_globalTitleData)
                {
                    ArchiveLogger.Msg($"{kvp.Key} : {kvp.Value}");
                }
                ArchiveLogger.Error("RefreshGlobalTitleData:END");

            }
        }*/

        [HarmonyPatch(typeof(PlayFabManager), "RefreshItemCatalog")]
        internal class PlayFabManager_RefreshItemCatalogPatch
        {
            public static bool Prefix()
            {
                if (DisableAllPlayFabInteraction)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled RefreshItemCatalog");
                    // other stuff maybe ?
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayFabManager), "RefreshLocalPlayerInventory")]
        internal class RefreshLocalPlayerInventoryPatch
        {
            public static bool Prefix(PlayFabManager __instance, PlayFabManager.delUpdatePlayerInventoryDone OnSuccess)
            {
                if (DisableAllPlayFabInteraction)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled RefreshLocalPlayerInventory");
                    // other stuff maybe ?
                    return false;
                }

                OnSuccess += test;
                return true;
            }
            public static void test(List<ItemInstance> instance)
            {
                ArchiveLogger.Error("RefreshLocalPlayerInventory:List<ItemInstance>:START");
                foreach (ItemInstance ii in instance)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"{ii}");
                }
                ArchiveLogger.Error("RefreshLocalPlayerInventory:List<ItemInstance>:END");
            }
        }


        [HarmonyPatch(typeof(PlayFabManager), "RefreshLocalPlayerTitleData")]
        internal class RefreshLocalPlayerTitleDataPatch
        {
            public static bool Prefix()
            {
                if (DisableAllPlayFabInteraction)
                {
                    //ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled RefreshLocalPlayerTitleData");
                    // other stuff maybe ?
                    //GearManager.SetupGearInOfflineMode();

                    return false;
                }

                return true;
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

        [HarmonyPatch(typeof(PlayFabManager), "RefreshStartupScreenTitelData")]
        internal class PlayFabManager_RefreshStartupScreenTitelDataPatch
        {
            public static bool Prefix(Action OnSuccess)
            {
                if (DisableAllPlayFabInteraction)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled RefreshStartupScreenTitelData");
                    // other stuff maybe ?

                    OnSuccess?.Invoke();

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayFabManager), "RefreshStoreItems")]
        internal class PlayFabManager_RefreshStoreItemsPatch
        {
            public static bool Prefix()
            {
                if (DisableAllPlayFabInteraction)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkYellow, "Canceled RefreshStoreItems");
                    // other stuff maybe ?
                    return false;
                }

                return true;
            }
        }

    }
}
