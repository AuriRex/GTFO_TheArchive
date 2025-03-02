using Player;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Special;

/// <summary>
/// Play voice lines on a key press!
/// </summary>
[RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
public class MuteSpeak : Feature
{
    public override string Name => "Mute Speak";

    public override FeatureGroup Group => FeatureGroups.Special;

    public override string Description => "Binds a few voice lines to keyboard keys.\n\nArrow keys\n[P, L, K, J, H] toggleable by hitting F8; off by default\nHold [Right Control] for alternate lines";

    public static bool EnableOtherVoiceBinds { get; set; } = false;

    private static PlayerAgent _localPlayerAgent = null!;

    public static void IfKeySay(KeyCode key, uint soundId)
    {
        if (Input.GetKeyDown(key))
        {
            PlayerVoiceManager.WantToSay(_localPlayerAgent.CharacterID, soundId);
        }
    }

    public static void IfKeySay(KeyCode key, uint soundId, int characterID)
    {
        if (Input.GetKeyDown(key))
        {
            PlayerVoiceManager.WantToSay(characterID, soundId);
        }
    }

#if IL2CPP
    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            EnableOtherVoiceBinds = !EnableOtherVoiceBinds;
            ArchiveLogger.Notice($"Voice binds enabled: {EnableOtherVoiceBinds}");
        }

        if (PlayerChatManager.InChatMode) return;
        if (SharedUtils.LocalPlayerIsInTerminal) return;
        if (!PlayerManager.TryGetLocalPlayerAgent(out _localPlayerAgent) || _localPlayerAgent == null) return;

        if (Input.GetKey(KeyCode.RightControl))
        {
            // Very important when playing as Dauda!
            IfKeySay(KeyCode.RightShift, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_BIGSPACEENTER01)));

            IfKeySay(KeyCode.UpArrow, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_NORTH)));
            IfKeySay(KeyCode.RightArrow, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_EAST)));
            IfKeySay(KeyCode.DownArrow, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_SOUTH)));
            IfKeySay(KeyCode.LeftArrow, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_WEST)));

            if (EnableOtherVoiceBinds)
            {
                    
                IfKeySay(KeyCode.P, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_WELLDONE)));
                IfKeySay(KeyCode.L, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_SHH)));
                IfKeySay(KeyCode.K, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_WILLDO)));
                IfKeySay(KeyCode.J, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_YOUTAKE)));
                IfKeySay(KeyCode.H, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_FALLDAMAGEGRUNT02_5A)), 2); // Make Hackett say the line xp
            }

            return;
        }

        IfKeySay(KeyCode.LeftArrow, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_LEFT)));
        IfKeySay(KeyCode.RightArrow, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_RIGHT)));
        IfKeySay(KeyCode.UpArrow, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_YES)));
        IfKeySay(KeyCode.DownArrow, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_NO)));

        if (EnableOtherVoiceBinds)
        {
            IfKeySay(KeyCode.P, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_SORRY)));
            IfKeySay(KeyCode.L, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_HURRY)));
            IfKeySay(KeyCode.J, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_THREETWOONEGO)));
            IfKeySay(KeyCode.K, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_SYNCHRONIZE)));
            IfKeySay(KeyCode.H, SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_FALLDAMAGEGRUNT02_5A))); // make local player say it :>
        }

    }
#endif

}