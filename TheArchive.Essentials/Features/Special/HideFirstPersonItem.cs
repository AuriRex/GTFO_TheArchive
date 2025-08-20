using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using UnityEngine;

namespace TheArchive.Features.Special;

public class HideFirstPersonItem : Feature
{
    public override string Name => "Weapon Model Toggle";

    public override FeatureGroup Group => FeatureGroups.Special;

    public override string Description => "Forces the held item to be hidden.\nIntended for taking pictures.\n<color=orange>(Warning! This makes you unable to use or switch items until unhidden!)</color>";

    [FeatureConfig]
    public static HideFirstPersonItemSettings Settings { get; set; }
    
    public class HideFirstPersonItemSettings
    {
        [FSDisplayName("Model Toggle Key")]
        [FSDescription("Key used to toggle the model.")]
        public KeyCode Key { get; set; } = KeyCode.F2;
    }
    
    public override void Update()
    {
        if (FocusStateManager.CurrentState != eFocusState.FPS)
            return;
        
        if (!Input.GetKeyDown(Settings.Key))
            return;
        
        // Toggle First Person Item hidden
        var localPlayerAgent = Player.PlayerManager.GetLocalPlayerAgent();
        if (localPlayerAgent != null && localPlayerAgent.FPItemHolder != null)
            localPlayerAgent.FPItemHolder.ForceItemHidden = !localPlayerAgent.FPItemHolder.ForceItemHidden;
    }
}