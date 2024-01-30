﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Core.Localization;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class GenericDictionarySetting : FeatureSetting
    {
        public Type DictKeyType { get; }
        public Type DictValueType { get; }

        internal FeatureLocalizationService Localization { get; }

        public GenericDictionarySetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
        {
            Localization = featureSettingsHelper.Localization;
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
            Helper.IsDirty = true;
        }

        public void AddEntry(object key, object entry)
        {
            if (!DictKeyType.IsAssignableFrom(key.GetType())) return;
            GetDict().Add(key, entry);
            Helper.IsDirty = true;
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
            public string KeyName { get; private set; }
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
                KeyName = gds.Localization.Get(KeyType, Key);

                Helper = new DynamicFeatureSettingsHelper(Parent.Helper.Feature, Parent.Helper).Initialize(entryType, instance);
            }

            public void RemoveFromList()
            {
                Parent.RemoveEntry(Key);
            }
        }
    }
}
