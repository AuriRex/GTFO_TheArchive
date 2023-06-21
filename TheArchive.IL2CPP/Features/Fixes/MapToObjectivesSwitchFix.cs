using Player;
using System.Collections;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Loader;
using TheArchive.Utilities;

namespace TheArchive.Features.Fixes
{
    [EnableFeatureByDefault]
    [RundownConstraint(Utils.RundownFlags.RundownFour, Utils.RundownFlags.Latest)]
    public class MapToObjectivesSwitchFix : Feature
    {
        public override string Name => "Map Chat Abduction Fix";

        public override string Group => FeatureGroups.Fixes;

        public override string Description => "Prevent a switch to the Objectives Screen whenever the chat is open and the 'o' key is pressed.";

        // Prevent a switch to the Objectives Screen whenever the chat is open
        [ArchivePatch(typeof(MainMenuGuiLayer), nameof(MainMenuGuiLayer.ChangePage))]
        internal static class MainMenuGuiLayer_ChangePagePatch
        {
#if IL2CPP
            public static readonly eCM_MenuPage eCM_MenuPage_CMP_OBJECTIVES = Utils.GetEnumFromName<eCM_MenuPage>(nameof(eCM_MenuPage.CMP_OBJECTIVES));
#else
            public static readonly eCM_MenuPage eCM_MenuPage_CMP_OBJECTIVES = Utils.GetEnumFromName<eCM_MenuPage>("CMP_OBJECTIVES");
#endif

            public static bool Prefix(eCM_MenuPage pageEnum)
            {
                if (pageEnum == eCM_MenuPage_CMP_OBJECTIVES && PlayerChatManager.InChatMode)
                {
                    LoaderWrapper.StartCoroutine(CancelSoundButJank());
                    return ArchivePatch.SKIP_OG;
                }

                return ArchivePatch.RUN_OG;
            }

            // Doesn't work perfectly on 60 FPS but whatever
            private static IEnumerator CancelSoundButJank()
            {
                yield return null;
                if (PlayerManager.TryGetLocalPlayerAgent(out var localPlayer))
                {
                    localPlayer.Sound?.Stop();
                }
            }
        }
    }
}
