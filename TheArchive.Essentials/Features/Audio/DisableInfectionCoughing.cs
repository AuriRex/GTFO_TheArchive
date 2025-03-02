using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Features.Dev;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Accessibility;

[RundownConstraint(RundownFlags.RundownTwo, RundownFlags.Latest)]
internal class DisableInfectionCoughing : Feature
{
    public override string Name => "Disable Coughing (<#1F835F>Infection</color>)";

    public override FeatureGroup Group => FeatureGroups.Audio;

    public override string Description => $"Disables the cough sound effect whenever a player looses HP due to infection.\n\n<size=70%>(SoundEvent=\"{nameof(AK.EVENTS.PLAY_COUGHSOFT01)}\")</size>";

    public override bool SkipInitialOnEnable => true;

    public override void OnEnable()
    {
        FilterCough();
    }

    public override void OnGameDataInitialized()
    {
        if(Enabled)
            FilterCough();
    }

    public void FilterCough()
    {
        PlayerDialogFilter.AddEventToFilter(nameof(AK.EVENTS.PLAY_COUGHSOFT01));
    }

    public override void OnDisable()
    {
        PlayerDialogFilter.RemoveEventFromFilter(nameof(AK.EVENTS.PLAY_COUGHSOFT01));
    }
}