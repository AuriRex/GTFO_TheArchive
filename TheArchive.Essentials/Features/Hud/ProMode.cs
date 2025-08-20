using System;
using SNetwork;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.Hud;

public class ProMode : Feature
{
    public override string Name => "Hud Pro Mode (Disable HUD)";

    public override FeatureGroup Group => FeatureGroups.Hud;

    public override string Description => "Force disable <i><u>ALL</u></i> HUD layers unless re-enabled via the submenu.\n\nMain purpose is for video production";

    public override bool SkipInitialOnEnable => true;

    [FeatureConfig]
    public static ProModeSettings Settings { get; set; }

    public class ProModeSettings
    {
        [FSDisplayName("Show Navmarkers")]
        [FSDescription("If Player Names, Bio Pings and Player Pings should be shown.")]
        public bool EnableNavmarkers { get; set; } = false;

        [FSDisplayName("Show Warden Intel")]
        [FSDescription("If Objective Info / Reminders should be shown.")]
        public bool EnableWardenIntel { get; set; } = false;

        [FSDisplayName("Show Coms-Menu")]
        [FSDescription("Enable the Communication Menu in order to communicate / command bots.")]
        [FSRundownHint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
        public bool EnableComsMenu { get; set; } = false;

        [FSDisplayName("Show Crosshair")]
        [FSDescription("If the crosshair in the center of the screen should be shown.")]
        public bool EnableCrosshair { get; set; } = false;

        [FSDisplayName("Show Interactions")]
        [FSDescription("If Interactions like picking up resource packs/consumables or reviving players should be shown.")]
        public bool EnableInteractions { get; set; } = false;

        [FSDisplayName("Show Chat")]
        [FSDescription("If the chat should be shown.")]
        public bool EnableChat { get; set; } = false;

        [FSDisplayName("Hide Inactive Chat")]
        [FSDescription("Hide the chat completely after a few seconds of inactivity instead of only dimming it.")]
        public bool HideInactiveChat { get; set; } = false;
    }

    private static eFocusState _eFocusState_MainMenu;
    private static eFocusState _eFocusState_Map;
    private static eFocusState _eFocusState_InActive;
    private static eFocusState _eFocusState_GlobalPopupMessage;
    private static eFocusState _eFocusState_InElevator;
    private static eFocusState _eFocusState_FPS;
    private static eFocusState _eFocusState_FPS_TypingInChat;
    private static eFocusState _eFocusState_FPS_CommunicationsMenu;

    internal static IValueAccessor<PlayerGuiLayer, PUI_GameEventLog> A_PlayerGuiLayer_m_gameEventLog;

    public override void Init()
    {
        _eFocusState_MainMenu = Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.MainMenu));
        _eFocusState_Map = Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.Map));
        _eFocusState_InActive = Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.InActive));
        if (!Utils.TryGetEnumFromName(nameof(eFocusState.GlobalPopupMessage), out _eFocusState_GlobalPopupMessage))
            _eFocusState_GlobalPopupMessage = (eFocusState) (-1);
        _eFocusState_InElevator = Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.InElevator));
        _eFocusState_FPS = Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.FPS));
        _eFocusState_FPS_TypingInChat = Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.FPS_TypingInChat));
#if IL2CPP
        Utils.TryGetEnumFromName(nameof(eFocusState.FPS_CommunicationDialog), out _eFocusState_FPS_CommunicationsMenu);
