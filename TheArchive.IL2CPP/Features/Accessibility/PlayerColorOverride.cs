using Player;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Models;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Accessibility
{
    public class PlayerColorOverride : Feature
    {
        public override string Name => "Player Color Override";

        public override string Group => FeatureGroups.Accessibility;

        [FeatureConfig]
        public static PlayerColorOverrideSettings Settings { get; set; }

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
                Color col = new Color();

                GetColor(ref col, characterID);

                __instance.Owner.PlayerColor = col;
            }

            public static void GetColor(ref Color col, int playerIndex)
            {
                if (Settings.Mode == PlayerColorOverrideSettings.ColorizationMode.CharacterAndLocal
                    && PlayerManager.GetLocalPlayerAgent().CharacterID == playerIndex)
                {
                    col = Settings.LocalPlayer.ToUnityColor();
                    return;
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
                            return;
                        }

                        col = Settings.OtherPlayers.ToUnityColor();
                        return;
                }
            }
        }

        public class PlayerColorOverrideSettings
        {
            public ColorizationMode Mode { get; set; } = ColorizationMode.Character;

            public SColor LocalPlayer { get; set; } = SColorExtensions.FromHexString("#44D2CD");
            public SColor OtherPlayers { get; set; } = SColorExtensions.FromHexString("#5AE55D");


            public SColor P1_Woods { get; set; } = SColorExtensions.FromHexString("#C21F4E");
            public SColor P2_Dauda { get; set; } = SColorExtensions.FromHexString("#18935E");
            public SColor P3_Hackett { get; set; } = SColorExtensions.FromHexString("#20558C");
            public SColor P4_Bishop { get; set; } = SColorExtensions.FromHexString("#7A1A8E");

            public enum ColorizationMode
            {
                /// <summary>Use character as custom color source</summary> 
                Character,
                /// <summary>Use character as custom color source, local player uses <seealso cref="LocalPlayer"/></summary>
                CharacterAndLocal,
                /// <summary>Custom Color <seealso cref="LocalPlayer"/> for local player, all other players use the same color: <seealso cref="OtherPlayers"/></summary>
                OtherAndLocal
            }
        }
    }
}
