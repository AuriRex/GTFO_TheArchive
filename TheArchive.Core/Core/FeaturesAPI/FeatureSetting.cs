using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Core.FeaturesAPI
{
    public class FeatureSetting
    {
        public FeatureSettingsHelper FeatureSettingsHelper { get; }
        public PropertyInfo Prop { get; }
        public string DEBUG_Path { get; }

        private readonly object _instance;

        public FeatureSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "")
        {
            FeatureSettingsHelper = featureSettingsHelper;
            Prop = prop;
            _instance = instance;
            DEBUG_Path = debug_path;
        }


        public virtual void SetValue(object value)
        {
            Prop.SetValue(_instance, value);
        }

        public virtual object GetValue()
        {
            return Prop.GetValue(_instance);
        }
    }
}
