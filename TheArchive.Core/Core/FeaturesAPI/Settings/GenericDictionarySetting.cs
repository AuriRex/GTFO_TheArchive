using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Core.Localization;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting for handling generic dictionaries.
/// </summary>
public class GenericDictionarySetting : FeatureSetting
{
    /// <summary>
    /// The type of the generic dictionary key.
    /// </summary>
    public Type DictKeyType { get; }
    /// <summary>
    /// The type of the generic dictionary value.
    /// </summary>
    public Type DictValueType { get; }

    internal BaseLocalizationService Localization { get; }

    /// <inheritdoc/>
    public GenericDictionarySetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "") : base(featureSettingsHelper, prop, instance, debugPath)
    {
        Localization = featureSettingsHelper.Localization;
        DictKeyType = Type.GenericTypeArguments[0];
        DictValueType = Type.GenericTypeArguments[1];
    }

    /// <summary>
    /// Get the dictionary instance.
    /// </summary>
    /// <returns>The dictionary instance.</returns>
    public IDictionary GetDict()
    {
        return GetValue() as IDictionary;
    }

    /// <summary>
    /// Remove an entry from the dictionary.
    /// </summary>
    /// <param name="key">The key of the entry to remove.</param>
    public void RemoveEntry(object key)
    {
        GetDict().Remove(key);
        Helper.IsDirty = true;
    }

    /// <summary>
    /// Add a new entry into the dictionary.
    /// </summary>
    /// <param name="key">The key of the new entry.</param>
    /// <param name="entry">The value.</param>
    public void AddEntry(object key, object entry)
    {
        if (!DictKeyType.IsAssignableFrom(key.GetType())) return;
        GetDict().Add(key, entry);
        Helper.IsDirty = true;
    }

    /// <summary>
    /// Get all dictionary entries.
    /// </summary>
    /// <returns><c>IEnumerable&lt;DictEntry&gt;</c></returns>
    public IEnumerable<DictEntry> GetEntries()
    {
        var list = new List<DictEntry>();

        foreach(DictionaryEntry obj in GetDict())
        {
            list.Add(new DictEntry(this, DictKeyType, DictValueType, obj.Key, obj.Value));
        }

        return list;
    }

    /// <summary>
    /// A single dictionary entry.
    /// </summary>
    public class DictEntry
    {
        /// <summary>
        /// The parent dictionary setting.
        /// </summary>
        public GenericDictionarySetting Parent { get; }
        
        /// <summary>
        /// The dictionary key type.
        /// </summary>
        public Type KeyType { get; }
        
        /// <summary>
        /// The dictionary value type.
        /// </summary>
        public Type EntryType { get; }
        
        /// <summary>
        /// The localized name of the key (if present)
        /// </summary>
        public string KeyName { get; }
        
        /// <summary>
        /// The key instance.
        /// </summary>
        public object Key { get; }
        
        /// <summary>
        /// The value instance.
        /// </summary>
        public object Value { get; }
        
        /// <summary>
        /// The settings helper responsible for populating any submenus.
        /// </summary>
        public DynamicFeatureSettingsHelper Helper { get; private set; }

        /// <summary>
        /// Dictionary entry constructor.
        /// </summary>
        /// <param name="gds">The parent <c>GenericDictionarySetting</c>.</param>
        /// <param name="keyType">The dictionary key type.</param>
        /// <param name="entryType">The dictionary value type.</param>
        /// <param name="key">The key instance.</param>
        /// <param name="instance">The value instance.</param>
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

        /// <summary>
        /// Remove this entry from its parent dictionary.
        /// </summary>
        public void RemoveFromList()
        {
            Parent.RemoveEntry(Key);
        }
    }
}