using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Utilities;

namespace TheArchive.Features.Dev;

[EnableFeatureByDefault, HideInModSettings]
[RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.Latest)]
internal class StartupscreenOverride : Feature
{
    public override string Name => nameof(StartupscreenOverride);

    public override GroupBase Group => GroupManager.Dev;

    [ArchivePatch(typeof(PlayFabManager), "TryGetStartupScreenData")]
    internal class PlayFabManager__TryGetStartupScreenData__Patch
    {
        public static bool Prefix(eStartupScreenKey key, out StartupScreenData data, ref bool __result)
        {
            var startupScreenData = new StartupScreenData();
            startupScreenData.AllowedToStartGame = true;

#pragma warning disable CS0618 // Type or member is obsolete
            startupScreenData.IntroText = Utils.GetStartupTextForRundown(BuildInfo.Rundown);
#pragma warning restore CS0618 // Type or member is obsolete
            startupScreenData.ShowDiscordButton = false;
            startupScreenData.ShowBugReportButton = false;
            startupScreenData.ShowRoadmapButton = false;
            startupScreenData.ShowIntroText = true;

            __result = true;
            data = startupScreenData;
            return ArchivePatch.SKIP_OG;
        }
    }
}