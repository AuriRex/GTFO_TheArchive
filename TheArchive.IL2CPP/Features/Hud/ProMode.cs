using System.Collections.Generic;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;

namespace TheArchive.Features.Hud
{
    public class ProMode : Feature
    {
        public override string Name => "Hud Pro Mode (Disable HUD)";

        public override string Group => FeatureGroups.Hud;

        public override string Description => "Force disable <i><u>ALL</u></i> HUD layers.\n\nMain purpose is for video production";
        

        [ArchivePatch(typeof(GuiManager), nameof(GuiManager.OnFocusStateChanged))]
        internal static class GuiManager_OnFocusStateChanged_Patch
        {
            public static bool Prefix(eFocusState state)
            {
                switch(state)
                {
                    case eFocusState.MainMenu:
                    case eFocusState.Map:
                        return ArchivePatch.RUN_OG;
                }

                foreach(var layer in GetAllLayers())
                {
                    if (layer.IsVisible())
                        layer.SetVisible(false);
                }
                return ArchivePatch.SKIP_OG;
            }
        }

        public static IEnumerable<GuiLayer> GetAllLayers()
        {
#if IL2CPP
            return GuiManager.GetAllLayers();
#else
            return new GuiLayer[] {
                GuiManager.PlayerLayer,
                GuiManager.DebugLayer,
                GuiManager.InteractionLayer,
                GuiManager.NavMarkerLayer,
                GuiManager.WatermarkLayer,
                GuiManager.MainMenuLayer,
                GuiManager.CrosshairLayer,
                GuiManager.ConsoleLayer,
                GuiManager.InGameMenuLayer,
                //GuiManager.GlobalPopupMessageLayer
            };
#endif

        }
    }
}
