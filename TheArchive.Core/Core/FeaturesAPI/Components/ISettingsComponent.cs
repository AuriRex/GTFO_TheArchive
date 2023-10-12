using UnityEngine;

namespace TheArchive.Core.FeaturesAPI.Components
{
    public interface ISettingsComponent
    {
        public bool HasPrimaryText { get; }
#warning TODO: Change those two to TextMeshPro after we switch to multiple Core builds
        public MonoBehaviour PrimaryText { get; set; }
        public bool HasSecondaryText { get; }
        public MonoBehaviour SecondaryText { get; set; }
    }
}
