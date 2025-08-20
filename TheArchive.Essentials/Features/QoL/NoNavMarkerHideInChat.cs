using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;

namespace TheArchive.Features.QoL;

[EnableFeatureByDefault]
public class NoNavMarkerHideInChat : Feature
{
	public override string Name => "See NavMarkers in Chat";

	public override FeatureGroup Group => FeatureGroups.QualityOfLife;

	public override string Description => "Prevent enemy pings from hiding whenever the chat is open.";

	[ArchivePatch(typeof(GuiManager), nameof(GuiManager.OnFocusStateChanged))]
	internal static class GuiManager_OnFocusStateChanged_Patch
	{
		private static eFocusState _eFocusState_FPS_TypingInChat;

		public static void Init()
		{
			_eFocusState_FPS_TypingInChat = Utilities.Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.FPS_TypingInChat));
		}

		public static void Postfix(eFocusState state)
		{
			if(state == _eFocusState_FPS_TypingInChat)
			{
				GuiManager.NavMarkerLayer.SetVisible(true);
			}
		}
	}
}