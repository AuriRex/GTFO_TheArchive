using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.Dev;

[EnableFeatureByDefault, HideInModSettings]
[RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.Latest)]
internal class StartupscreenOverride : Feature
{
    public override string Name => nameof(StartupscreenOverride);

    public override FeatureGroup Group => FeatureGroups.Dev;

    [ArchivePatch(typeof(PlayFabManager), "TryGetStartupScreenData")]
    internal class PlayFabManager_TryGetStartupScreenData_Patch
    {
        public static bool Prefix(eStartupScreenKey key, out StartupScreenData data, ref bool __result)
        {
            var startupScreenData = new StartupScreenData();
            startupScreenData.AllowedToStartGame = true;

            startupScreenData.IntroText = Utils.GetStartupTextForRundown(BuildInfo.Rundown);
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