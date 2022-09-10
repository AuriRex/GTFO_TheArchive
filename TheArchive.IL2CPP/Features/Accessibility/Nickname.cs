using Player;
using SNetwork;
using Steamworks;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Models;
using TheArchive.Utilities;

namespace TheArchive.Features.Accessibility
{
    public class Nickname : Feature
    {
        public override string Name => "Nickname";

        public override string Group => FeatureGroups.Accessibility;

        public class NicknameSettings
        {
            public NicknameMode Mode { get; set; } = NicknameMode.Normal;
            [FSMaxLength(25), FSDisplayName("Nick (25) no Color")]
            public string Nick25 { get; set; }
            public SColor Color { get; set; } = new SColor(0f, 1f, 0.75f);
            [FSMaxLength(19), FSDisplayName("Nick (19) ++ Color")]
            public string Nick19 { get; set; }
            [FSMaxLength(11), FSDisplayName("Nick (11) + Color")]
            public string Nick11 { get; set; }

            [FSDisplayName("Allow Other Player Nicknames"), FSIdentifier("AllowNicks")]
            public bool AllowRemotePlayerNicknames { get; set; } = true;
            [FSDisplayName("See Other Players Colors"), FSIdentifier("AllowTags")]
            public bool AllowRemoteTMPTags { get; set; } = true;
        }

        [FeatureConfig]
        public static NicknameSettings Settings { get; set; }

        public override void OnEnable()
        {
            SetNickname();
        }

        public override void OnDisable()
        {
            ResetNickname();
        }

        public void OnGameStateChanged(eGameStateName state)
        {
            if(state == eGameStateName.NoLobby)
            {
                FeatureLogger.Notice("State changed, setting nickname");
                SetNickname();
            }
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            if(setting.Identifier.StartsWith("Allow"))
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
            }
        }

        public static void ResetNickname() => ResetNickname(SNet.LocalPlayer);
        public static void ResetNickname(SNet_Player player)
        {
            if (player == null) return;

            try
            {
                var data = PlayerManager.GetLocalPlayerAgent()?.Owner?.PlatformData?.TryCastTo<SNet_SteamPlayerData>();
                if (data != null)
                {
                    var personaName = SteamFriends.GetFriendPersonaName(data.SteamID);
                    player.NickName = Utils.StripTMPTagsRegex(personaName);
                }
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
                case NicknameMode.ColorWithOverflow:
                    if(!string.IsNullOrWhiteSpace(Settings.Nick19))
                    {
                        SNet.LocalPlayer.NickName = $"<{Settings.Color.ToShortHexString()}>{Settings.Nick19}";
                        return;
                    }
                    break;
                case NicknameMode.Color:
                    if (!string.IsNullOrWhiteSpace(Settings.Nick11))
                    {
                        SNet.LocalPlayer.NickName = $"<{Settings.Color.ToShortHexString()}>{Settings.Nick11}</color>";
                        return;
                    }
                    break;
            }

            ResetNickname();
        }

        [ArchivePatch(typeof(SNet_Player), nameof(SNet_Player.UpdateVisibleName))]
        public static class SNet_Player_UpdateVisibleName_Patch
        {
            public static bool Prefix(SNet_Player __instance)
            {
                if (__instance.IsLocal) return ArchivePatch.RUN_OG;

                if (!Settings.AllowRemotePlayerNicknames)
                {
                    ResetNickname(__instance);
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
            ColorWithOverflow,
            Color
        }
    }
}
