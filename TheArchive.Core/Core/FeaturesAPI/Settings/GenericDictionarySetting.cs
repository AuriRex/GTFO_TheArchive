using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class GenericDictionarySetting : FeatureSetting
    {
        public Type DictKeyType { get; }
        public Type DictValueType { get; }

        public GenericDictionarySetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
        {
            DictKeyType = Type.GenericTypeArguments[0];
            DictValueType = Type.GenericTypeArguments[1];
        }

        public IDictionary GetDict()
        {
            return GetValue() as IDictionary;
        }

        public void RemoveEntry(object key)
        {
            GetDict().Remove(key);
        }

        public void AddEntry(object key, object entry)
        {
            if (!DictKeyType.IsAssignableFrom(key.GetType())) return;
            GetDict().Add(key, entry);
        }

        public IEnumerable<DictEntry> GetEntries()
        {
            var list = new List<DictEntry>();

            foreach(DictionaryEntry obj in GetDict())
            {
                list.Add(new DictEntry(this, DictKeyType, DictValueType, obj.Key, obj.Value));
            }

            return list;
        }

        public class DictEntry
        {
            public GenericDictionarySetting Parent { get; private set; }
            public Type KeyType { get; private set; }
            public Type EntryType { get; private set; }
            public object Key { get; private set; }
            public object Value { get; private set; }
            public DynamicFeatureSettingsHelper Helper { get; private set; }

            public DictEntry(GenericDictionarySetting gds, Type keyType, Type entryType, object key, object instance)
            {
                Parent = gds;
                KeyType = keyType;
                EntryType = entryType;
                Key = key;
                Value = instance;

                Helper = new DynamicFeatureSettingsHelper(Parent.Helper.Feature).Initialize(entryType, instance);
            }

            public void RemoveFromList()
            {
                Parent.RemoveEntry(Key);
            }
        }
    }
}
