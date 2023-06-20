using Player;
using SNetwork;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Models;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Accessibility
{
    [EnableFeatureByDefault]
    public class PlayerColorOverride : Feature
    {
        public override string Name => "Player Color Override";

        public override string Group => FeatureGroups.Accessibility;

        public override string Description => "Override the built in player colors.";

        public override bool SkipInitialOnEnable => true;

        [FeatureConfig]
        public static PlayerColorOverrideSettings Settings { get; set; }

        public class PlayerColorOverrideSettings
        {
            public ColorizationMode Mode { get; set; } = ColorizationMode.NicknameOnly;

            [FSDisplayName("Your Color")]
            public SColor LocalPlayer { get; set; } = SColorExtensions.FromHexString("#44D2CD");
            [FSDisplayName("Other Players' Color")]
            public SColor OtherPlayers { get; set; } = SColorExtensions.FromHexString("#5AE55D");

            [FSHeader("Character Colors")]
            [FSDisplayName("Color for <#C21F4E>Woods</color> (P1)")]
            public SColor P1_Woods { get; set; } = SColorExtensions.FromHexString("#C21F4E");
            [FSDisplayName("Color for <#18935E>Dauda</color> (P2)")]
            public SColor P2_Dauda { get; set; } = SColorExtensions.FromHexString("#18935E");
            [FSDisplayName("Color for <#20558C>Hackett</color> (P3)")]
            public SColor P3_Hackett { get; set; } = SColorExtensions.FromHexString("#20558C");
            [FSDisplayName("Color for <#7A1A8E>Bishop</color> (P4)")]
            public SColor P4_Bishop { get; set; } = SColorExtensions.FromHexString("#7A1A8E");

            [FSHeader("Misc")]
            [FSDisplayName("Local Nickname Color")]
            [FSDescription("Use the Nickname Features color as your own player color.")]
            public bool LocalUseNicknameColorAsPlayerColor { get; set; } = true;

            [FSDisplayName("Remote Nickname Color")]
            [FSDescription("Use a remote players custom (colored) nickname as the players color.\n\n<color=orange>Warning! This overrides the above settings if the remote player has a colored nickname!</color>")]
            public bool RemoteUseNicknameColorAsPlayerColor { get; set; } = true;

            [FSDisplayName("Only fully colored names")]
            [FSDescription("Only pick the nickname color if the whole name is colored.\n\nExample:\n<#404>Nickname</color> -> <#0c0>use as player color</color>\n<#404>Nick</color>name -> <#c00>don't use as player color</color>")]
            public bool OnlyColorIfWholeNameIsColored { get; set; } = true;

            public enum ColorizationMode
            {
                /// <summary>Only use nickname colors if applicable</summary>
                NicknameOnly,
                /// <summary>Only set the local players color, uses <seealso cref="LocalPlayer"/> as color source</summary>
                LocalOnly,
                /// <summary>Use character values as custom color source</summary> 
                Character,
                /// <summary>Use character values as custom color source, local player uses <seealso cref="LocalPlayer"/></summary>
                CharacterAndLocal,
                /// <summary>Use <seealso cref="OtherPlayers"/> for other players, local player uses <seealso cref="LocalPlayer"/></summary>
                OtherAndLocal
            }
        }

#if MONO
        // R1 build is missing PlayerManager.PlayerAgentsInLevel
        private FieldAccessor<PlayerManager, List<PlayerAgent>> _PlayerManager_m_playerAgentsInLevelAccessor;
        public override void Init()
        {
            if (Is.R1)
            {
                _PlayerManager_m_playerAgentsInLevelAccessor = FieldAccessor<PlayerManager, List<PlayerAgent>>.GetAccessor("m_playerAgentsInLevel");
            }
        }
#endif

        public override void OnEnable()
        {
            foreach(PlayerAgent player in GetAllPlayers())
            {
                if (player == null) continue;
                if (player?.Owner == null) continue;
                PlayerAgent_Setup_Patch.SetupColor(player.Owner, player.Owner.CharacterIndex);
            }
        }

        public override void OnDisable()
        {
            foreach (PlayerAgent player in GetAllPlayers())
            {
                if (player == null) continue;
                if (player?.Owner == null) continue;
                var color = PlayerManager.GetStaticPlayerColor(player.Owner.CharacterIndex);
                player.Owner.PlayerColor = color;
            }
        }

        private IEnumerable<PlayerAgent> GetAllPlayers()
        {
#if MONO
            if(Is.R1)
            {
                return _PlayerManager_m_playerAgentsInLevelAccessor.Get(PlayerManager.Current);
            }
#endif
            return GetAllPlayersR2Plus();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerable<PlayerAgent> GetAllPlayersR2Plus()
        {
            return PlayerManager.PlayerAgentsInLevel.ToArray();
        }

#if IL2CPP
        [RundownConstraint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
        [ArchivePatch(null, nameof(LocalPlayerAgent.Setup))]
        public static class LocalPlayerAgent_Setup_Patch
        {
            public static System.Type Type() => typeof(LocalPlayerAgent);
            public static void Postfix(LocalPlayerAgent __instance, int characterID) => PlayerAgent_Setup_Patch.Postfix(__instance, characterID);
        }
#endif

        [ArchivePatch(typeof(PlayerAgent), nameof(PlayerAgent.Setup))]
        public static class PlayerAgent_Setup_Patch
        {
            public static void Postfix(PlayerAgent __instance, int characterID)
            {
                SetupColor(__instance.Owner, characterID);
            }

            public static void SetupColor(SNet_Player player, int characterID)
            {
                if(player.IsLocal && Settings.LocalUseNicknameColorAsPlayerColor && FeatureManager.IsFeatureEnabled<Nickname>() && Nickname.IsNicknameColorEnabled)
                {
                    player.PlayerColor = Nickname.CurrentNicknameColor;
                    return;
                }

                if (!player.IsLocal && Settings.RemoteUseNicknameColorAsPlayerColor && ShouldApplyNickNameColor(player.Profile.nick.data, out var colNick))
                {
                    player.PlayerColor = colNick;
                    return;
                }

                if (TryGetColor(characterID, out var col))
                {
                    player.PlayerColor = col;
                }
            }

            public static bool TryGetColor(int playerIndex, out Color col)
            {
                if (Settings.Mode == PlayerColorOverrideSettings.ColorizationMode.NicknameOnly)
                {
                    col = Color.white;
                    return false;
                }

                var localPlayerCharacterId = PlayerManager.GetLocalPlayerAgent()?.CharacterID ?? -1;

                if (Settings.Mode == PlayerColorOverrideSettings.ColorizationMode.LocalOnly)
                {
                    if (localPlayerCharacterId == playerIndex)
                    {
                        col = Settings.LocalPlayer.ToUnityColor();
                        return true;
                    }
                    col = Color.white;
                    return false;
                }

                if (Settings.Mode == PlayerColorOverrideSettings.ColorizationMode.CharacterAndLocal
                    && localPlayerCharacterId == playerIndex)
                {
                    col = Settings.LocalPlayer.ToUnityColor();
                    return true;
                }

                switch (Settings.Mode)
                {
                    default:
                    case PlayerColorOverrideSettings.ColorizationMode.CharacterAndLocal:
                    case PlayerColorOverrideSettings.ColorizationMode.Character:
                        switch(playerIndex % 4)
                        {
                            case 0:
                                col = Settings.P1_Woods.ToUnityColor();
                                break;
                            case 1:
                                col = Settings.P2_Dauda.ToUnityColor();
                                break;
                            case 2:
                                col = Settings.P3_Hackett.ToUnityColor();
                                break;
                            default:
                            case 3:
                                col = Settings.P4_Bishop.ToUnityColor();
                                break;
                        }
                        break;
    
                    case PlayerColorOverrideSettings.ColorizationMode.OtherAndLocal:
                        if(localPlayerCharacterId == playerIndex)
                        {
                            col = Settings.LocalPlayer.ToUnityColor();
                            break;
                        }

                        col = Settings.OtherPlayers.ToUnityColor();
                        break;
                }
                return true;
            }
        }

        private static readonly Regex _ShortColorTagRegex = new Regex(@"\<\#([0-9a-fA-F]{3})\>");

        public static bool GetColorFromNickname(string nick, out Color color)
        {
            color = Color.white;
            var match = _ShortColorTagRegex.Match(nick);

            if (!match.Success)
                return false;

            if (match.Groups.Count <= 1)
                return false;

            var hexString = match.Groups[1].ToString();

            return ColorUtility.TryParseHtmlString($"#{hexString}", out color);
        }

        public static bool ShouldApplyNickNameColor(string nick, out Color color)
        {
            if (!GetColorFromNickname(nick, out color))
                return false;

            if(Settings.OnlyColorIfWholeNameIsColored)
            {
                var i = nick.IndexOf("<#");
                if(i > 0)
                {
                    var preString = nick.Substring(0, i);

                    switch (preString)
                    {
                        case "<i>":
                        case "<b>":
                        case "<s>":
                        case "<u>":
                            break;
                        default:
                            return false;
                    }
                }
                else if (!nick.StartsWith("<#"))
                    return false;

                if (nick.Contains("</color>"))
                {
                    if (nick.EndsWith("</color>"))
                        return true;

                    return false;
                }
            }

            return true;
        }

        [ArchivePatch(typeof(SNet_Player), nameof(SNet_Player.UpdateVisibleName))]
        internal static class SNet_Player_UpdateVisibleName_Patch
        {
            public static void Prefix(SNet_Player __instance)
            {
                if (!Settings.RemoteUseNicknameColorAsPlayerColor)
                    return;

                if (!ShouldApplyNickNameColor(__instance.Profile.nick.data, out var color))
                    return;

                __instance.PlayerColor = color;
            }
        }
    }
}
