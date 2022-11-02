using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class NumberSetting : FeatureSetting
    {
        public NumberSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
        {

        }
    }
}
