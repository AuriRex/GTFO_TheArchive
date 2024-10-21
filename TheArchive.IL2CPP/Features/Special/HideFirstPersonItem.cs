using TheArchive.Core.FeaturesAPI;
using UnityEngine;

namespace TheArchive.Features.Special
{
    public class HideFirstPersonItem : Feature
    {
        public override string Name => "Weapon Model Toggle (F2)";

        public override FeatureGroup Group => FeatureGroups.Special;

        public override string Description => "Forces the held item to be hidden.\n(Warning! This makes you unable to use or switch items until unhidden!)";

        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                // Toggle First Person Item hidden
                var localPlayerAgent = Player.PlayerManager.GetLocalPlayerAgent();
                if (localPlayerAgent != null && localPlayerAgent.FPItemHolder != null)
                    localPlayerAgent.FPItemHolder.ForceItemHidden = !localPlayerAgent.FPItemHolder.ForceItemHidden;
            }
        }
    }
}
