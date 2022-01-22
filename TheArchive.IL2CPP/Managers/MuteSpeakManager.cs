using Player;
using TheArchive.HarmonyPatches.Patches;
using UnityEngine;

namespace TheArchive.Managers
{
    /// <summary>
    /// Play voice lines on a key press!
    /// </summary>
    public class MuteSpeakManager
    {
        public static bool EnableOtherVoiceBinds { get; set; } = false;

        public static void IfKeySay(KeyCode key, uint soundId)
        {
            if (Input.GetKeyDown(key))
            {
                var localPlayer = PlayerManager.GetLocalPlayerAgent();

                PlayerVoiceManager.WantToSay(localPlayer.CharacterID, soundId);
            }
        }

        public static void IfKeySay(KeyCode key, uint soundId, int characterID)
        {
            if (Input.GetKeyDown(key))
            {
                PlayerVoiceManager.WantToSay(characterID, soundId);
            }
        }

        public static void Update()
        {

            if (StatePatches.LocalPlayerIsInTerminal) return;
            if (PlayerChatManager.InChatMode) return;

            if(Input.GetKey(KeyCode.RightControl))
            {
                IfKeySay(KeyCode.UpArrow, AK.EVENTS.PLAY_CL_NORTH);
                IfKeySay(KeyCode.RightArrow, AK.EVENTS.PLAY_CL_EAST);
                IfKeySay(KeyCode.DownArrow, AK.EVENTS.PLAY_CL_SOUTH);
                IfKeySay(KeyCode.LeftArrow, AK.EVENTS.PLAY_CL_WEST);

                if (EnableOtherVoiceBinds)
                {
                    
                    IfKeySay(KeyCode.P, AK.EVENTS.PLAY_CL_WELLDONE);
                    IfKeySay(KeyCode.L, AK.EVENTS.PLAY_CL_SHH);
                    IfKeySay(KeyCode.K, AK.EVENTS.PLAY_CL_WILLDO);
                    IfKeySay(KeyCode.J, AK.EVENTS.PLAY_CL_YOUTAKE);
                    IfKeySay(KeyCode.H, AK.EVENTS.PLAY_FALLDAMAGEGRUNT02_5A, 2); // Make Hackett say the line xp
                }

                return;
            }

            IfKeySay(KeyCode.LeftArrow, AK.EVENTS.PLAY_CL_LEFT);
            IfKeySay(KeyCode.RightArrow, AK.EVENTS.PLAY_CL_RIGHT);
            IfKeySay(KeyCode.UpArrow, AK.EVENTS.PLAY_CL_YES);
            IfKeySay(KeyCode.DownArrow, AK.EVENTS.PLAY_CL_NO);

            if (EnableOtherVoiceBinds)
            {
                IfKeySay(KeyCode.P, AK.EVENTS.PLAY_CL_SORRY);
                IfKeySay(KeyCode.L, AK.EVENTS.PLAY_CL_HURRY);
                IfKeySay(KeyCode.J, AK.EVENTS.PLAY_CL_THREETWOONEGO);
                IfKeySay(KeyCode.K, AK.EVENTS.PLAY_CL_SYNCHRONIZE);
                IfKeySay(KeyCode.H, AK.EVENTS.PLAY_FALLDAMAGEGRUNT02_5A); // make local player say it :>
            }

        }

    }
}
