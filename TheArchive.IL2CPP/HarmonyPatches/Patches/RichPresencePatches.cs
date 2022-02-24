﻿using CellMenu;
using SNetwork;
using System;
using System.Linq;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Core.ArchivePatcher;
using static TheArchive.Utilities.PresenceFormatter;
using static TheArchive.Utilities.Utils;

namespace TheArchive.HarmonyPatches.Patches
{
    public class RichPresencePatches
    {


        [PresenceFormatProvider("LobbyID")]
        public static string LobbyID => SNet.Lobby?.Identifier?.ID.ToString() ?? "0123456789";

        [PresenceFormatProvider("OpenSlots")]
        public static int OpenSlots
        {
            get
            {
                return Core.Managers.PresenceManager.MaxPlayerSlots - GetPlayerCount();
            }
        }

        private static int GetPlayerCount()
        {
            if (FlagsContain(RundownFlags.RundownSix.To(RundownFlags.Latest), ArchiveMod.CurrentRundown))
                return GetPlayerCountR6Plus();

            return SNet.Lobby?.Players?.Count ?? 1;
        }

        private static int GetPlayerCountR6Plus()
        {
            return SNet.Lobby?.Players.ToSystemList()?.Where(ply => !ply.IsBot)?.Count() ?? 1;
        }

        [PresenceFormatProvider("ExpeditionTier")]
        public static string ExpeditionTier => RundownManager.ActiveExpedition?.Descriptive?.Prefix ?? "?";

        [PresenceFormatProvider("ExpeditionNumber")]
        public static int ExpeditionNumber { get; set; } = 0;

        [PresenceFormatProvider("ExpeditionName")]
        public static string ExpeditionName => RundownManager.ActiveExpedition?.Descriptive?.PublicName ?? "???";

        // Disables or changes Steam rich presence
        [ArchivePatch(typeof(SNet_Core_STEAM), "SetFriendsData", new Type[] { typeof(FriendsDataType), typeof(string) })]
        internal static class SNet_Core_STEAM_SetFriendsDataPatch
        {
            public static void Prefix(FriendsDataType type, ref string data)
            {
                if (ArchiveMod.Settings.DisableSteamRichPresence)
                {
                    data = string.Empty;
                    return;
                }

                if (type == FriendsDataType.ExpeditionName)
                {
                    data = $"{FormatPresenceString("%Rundown%%Expedition%")} \"{data}\"";
                }
                //ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"{nameof(SNet_Core_STEAM)}.{nameof(SNet_Core_STEAM.SetFriendsData)} called: \"{type}\": {data}");
            }
        }

        // RundownManager.SetActiveExpedition(pActiveExpedition expPackage, ExpeditionInTierData expTierData) calls:
        // RundownManager.GetUniqueExpeditionKey(string rundownKey, eRundownTier tier, int expIndex)
        [ArchivePatch(typeof(RundownManager), nameof(RundownManager.GetUniqueExpeditionKey), new Type[] { typeof(string), typeof(eRundownTier), typeof(int) })]
        internal static class RundownManager_SetActiveExpeditionPatch
        {
            public static void Postfix(string rundownKey, eRundownTier tier, int expIndex)
            {
                ExpeditionNumber = expIndex + 1;
            }
        }

        public static void CopyLobbyIdToClipboard(int _)
        {
            if (SNet.IsExternalMatchMakingActive)
            {
                return;
            }

            if (!SNet.IsInLobby)
            {
                return;
            }

            var formatedLobbyId = PresenceFormatter.FormatPresenceString(ArchiveMod.Settings.LobbyIdFormatString);
            GUIUtility.systemCopyBuffer = formatedLobbyId;
            ArchiveLogger.Notice($"Copied lobby id to clipboard: {formatedLobbyId}");
        }

        [ArchivePatch(typeof(CM_PageSettings), nameof(CM_PageSettings.Setup))]
        internal static class CM_PageSettings_SetupPatch
        {
            public static void Postfix(CM_PageSettings __instance)
            {
                CM_Item copyLobbyIDButton = (CM_Item) __instance.GetType().GetProperty(nameof(__instance.m_copyLobbyIdButton))?.GetValue(__instance, null);

                if(copyLobbyIDButton != null)
                {
                    ArchiveLogger.Info("Hooking CM_PageSettings Copy Lobby ID Button ...");
                    copyLobbyIDButton.OnBtnPressCallback = (Action<int>) CopyLobbyIdToClipboard;
                }
            }
        }

        [ArchivePatch(typeof(CM_PageLoadout), nameof(CM_PageLoadout.Setup), RundownFlags.RundownFour, RundownFlags.RundownFive)]
        internal static class CM_PageLoadout_SetupPatch
        {
            public static void Postfix(CM_PageLoadout __instance)
            {
                CM_Item copyLobbyIDButton = __instance.m_movingContentHolder?.GetChildWithExactName("ShareServerId")?.GetChildWithExactName("Button Copy Clipboard")?.GetComponent<CM_Item>();

                if(copyLobbyIDButton != null)
                {
                    ArchiveLogger.Info("Hooking CM_PageLoadout Copy Lobby ID Button ...");
                    copyLobbyIDButton.OnBtnPressCallback = (Action<int>) CopyLobbyIdToClipboard;
                }
            }
        }
    }
}