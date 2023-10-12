using Player;
using SNetwork;
using Steamworks;
using System;
using System.Text.RegularExpressions;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Components;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using TMPro;
using UnityEngine;

namespace TheArchive.Features.Accessibility
{
    public class Nickname : Feature
    {
        public override string Name => "Nickname";

        public override string Group => FeatureGroups.Accessibility;

        public override string Description => "Nickname related settings.";

        public class NicknameSettings
        {
            [FSMaxLength(25), FSDisplayName("Nickname")]
            [FSDescription("A custom Nickname.\nOverrides your steam name in game.\nLimited to 25 characters maximum\n\n<color=green>If 11 or less characters: Full color support</color>\n<color=orange>If 19 or less characters: Partial color support, colors are gonna escape!</color>\n<color=red>If 20 or more characters: No color support</color>")]
            public string Nickname { get; set; }

            [FSDisplayName("Use Color")]
            [FSDescription("If the color below should be used as nickname color\nUsing a color removes 6 characters from your nickname budget minimum!")]
            public bool UseColor { get; set; } = false;

            [FSDescription("The color used to tint your name.")]
            public SColor Color { get; set; } = new SColor(0f, 1f, 0.75f);

            public FLabel InfoLabelTop { get; set; } = new FLabel("///");
            public FLabel InfoLabelBottom { get; set; } = new FLabel("///");

            [FSHeader("Nickname :// Others")]
            [FSDisplayName("Allow Other Player Nicknames"), FSIdentifier("AllowNicks")]
            public bool AllowRemotePlayerNicknames { get; set; } = true;
            [FSDisplayName("See Other Players Colors"), FSIdentifier("AllowTags")]
            public bool AllowRemoteTMPTags { get; set; } = true;

            [FSHide]
            public bool HasMigrated { get; set; } = false;

            [FSHide]
            public FLabel HiddenSettingsWarningLabel { get; set; } = new FLabel("<color=red>Old settings below, do not use!</color>");

            [FSHide]
            [FSDescription($"Modes explained using <#C21F4E>this</color> as the character color and <#404>this</color> as the custom color:\n\n<#E4A818>{nameof(NicknameMode.Normal)}</color>:\n <#C21F4E>> > Nickname</color>: Some chat message here.\n<#E4A818>{nameof(NicknameMode.Color)}</color>:\n <#C21F4E>> > <#404>Nickname</color>: Some chat message here.\n</color><#E4A818>{nameof(NicknameMode.TerminatedColor)}</color>:\n <#C21F4E>> > <#404>Nickname</color></color>: Some chat message here.")]
            public NicknameMode Mode { get; set; } = NicknameMode.Normal;

            [FSHide]
            [FSMaxLength(25), FSDisplayName("Nick (25) (No Color)")]
            [FSDescription($"Un-colored nickname (use character color), max length of 25 characters.\n\nOnly used if Mode is set to <#E4A818>{nameof(NicknameMode.Normal)}</color>!")]
            public string Nick25 { get; set; }

            [FSHide]
            [FSMaxLength(19), FSDisplayName("Nick (19) <#E4A818>(Colored)</color>")]
            [FSDescription($"Colored nickname, max length of 19 characters.\n\nOnly used if Mode is set to <#E4A818>{nameof(NicknameMode.Color)}</color>!")]
            public string Nick19 { get; set; }

            [FSHide]
            [FSMaxLength(11), FSDisplayName("Nick (11) <#E4A818>(C + Terminated)</color>")]
            [FSDescription($"Terminated colored nickname, max length of 11 characters.\n\nOnly used if Mode is set to <#E4A818>{nameof(NicknameMode.TerminatedColor)}</color>!")]
            public string Nick11 { get; set; }
        }

        [FeatureConfig]
        public static NicknameSettings Settings { get; set; }

        public static new IArchiveLogger FeatureLogger { get; set; }

        private static Color? _currentNicknameColor;
        public static Color CurrentNicknameColor
        {
            get
            {
                if(_currentNicknameColor == null)
                {
                    if(Settings.Mode == NicknameMode.Advanced && PlayerColorOverride.GetColorFromNickname(Settings.Nickname, out var nickColor))
                    {
                        _currentNicknameColor = nickColor;
                    }
                    else
                    {
                        _currentNicknameColor = Settings.Color.ToUnityColor();
                    }
                    
                }
                return _currentNicknameColor.Value;
            }
        }