#endif

        A_PlayerGuiLayer_m_gameEventLog = AccessorBase.GetValueAccessor<PlayerGuiLayer, PUI_GameEventLog>("m_gameEventLog");
    }

    private static string _originalText = null;

    public override void OnEnable()
    {
        if (Is.R4OrLater || GuiManager.PlayerLayer == null)
            return;

        var helpText = A_PlayerGuiLayer_m_gameEventLog.Get(GuiManager.PlayerLayer)?.m_txtHelp;
        if (helpText != null)
        {
            if (string.IsNullOrWhiteSpace(_originalText))
                _originalText = helpText.text;
            helpText.enabled = false;
            helpText.text = string.Empty;
        }
    }

    public override void OnDisable()
    {
        if (IsApplicationQuitting || GuiManager.PlayerLayer == null)
            return;

        // Reshow Helper Text on disable
        var helpText = A_PlayerGuiLayer_m_gameEventLog.Get(GuiManager.PlayerLayer)?.m_txtHelp;
        if(helpText != null)
        {
            helpText.enabled = true;
            if (!Is.R4OrLater && !string.IsNullOrWhiteSpace(_originalText))
                helpText.text = _originalText;
        }
    }

    public static void SetChatVisible(bool visible)
    {
        A_PlayerGuiLayer_m_gameEventLog.Get(GuiManager.PlayerLayer).SetVisible(visible);
    }

    public static IEnumerable<GuiLayer> GetAllLayers()
    {
#if IL2CPP
        return GuiManager.GetAllLayers();
#else
            return new GuiLayer[] {
                GuiManager.PlayerLayer,
                GuiManager.DebugLayer,
                GuiManager.InteractionLayer,
                GuiManager.NavMarkerLayer,
                GuiManager.WatermarkLayer,
                GuiManager.MainMenuLayer,
                GuiManager.CrosshairLayer,
                GuiManager.ConsoleLayer,
                GuiManager.InGameMenuLayer,
                //GuiManager.GlobalPopupMessageLayer
            };
#endif
    }

    [ArchivePatch(typeof(GuiManager), nameof(GuiManager.OnFocusStateChanged))]
    internal static class GuiManager_OnFocusStateChanged_Patch
    {
        private static MethodAccessor<PlayerGuiLayer> _UpdateGUIElementsVisibility;

        public static void Init()
        {
            if (!Is.R5OrLater)
                _UpdateGUIElementsVisibility = MethodAccessor<PlayerGuiLayer>.GetAccessor("UpdateGUIElementsVisibility", new System.Type[] { typeof(bool) });
        }

        public static bool Prefix(eFocusState state)
        {
            if(state == _eFocusState_MainMenu
               || state == _eFocusState_Map
               || state == _eFocusState_InActive
               || state == _eFocusState_GlobalPopupMessage
               || (Is.R6OrLater && Settings.EnableComsMenu && state == _eFocusState_FPS_CommunicationsMenu))
                return ArchivePatch.RUN_OG;

            foreach (var layer in GetAllLayers())
            {
                if (layer.IsVisible())
                    layer.SetVisible(false);
            }

            if (state == _eFocusState_FPS
                || state == _eFocusState_FPS_TypingInChat)
            {
                if (Settings.EnableNavmarkers)
                    GuiManager.NavMarkerLayer.SetVisible(true);

                if (Settings.EnableInteractions)
                    GuiManager.InteractionLayer.SetVisible(true);

                if (Settings.EnableCrosshair)
                    GuiManager.CrosshairLayer.SetVisible(true);

                if (Settings.EnableChat)
                    DoChatVisibilityJank();
            }

            return ArchivePatch.SKIP_OG;
        }

        internal static void DoChatVisibilityJank()
        {
            GuiManager.PlayerLayer.SetVisible(true);
            if (Is.R5OrLater)
            {
                UpdateGUIElementsVisibility_IL2CPP();
            }
            else
            {
                _UpdateGUIElementsVisibility.Invoke(GuiManager.PlayerLayer, true);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UpdateGUIElementsVisibility_IL2CPP()
        {
#if IL2CPP
            GuiManager.PlayerLayer.UpdateGUIElementsVisibility(_eFocusState_InElevator);
#endif
        }
    }

    // Hide Chat instead of dimming
    [ArchivePatch(typeof(PUI_GameEventLog), "Update")]
    internal static class PUI_GameEventLog_Update_Patch
    {
        private static IValueAccessor<PUI_GameEventLog, bool> A_m_logHidden;

        public static void Init()
        {
            A_m_logHidden = AccessorBase.GetValueAccessor<PUI_GameEventLog, bool>("m_logHidden");
        }

        public static void Postfix(PUI_GameEventLog __instance)
        {
            if (!GuiManager.PlayerLayer.IsVisible() || !Settings.HideInactiveChat)
                return;
                
            var logHidden = A_m_logHidden.Get(__instance);
            SetChatVisible(!logHidden);
        }
    }

    // Hide WardenIntel completely
    [ArchivePatch(typeof(PUI_WardenIntel), nameof(PUI_WardenIntel.SetVisible), new Type[] { typeof(bool), typeof(float) })]
    internal static class PUI_WardenIntel_SetVisible_Patch
    {
        public static bool Prefix(PUI_WardenIntel __instance)
        {
            if (Settings.EnableWardenIntel)
                return ArchivePatch.RUN_OG;

            __instance.gameObject.SetActive(false);

            return ArchivePatch.SKIP_OG;
        }
    }

    // Disable the helper text below the chat
#if IL2CPP
    [RundownConstraint(Utils.RundownFlags.RundownFour, Utils.RundownFlags.Latest)]
    [ArchivePatch(typeof(PUI_GameEventLog), nameof(PUI_GameEventLog.UpdateHelpText))]
    internal static class PUI_GameEventLog_UpdateHelpText_Patch
    {
        public static bool Prefix(PUI_GameEventLog __instance, SNet_Player speaker)
        {
            if (speaker != null || (__instance.m_checkPushToTalk && __instance.m_lastPushToTalkStatus))
            {
                __instance.m_txtHelp.enabled = true;
                return ArchivePatch.RUN_OG;
            }

            __instance.m_txtHelp.enabled = false;
            return ArchivePatch.SKIP_OG;
        }
    }
#else
        [ArchivePatch(typeof(PUI_GameEventLog), nameof(PUI_GameEventLog.Setup))]
        internal static class PUI_GameEventLog_Setup_Patch
        {
            public static void Postfix(PUI_GameEventLog __instance)
            {
                _originalText = __instance.m_txtHelp.text;
                __instance.m_txtHelp.enabled = false;
                __instance.m_txtHelp.text = string.Empty;
            }
        }
#endif

    [ArchivePatch(typeof(PlayerGuiLayer), "UpdateGUIElementsVisibility")]
    internal static class PlayerGuiLayer_UpdateGUIElementsVisibility_Patch
    {
        [IsPrefix]
        [RundownConstraint(Utils.RundownFlags.RundownFive, Utils.RundownFlags.Latest)]
        public static void PrefixNew(ref eFocusState currentState)
        {
            if (!Settings.EnableChat)
                return;

            currentState = _eFocusState_InElevator;
        }

        [IsPrefix]
        [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFour)]
        public static void PrefixOld(ref bool inElevator)
        {
            if (!Settings.EnableChat)
                return;

            inElevator = true;
        }

        public static void Postfix()
        {
            if (!Settings.EnableChat)
                return;

            SetChatVisible(true);
        }
    }
}