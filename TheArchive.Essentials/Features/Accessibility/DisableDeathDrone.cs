using AK;
using Player;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Utilities;
using static Player.PlayerLocomotion;

namespace TheArchive.Features.Accessibility;

public class DisableDeathDrone : Feature
{
    public override string Name => "Disable Downed Sound";

    public override GroupBase Group => GroupManager.Accessibility;

    public override string Description => "Removes the droning sound that playes whenever you're downed.";

    public override bool SkipInitialOnEnable => true;

    private static PLOC_State _PLOC_State_Downed = Utils.GetEnumFromName<PLOC_State>(nameof(PLOC_State.Downed));

    public override void OnEnable()
    {
        var localPlayer = PlayerManager.GetLocalPlayerAgent();

        if (!IsPlayerDowned(localPlayer))
            return;

        var stateDowned = localPlayer.Locomotion.CurrentState.TryCastTo<PLOC_Downed>();

        SetDownedSoundEnabled(stateDowned, false);
    }

    public override void OnDisable()
    {
        if (IsApplicationQuitting)
            return;

        var localPlayer = PlayerManager.GetLocalPlayerAgent();

        if (!IsPlayerDowned(localPlayer))
            return;

        var stateDowned = localPlayer.Locomotion.CurrentState.TryCastTo<PLOC_Downed>();

        SetDownedSoundEnabled(stateDowned, true);
    }

    private static uint _ID_PLAYER_DOWNED_1ST_PERSON;
    private static uint _ID_PLAYER_REVIVED_1ST_PERSON;

    public override void OnDatablocksReady()
    {
        SoundEventCache.TryResolve(nameof(EVENTS.PLAYER_DOWNED_1ST_PERSON), out _ID_PLAYER_DOWNED_1ST_PERSON);
        SoundEventCache.TryResolve(nameof(EVENTS.PLAYER_REVIVED_1ST_PERSON), out _ID_PLAYER_REVIVED_1ST_PERSON);
    }

    public static bool IsPlayerDowned(PlayerAgent player)
    {
        if (player == null)
            return false;

        if (player.Locomotion?.m_currentStateEnum != _PLOC_State_Downed)
            return false;

        return true;
    }

    public static void SetDownedSoundEnabled(PLOC_Downed downedState, bool value)
    {
        downedState?.m_owner?.Sound?.SafePost(value ? _ID_PLAYER_DOWNED_1ST_PERSON : _ID_PLAYER_REVIVED_1ST_PERSON, true);
    }

    [ArchivePatch(typeof(PLOC_Downed), nameof(PLOC_Downed.Enter))]
    internal static class PLOC_Downed_Enter_Patch
    {
        public static void Postfix(PLOC_Downed __instance)
        {
            SetDownedSoundEnabled(__instance, false);
        }
    }
}