        public static bool IsNicknameColorEnabled
        {
            get
            {
                switch(Settings.Mode)
                {
                    case NicknameMode.Color:
                    case NicknameMode.TerminatedColor:
                    case NicknameMode.Advanced:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public override void Init()
        {
            HandleSettingsChanged();
        }

        public override void OnEnable()
        {
            if (string.IsNullOrEmpty(Settings.Nickname) && !Settings.HasMigrated)
            {
                MigrateOldSettings();
            }

            SetNickname();
            PlayerManager.GetLocalPlayerAgent()?.Owner?.UpdateVisibleName();
        }

        private static bool _hasMigrated = false;

        private void MigrateOldSettings()
        {
            Settings.HasMigrated = true;
            _hasMigrated = true;

            switch (Settings.Mode)
            {
                case NicknameMode.TerminatedColor:
                case NicknameMode.Color:
                    Settings.UseColor = true;
                    break;
                default:
                    Settings.UseColor = false;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(Settings.Nick11))
            {
                Settings.Nickname = Settings.Nick11;
                return;
            }

            if (!string.IsNullOrWhiteSpace(Settings.Nick19))
            {
                Settings.Nickname = Settings.Nick19;
                return;
            }

            if (!string.IsNullOrWhiteSpace(Settings.Nick25))
            {
                Settings.Nickname = Settings.Nick25;
                return;
            }
        }

        public override void OnDisable()
        {
            if (IsApplicationQuitting)
                return;
            
            ResetNickname();
        }

        public void OnGameStateChanged(eGameStateName state)
        {
            if(state == eGameStateName.NoLobby)
            {
                FeatureLogger.Notice($"State changed to {eGameStateName.NoLobby}, initially setting nickname ...");
                if(_hasMigrated)
                {
                    _hasMigrated = false;
                    MarkSettingsAsDirty(Settings);
                }
                HandleSettingsChanged();
                SetNickname();
                PlayerManager.GetLocalPlayerAgent()?.Owner?.UpdateVisibleName();
            }
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            HandleSettingsChanged();

            if(Settings.Mode != NicknameMode.Advanced)
            {
                _currentNicknameColor = Settings.Color.ToUnityColor();
            }

            if (setting.Identifier.StartsWith("Allow"))
            {
                foreach(var player in SNet.LobbyPlayers)
                {
                    if (player.IsLocal) continue;

                    player.UpdateVisibleName();
                }
            }
            else
            {
                SetNickname();
                PlayerManager.GetLocalPlayerAgent()?.Owner?.UpdateVisibleName();
            }
        }

        private void HandleSettingsChanged()
        {
            var nickLength = Settings.Nickname.Length;

            bool advancedMode = Settings.Nickname.IndexOf("<#") >= 0;

            string colPrefix = string.Empty;

            if (nickLength > 0 && nickLength <= 11 && Settings.UseColor && !advancedMode)
            {
                Settings.Mode = NicknameMode.TerminatedColor;
                Settings.Nick11 = Settings.Nickname;
                colPrefix = $"<{Settings.Color.ToShortHexString()}>";
            }
            else if (nickLength > 0 && nickLength <= 19 && Settings.UseColor && !advancedMode)
            {
                Settings.Mode = NicknameMode.Color;
                Settings.Nick19 = Settings.Nickname;
                colPrefix = $"<{Settings.Color.ToShortHexString()}>";
            }
            else
            {
                Settings.Mode = NicknameMode.Normal;
                Settings.Nick25 = Settings.Nickname;
            }

            if (advancedMode)
            {
                Settings.Mode = NicknameMode.Advanced;
                if(PlayerColorOverride.GetColorFromNickname(Settings.Nickname, out var nickColor))
                {
                    _currentNicknameColor = nickColor;
                }
            }

            var text = $"Nickname budget: {nickLength} / 25  =>  {colPrefix}{Settings.Nickname}";
            if (Settings.InfoLabelTop.HasPrimaryText)
                Settings.InfoLabelTop.PrimaryText.TryCastTo<TextMeshPro>().text = text;
            Settings.InfoLabelTop.LabelText = text;

            var textBottom = Settings.Mode switch
            {
                NicknameMode.Color => "<color=orange><u>/!\\</u></color> Your Player color is going to escape! (Max 11 characters)",
                NicknameMode.TerminatedColor => "Colored name applied!",
                _ => ""
            };

            if (nickLength > 19 && Settings.UseColor)
                textBottom = "<color=red><u>/!\\</u></color> Name too long, can't apply color! (Max 19 characters)";

            if (advancedMode)
            {
                textBottom = $"<#440144><u>/!\\</u></color> Custom Color Tags detected! => <noparse>{Settings.Nickname}</noparse>";
            }

            if (Settings.InfoLabelBottom.HasPrimaryText)
                Settings.InfoLabelBottom.PrimaryText.TryCastTo<TextMeshPro>().text = textBottom;
            Settings.InfoLabelBottom.LabelText = textBottom;
        }

        public static void ResetNickname() => ResetNickname(SNet.LocalPlayer);
        public static void ResetNickname(SNet_Player player, bool callUpdateVisiblePlayerName = true)
        {
            if (player == null) return;

            try
            {
                var data = player?.PlatformData?.TryCastTo<SNet_SteamPlayerData>();
                if (data != null)
                {
                    var personaName = SteamFriends.GetFriendPersonaName(data.SteamID);
                    player.NickName = Utils.StripTMPTagsRegex(personaName);
                }

                if(callUpdateVisiblePlayerName)
                    player.UpdateVisibleName();
            }
            catch(Exception ex)
            {
                if(!ex.Message.Contains("Steamworks"))
                {
                    throw ex;
                }
            }
        }

        public static void SetNickname()
        {
            if(SNet.LocalPlayer == null) return;

            switch (Settings.Mode)
            {
                default:
                case NicknameMode.Normal:
                    if(!string.IsNullOrWhiteSpace(Settings.Nick25))
                    {
                        SNet.LocalPlayer.NickName = Settings.Nick25;
                        return;
                    }
                    
                    break;
                case NicknameMode.Color:
                    if (!string.IsNullOrWhiteSpace(Settings.Nick19))
                    {
                        SNet.LocalPlayer.NickName = $"<{Settings.Color.ToShortHexString()}>{Settings.Nick19}";
                        return;
                    }
                    break;
                case NicknameMode.TerminatedColor:
                    if (!string.IsNullOrWhiteSpace(Settings.Nick11))
                    {
                        SNet.LocalPlayer.NickName = $"<{Settings.Color.ToShortHexString()}>{Settings.Nick11}</color>";
                        return;
                    }
                    break;
            }

            ResetNickname();
        }

        [ArchivePatch(typeof(PUI_GameEventLog), "AddLogItem")]
        public static class PUI_GameEventLog_AddLogItem_Patch
        {
            public static void Prefix(ref string log, eGameEventChatLogType type)
            {
                if (Settings.Mode != NicknameMode.Color) return;
                if (type != eGameEventChatLogType.OutgoingChat) return;

                var match = Regex.Match(log, @"<color=#.+<\/color>");

                if(!match.Success)
                {
                    FeatureLogger.Warning("Couldn't parse outgoing chat message from local player! This should not happen?!");
                    return;
                }

                var chatMsgAndColon = log.Remove(0, match.Value.Length);

                log = $"{match.Value}</color>{chatMsgAndColon}";
            }
        }

        [ArchivePatch(typeof(SNet_Player), nameof(SNet_Player.UpdateVisibleName))]
        public static class SNet_Player_UpdateVisibleName_Patch
        {
            public static bool Prefix(SNet_Player __instance)
            {
                if (__instance.IsLocal) return ArchivePatch.RUN_OG;

                if (!Settings.AllowRemotePlayerNicknames)
                {
                    ResetNickname(__instance, false);
                    return ArchivePatch.SKIP_OG;
                }

                if(!Settings.AllowRemoteTMPTags)
                {
                    __instance.Profile.nick.data = Utils.StripTMPTagsRegex(__instance.Profile.nick.data);
                }

                return ArchivePatch.RUN_OG;
            }
        }

        public enum NicknameMode
        {
            Normal,
            Color,
            TerminatedColor,
            Advanced
        }
    }
}
