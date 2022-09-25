using Player;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Models;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Accessibility
{
    public class PlayerColorOverride : Feature
    {
        public override string Name => "Player Color Override";

        public override string Group => FeatureGroups.Accessibility;

        public override bool SkipInitialOnEnable => true;

        [FeatureConfig]
        public static PlayerColorOverrideSettings Settings { get; set; }

#if MONO
        // R1 build is missing PlayerManager.PlayerAgentsInLevel
        private FieldAccessor<PlayerManager, List<PlayerAgent>> _PlayerManager_m_playerAgentsInLevelAccessor;
        public override void Init()
        {
            if (BuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownOne))
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
                PlayerAgent_SetupPatch.SetupColor(player.Owner, player.Owner.CharacterIndex);
            }
        }

        public override void OnDisable()
        {
            foreach (PlayerAgent player in GetAllPlayers())
            {
                if (player == null) continue;
                var color = PlayerManager.GetStaticPlayerColor(player.Owner.CharacterIndex);
                player.Owner.PlayerColor = color;
            }
        }

        private IEnumerable<PlayerAgent> GetAllPlayers()
        {
#if MONO
            if(BuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownOne))
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
        public static class LocalPlayerAgent_SetupPatch
        {
            public static System.Type Type() => typeof(LocalPlayerAgent);
            public static void Postfix(LocalPlayerAgent __instance, int characterID) => PlayerAgent_SetupPatch.Postfix(__instance, characterID);
        }
#endif

        [ArchivePatch(typeof(PlayerAgent), nameof(PlayerAgent.Setup))]
        public static class PlayerAgent_SetupPatch
        {
            public static void Postfix(PlayerAgent __instance, int characterID)
            {
                SetupColor(__instance.Owner, characterID);
            }

            public static void SetupColor(SNetwork.SNet_Player player, int characterID)
            {
                Color col = new Color();

                if (TryGetColor(characterID, ref col))
                {
                    player.PlayerColor = col;
                }
            }

            public static bool TryGetColor(int playerIndex, ref Color col)
            {
                if (Settings.Mode == PlayerColorOverrideSettings.ColorizationMode.LocalOnly)
                {
                    if (PlayerManager.GetLocalPlayerAgent().CharacterID == playerIndex)
                    {
                        col = Settings.LocalPlayer.ToUnityColor();
                        return true;
                    }
                    return false;
                }

                if (Settings.Mode == PlayerColorOverrideSettings.ColorizationMode.CharacterAndLocal
                    && PlayerManager.GetLocalPlayerAgent().CharacterID == playerIndex)
                {
                    col = Settings.LocalPlayer.ToUnityColor();
                    return true;
                }

                switch (Settings.Mode)
                {
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
                            case 3:
                                col = Settings.P4_Bishop.ToUnityColor();
                                break;
                        }
                        break;
    
                    case PlayerColorOverrideSettings.ColorizationMode.OtherAndLocal:
                        if(PlayerManager.GetLocalPlayerAgent().CharacterID == playerIndex)
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

        public class PlayerColorOverrideSettings
        {
            public ColorizationMode Mode { get; set; } = ColorizationMode.Character;

            [FSDisplayName("Your Color")]
            public SColor LocalPlayer { get; set; } = SColorExtensions.FromHexString("#44D2CD");
            [FSDisplayName("Other Players' Color")]
            public SColor OtherPlayers { get; set; } = SColorExtensions.FromHexString("#5AE55D");

            [FSDisplayName("Color for <#C21F4E>Woods</color> (P1)")]
            public SColor P1_Woods { get; set; } = SColorExtensions.FromHexString("#C21F4E");
            [FSDisplayName("Color for <#18935E>Dauda</color> (P2)")]
            public SColor P2_Dauda { get; set; } = SColorExtensions.FromHexString("#18935E");
            [FSDisplayName("Color for <#20558C>Hackett</color> (P3)")]
            public SColor P3_Hackett { get; set; } = SColorExtensions.FromHexString("#20558C");
            [FSDisplayName("Color for <#7A1A8E>Bishop</color> (P4)")]
            public SColor P4_Bishop { get; set; } = SColorExtensions.FromHexString("#7A1A8E");

            public enum ColorizationMode
            {
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
    }
}
