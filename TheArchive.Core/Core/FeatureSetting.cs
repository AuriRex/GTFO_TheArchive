using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Core
{
    public class FeatureSetting
    {
        public FeatureSettingsHelper FeatureSettingsHelper { get; }
        public PropertyInfo Prop { get; }

        public FeatureSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop)
        {
            FeatureSettingsHelper = featureSettingsHelper;
            Prop = prop;
        }

    }
}
