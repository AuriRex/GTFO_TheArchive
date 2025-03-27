using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Components;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Features.QoL;

public class ReloadSoundCue : Feature
{
    public override string Name => "Reload Sound Cue";

    public override FeatureGroup Group => FeatureGroups.QualityOfLife;

    public override string Description => "Play a sound cue on reload the moment the bullets have entered your gun.";

    public new static IArchiveLogger FeatureLogger { get; set; }

    [FeatureConfig]
    public static ReloadSoundCueSettings Settings { get; set; }

    public class ReloadSoundCueSettings
    {
        [FSDisplayName("Test Sound Event")]
        [FSDescription("Plays the sound event below,\nit's a little janky and might not work depending on the sound, sorry!")]
        public FButton TestSoundButton { get; set; } = new FButton("Play Sound");

        [FSDisplayName("Sound Event")]
        [FSDescription("The sound event to play whenever the reload has happened.")]
        public string SoundEvent { get; set; } = nameof(AK.EVENTS.HACKING_PUZZLE_CORRECT);
            
        [FSDisplayName("Print Sound Events To Console")]
        [FSDescription("Prints all available sound events to the <b><#F00>console</color></b>.\n<color=orange>Warning! This might freeze your game for a few seconds!</color>\n\nSome events might not work because their playback conditions are not met!")]
        public FButton PrintSoundEventsButton { get; set; } = new FButton("Print To Console");
    }

    private static bool _hasPrintedEvents = false;
    public override void OnButtonPressed(ButtonSetting setting)
    {
        if(setting.ButtonID == nameof(ReloadSoundCueSettings.TestSoundButton))
        {
            PlaySound();
        }

        if (setting.ButtonID == nameof(ReloadSoundCueSettings.PrintSoundEventsButton))
        {
            if(!_hasPrintedEvents)
            {
                _hasPrintedEvents = true;
                SoundEventCache.DebugLog(FeatureLogger);
            }
            else
            {
                FeatureLogger.Info("Sound events have already been printed once, skipping! :)");
            }
        }
    }

    private static void PlaySound()
    {
        var localPlayer = Player.PlayerManager.GetLocalPlayerAgent();
        if (localPlayer != null && localPlayer.Sound != null)
        {
            if (SoundEventCache.TryResolve(Settings.SoundEvent, out var soundId))
            {
                localPlayer.Sound.SafePost(soundId);
            }
            else
            {
                localPlayer.Sound.SafePost(SoundEventCache.Resolve(nameof(AK.EVENTS.HACKING_PUZZLE_CORRECT)));
            }
        }
    }

    [ArchivePatch(typeof(PlayerInventoryLocal), nameof(PlayerInventoryLocal.DoReload))]
    internal static class PlayerInventoryLocal_DoReload_Patch
    {
        public static void Postfix()
        {
            PlaySound();
        }
    }
}