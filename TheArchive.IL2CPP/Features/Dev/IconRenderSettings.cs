using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using static GearIconRendering;

namespace TheArchive.Features.Dev
{
    [HideInModSettings]
    public class IconRenderSettings : Feature
    {
        public override string Name => nameof(IconRenderSettings);

        public override FeatureGroup Group => FeatureGroups.Dev;

        [FeatureConfig]
        public static IconRenderSettingsSettings Settings { get; set; }

        public class IconRenderSettingsSettings
        {
            public int ResolutionMultiplier { get; set; } = 5;
        }

        [ArchivePatch(typeof(GearIconRendering), "RenderIconPrep")]
        internal static class GearIconRendering_RenderIconPrepPatch
        {
            public static void Prefix(ref IconRenderJob job)
            {
                var settings = job.datas[job.currentIconIndex].settings;

                settings.resX = settings.resX * Settings.ResolutionMultiplier;
                settings.resY = settings.resY * Settings.ResolutionMultiplier;
            }
        }
    }
}
