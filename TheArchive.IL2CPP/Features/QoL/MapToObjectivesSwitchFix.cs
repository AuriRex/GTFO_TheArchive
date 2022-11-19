using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.QoL
{
    [EnableFeatureByDefault]
    [RundownConstraint(Utils.RundownFlags.RundownFour, Utils.RundownFlags.Latest)]
    public class MapToObjectivesSwitchFix : Feature
    {
        public override string Name => "Map Chat Abduction Fix";

        public override string Group => FeatureGroups.QualityOfLife;

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
                    return ArchivePatch.SKIP_OG;
                }

                return ArchivePatch.RUN_OG;
            }
        }
    }
}
