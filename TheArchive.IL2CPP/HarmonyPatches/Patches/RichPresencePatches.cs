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
        public static string LobbyID => SNet.Lobby.Identifier.ID.ToString();

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

            return SNet.Lobby.Players?.Count ?? 0;
        }

        private static int GetPlayerCountR6Plus()
        {
            return SNet.Lobby.Players.ToSystemList()?.Where(ply => !ply.IsBot)?.Count() ?? 0;
        }

        [PresenceFormatProvider("ExpeditionTier")]
        public static string ExpeditionTier => RundownManager.ActiveExpedition?.Descriptive?.Prefix ?? "?";

        [PresenceFormatProvider("ExpeditionNumber")]
        public static int ExpeditionNumber { get; set; } = 0;

        [PresenceFormatProvider("ExpeditionName")]
        public static string ExpeditionName => RundownManager.ActiveExpedition?.Descriptive?.PublicName ?? "???";

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

        // R4-R6
        [ArchivePatch(typeof(CM_PageSettings), nameof(CM_PageSettings.Setup))]
        internal static class CM_PageSettings_SetupPatch
        {
            public static void Postfix(CM_PageSettings __instance)
            {
                //__instance.m_copyLobbyIdButton
                ArchiveLogger.Info("Hooking CM_PageSettings Copy Lobby ID Button ...");
                CM_Item copyLobbyIdButton = (CM_Item) __instance.GetType().GetProperty(nameof(__instance.m_copyLobbyIdButton))?.GetValue(__instance, null);

                copyLobbyIdButton.OnBtnPressCallback = (Action<int>) CopyLobbyIdToClipboard;
            }
        }

        // R4-R5
        [ArchivePatch(typeof(CM_PageLoadout), nameof(CM_PageLoadout.Setup), RundownFlags.RundownFour, RundownFlags.RundownFive)]
        internal static class CM_PageLoadout_SetupPatch
        {
            public static void Postfix(CM_PageLoadout __instance)
            {
                ArchiveLogger.Info("Hooking CM_PageLoadout Copy Lobby ID Button ...");
                CM_Item copyLobbyIDButton = __instance.m_movingContentHolder?.GetChildWithExactName("ShareServerId")?.GetChildWithExactName("Button Copy Clipboard")?.GetComponent<CM_Item>();

                if(copyLobbyIDButton != null)
                {
                    copyLobbyIDButton.OnBtnPressCallback = (Action<int>) CopyLobbyIdToClipboard;
                }
            }
        }
    }
}
