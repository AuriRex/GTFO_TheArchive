using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class GenericListSetting : FeatureSetting
    {
        public Type ListType { get; }

        public GenericListSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
        {
            ListType = Type.GenericTypeArguments[0];
        }

        public IList GetList()
        {
            return GetValue() as IList;
        }

        public void RemoveEntry(object entry)
        {
            GetList().Remove(entry);
            Helper.IsDirty = true;
        }

        public void AddEntry(object entry)
        {
            if (!ListType.IsAssignableFrom(entry.GetType())) return;
            GetList().Add(entry);
            Helper.IsDirty = true;
        }

        public IEnumerable<ListEntry> GetEntries()
        {
            var list = new List<ListEntry>();
            foreach(var obj in GetList())
            {
                list.Add(new ListEntry(this, ListType, obj));
            }

            return list;
        }

        public class ListEntry
        {
            public GenericListSetting Parent { get; private set; }
            public Type EntryType { get; private set; }
            public object Instance { get; private set; }
            public DynamicFeatureSettingsHelper Helper { get; private set; }

            public ListEntry(GenericListSetting gls, Type entryType, object instance)
            {
                Parent = gls;
                EntryType = entryType;
                Instance = instance;

                Helper = new DynamicFeatureSettingsHelper(Parent.Helper.Feature, Parent.Helper).Initialize(entryType, instance);
            }

            public void RemoveFromList()
            {
                Parent.RemoveEntry(Instance);
            }
        }
    }
}
