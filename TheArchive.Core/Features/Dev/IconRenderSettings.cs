using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using static GearIconRendering;

namespace TheArchive.Features.Dev;

[HideInModSettings]
internal class IconRenderSettings : Feature
{
    public override string Name => nameof(IconRenderSettings);

    public override GroupBase Group => GroupManager.Dev;

    [FeatureConfig]
    public static IconRenderSettingsSettings Settings { get; set; }

    public class IconRenderSettingsSettings
    {
        [FSDisplayName("Resolution Multiplier")]
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