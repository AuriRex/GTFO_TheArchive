using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;

namespace TheArchive.Features.LocalProgression
{
    [HideInModSettings]
    [DoNotSaveToConfig]
    [AutomatedFeature]
    internal class PlayFabManagerPatches : Feature
    {
        public override string Name => nameof(PlayFabManagerPatches);

        public override string Group => FeatureGroups.LocalProgression;
    }
}
