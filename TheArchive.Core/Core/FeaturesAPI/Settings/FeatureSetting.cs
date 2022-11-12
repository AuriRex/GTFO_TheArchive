using System;
using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class FeatureSetting
    {
        public FeatureSettingsHelper Helper { get; }
        public PropertyInfo Prop { get; }
        public RundownFlags RundownHint { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string Identifier { get; }
        public bool Readonly { get; }
        public bool TopLevelReadonly { get; }
        public Type Type { get; }
        public string DEBUG_Path { get; }

        public bool SeparatorAbove { get; private set; }
        public bool SpacerAbove { get; private set; }
        public FSHeader HeaderAbove { get; private set; }
        public bool HideInModSettings { get; private set; }

        public object WrappedInstance { get; set; }

        public FeatureSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "")
        {
            Helper = featureSettingsHelper;
            Prop = prop;
            Type = prop?.GetMethod?.ReturnType;
            DisplayName = $"> {prop?.GetCustomAttribute<FSDisplayName>()?.DisplayName ?? prop.Name}";
            Identifier = prop?.GetCustomAttribute<FSIdentifier>()?.Identifier ?? ($"{prop.PropertyType.FullName}_{prop.Name}");
            RundownHint = prop?.GetCustomAttribute<FSRundownHint>()?.Rundowns ?? RundownFlags.None;
            Description = prop?.GetCustomAttribute<FSDescription>()?.Description;

            SeparatorAbove = prop?.GetCustomAttribute<FSSeparator>() != null;
            SpacerAbove = prop?.GetCustomAttribute<FSSpacer>() != null;
            HeaderAbove = prop?.GetCustomAttribute<FSHeader>();

            HideInModSettings = prop?.GetCustomAttribute<FSHide>() != null;
            Readonly = prop?.GetCustomAttribute<FSReadOnly>()?.RecursiveReadOnly ?? false;
            TopLevelReadonly = !(prop?.GetCustomAttribute<FSReadOnly>()?.RecursiveReadOnly ?? true);

            WrappedInstance = instance;
            DEBUG_Path = debug_path;
        }

        public virtual object SetValue(object value)
        {
            Prop.SetValue(WrappedInstance, value);
            FeatureManager.Instance.OnFeatureSettingChanged(this);
            return value;
        }

        public virtual object GetValue()
        {
            return Prop.GetValue(WrappedInstance, Array.Empty<object>());
        }
    }
}
