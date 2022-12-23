using System.Reflection;
using TheArchive.Core.FeaturesAPI.Components;
using TheArchive.Utilities;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class ButtonSetting : NotSavedFeatureSetting
    {
        public FButton FButton { get; private set; }

        public string ButtonText { get; private set; }
        public string ButtonID { get; private set; }

        public ButtonSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
        {
            if(prop.SetMethod != null)
            {
                ArchiveLogger.Error($"Don't define a set method for {nameof(Components.FButton)} ({debug_path})");
                return;
            }

            FButton = (FButton) prop.GetValue(instance);

            ButtonText = FButton.ButtonText;
            ButtonID = FButton.ButtonID ?? prop.Name;
        }
    }
}
