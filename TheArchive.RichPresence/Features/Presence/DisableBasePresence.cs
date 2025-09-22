using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Presence;

[RundownConstraint(RundownFlags.RundownEight, RundownFlags.Latest)]
public class DisableBasePresence : Feature
{
    public override string Name => "Disable Built-in Rich Presence";

    public override GroupBase Group => GroupManager.Presence;

    public override string Description => "Disables the Discord Rich Presence added to the game in Rundown 8.";

#if IL2CPP
    public override void OnEnable()
    {
        if (!DataBlocksReady)
            return;

        DisableBaseDiscord();
    }

    private bool _firstTime = true;
    public void OnGameStateChanged(eGameStateName state)
    {
        if(state == eGameStateName.NoLobby && _firstTime)
        {
            DisableBaseDiscord();
            _firstTime = false;
        }
    }

    public override void OnDisable()
    {
        if (IsApplicationQuitting)
            return;

        if (!DataBlocksReady)
            return;

        EnableBaseDiscord();
    }

    public static void DisableBaseDiscord()
    {
        DiscordManager.Current.ToggleDiscord(false);
    }

    public static void EnableBaseDiscord()
    {
        DiscordManager.Current.ToggleDiscord(true);
    }
#endif
}