using TheArchive.Core;
using UnityEngine;

namespace TheArchive.Features.Special
{
    public class HideFirstPersonItem : Feature
    {
        public override string Name => "Weapon Model Toggle (F2)";

        public override string Group => FeatureGroups.Special;

        public void Update()
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
