using CellMenu;
using SNetwork;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features
{
    [EnableFeatureByDefault]
    public class CustomLobbyCodeString : Feature
    {
        public override string Name => "Copy Lobby ID Format";

        public override string Description => "Customize copied lobby code from the 'Copy Lobby ID'-Button on the loadout and settings screens with a custom format.";

        public override bool PlaceSettingsInSubMenu => true;

        public new static IArchiveLogger FeatureLogger { get; set; }

        public const string DefaultFormat = "LF%OpenSlots% %Rundown%%Expedition% \"%ExpeditionName%\": `%LobbyID%`";

        public class CustomLobbyCodeStringConfig
        {
            [FSMaxLength(120)]
            public string Format { get; set; } = DefaultFormat;
        }
        
        [FeatureConfig]
        public static CustomLobbyCodeStringConfig Config { get; set; }

        private static CM_Item _CM_PageLoadout_coppyLobbyIDButton;
        private static CM_Item _CM_PageSettings_coppyLobbyIDButton;

        [SetEnabledStatus]
        public static bool IsEnabled { get; set; }

        public override void OnEnable()
        {
            if(_CM_PageLoadout_coppyLobbyIDButton == null)
            {
                SetupViaInstance(CM_PageLoadout.Current);
            }
        }

        public static void CopyLobbyIdToClipboard(int _)
        {
            if(!IsEnabled)
            {
                var formatedLobbyIdVanillaish = PresenceFormatter.FormatPresenceString("%LobbyID%");
                GUIUtility.systemCopyBuffer = formatedLobbyIdVanillaish;
                FeatureLogger.Notice($"Copied lobby id to clipboard: {formatedLobbyIdVanillaish}");
                return;
            }

            if (BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownFive.ToLatest()) && IsExternalMatchMakingActive())
            {
                return;
            }

            if (!SNet.IsInLobby)
            {
                return;
            }

            if(string.IsNullOrWhiteSpace(Config.Format))
            {
                Config.Format = DefaultFormat;
            }

            var formatedLobbyId = PresenceFormatter.FormatPresenceString(Config.Format);
            GUIUtility.systemCopyBuffer = formatedLobbyId;
            FeatureLogger.Notice($"Copied lobby id to clipboard: {formatedLobbyId}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsExternalMatchMakingActive()
        {
#if IL2CPP
            return SNet.IsExternalMatchMakingActive;
#else
            return false;
#endif
        }

        public static void SetupViaInstance(CM_PageSettings pageSettings)
        {
            if (pageSettings == null) return;

#if IL2CPP
            _CM_PageSettings_coppyLobbyIDButton = pageSettings.m_copyLobbyIdButton;
#else
            _CM_PageSettings_coppyLobbyIDButton = pageSettings.m_movingContentHolder.GetChildWithExactName("CM_RedButtonFramed(Clone)")?.GetComponent<CM_Item>();
#endif
            if (_CM_PageSettings_coppyLobbyIDButton != null)
            {
                FeatureLogger.Info("Hooking CM_PageSettings Copy Lobby ID Button ...");
                _CM_PageSettings_coppyLobbyIDButton.RemoveCMItemEvents(keepHover: true).SetCMItemEvents(CopyLobbyIdToClipboard);
            }
            else
            {
                FeatureLogger.Warning($"[{nameof(CustomLobbyCodeString)}] copy lobby id button in {nameof(CM_PageSettings)} wasn't found!!!");
            }
        }

        public static void SetupViaInstance(CM_PageLoadout pageLoadout)
        {
            if (pageLoadout == null) return;

            _CM_PageLoadout_coppyLobbyIDButton = pageLoadout.m_copyLobbyIdButton;

            if (_CM_PageLoadout_coppyLobbyIDButton != null)
            {
                FeatureLogger.Info("Hooking CM_PageLoadout Copy Lobby ID Button ...");
                _CM_PageLoadout_coppyLobbyIDButton.RemoveCMItemEvents(keepHover: true).SetCMItemEvents(CopyLobbyIdToClipboard);
            }
            else
            {
                FeatureLogger.Warning($"[{nameof(CustomLobbyCodeString)}] copy lobby id button in {nameof(CM_PageLoadout)} wasn't found!!!");
            }
        }

        [ArchivePatch(typeof(CM_PageSettings), nameof(CM_PageSettings.Setup))]
        internal static class CM_PageSettings_SetupPatch
        {
            public static void Postfix(CM_PageSettings __instance)
            {
                SetupViaInstance(__instance);
            }
        }

        [ArchivePatch(typeof(CM_PageLoadout), nameof(CM_PageLoadout.Setup))]
        internal static class CM_PageLoadout_SetupPatch
        {
            public static void Postfix(CM_PageLoadout __instance)
            {
                SetupViaInstance(__instance);
            }
        }
    }
}
