using System.Reflection;
using TheArchive.Core.FeaturesAPI.Components;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class LabelSetting : FComponentSetting<FLabel>
    {
        public string LabelText => FComponent.LabelText;
        public string LabelID => FComponent.LabelID;

        public LabelSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
        {

        }
    }
}
