using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting for handling generic lists.
/// </summary>
public class GenericListSetting : FeatureSetting
{
    /// <summary>
    /// The generic list type.
    /// </summary>
    public Type ListType { get; }

    /// <inheritdoc/>
    public GenericListSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "") : base(featureSettingsHelper, prop, instance, debugPath)
    {
        ListType = Type.GenericTypeArguments[0];
    }

    /// <summary>
    /// Get the list instance.
    /// </summary>
    /// <returns>The list instance.</returns>
    public IList GetList()
    {
        return GetValue() as IList;
    }

    /// <summary>
    /// Remove an entry from the list.
    /// </summary>
    /// <param name="entry">The entry to remove.</param>
    public void RemoveEntry(object entry)
    {
        GetList().Remove(entry);
        Helper.IsDirty = true;
    }

    /// <summary>
    /// Add an entry to the list.
    /// </summary>
    /// <param name="entry">The entry to add.</param>
    public void AddEntry(object entry)
    {
        if (!ListType.IsAssignableFrom(entry.GetType())) return;
        GetList().Add(entry);
        Helper.IsDirty = true;
    }

    /// <summary>
    /// Get all list entries.
    /// </summary>
    /// <returns><c>IEnumerable&lt;ListEntry&gt;</c></returns>
    public IEnumerable<ListEntry> GetEntries()
    {
        var list = new List<ListEntry>();
        foreach(var obj in GetList())
        {
            list.Add(new ListEntry(this, ListType, obj));
        }

        return list;
    }

    /// <summary>
    /// A single list entry.
    /// </summary>
    public class ListEntry
    {
        /// <summary>
        /// The parent list setting.
        /// </summary>
        public GenericListSetting Parent { get; }
        
        /// <summary>
        /// The type of this entry.
        /// </summary>
        public Type EntryType { get; private set; }
        
        /// <summary>
        /// The instance of this entry.
        /// </summary>
        public object Instance { get; }
        
        /// <summary>
        /// The settings helper responsible for populating any submenus.
        /// </summary>
        public DynamicFeatureSettingsHelper Helper { get; private set; }

        /// <summary>
        /// List entry constructor.
        /// </summary>
        /// <param name="gls">The parent <c>GenericListSetting</c>.</param>
        /// <param name="entryType">The type of this entry.</param>
        /// <param name="instance">The instance of this entry.</param>
        public ListEntry(GenericListSetting gls, Type entryType, object instance)
        {
            Parent = gls;
            EntryType = entryType;
            Instance = instance;

            Helper = new DynamicFeatureSettingsHelper(Parent.Helper.Feature, Parent.Helper).Initialize(entryType, instance);
        }

        /// <summary>
        /// Remove this entry from its parent list.
        /// </summary>
        public void RemoveFromList()
        {
            Parent.RemoveEntry(Instance);
        }
    }
}