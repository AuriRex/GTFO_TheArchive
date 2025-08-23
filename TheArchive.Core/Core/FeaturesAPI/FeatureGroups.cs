using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.Localization;

namespace TheArchive.Core.FeaturesAPI;

/// <summary>
/// Feature groupings
/// </summary>
public static class FeatureGroups
{
    /// <summary>
    /// Gets an existing or creates a new top level feature group.
    /// </summary>
    /// <param name="identifier">The identifier of the group.</param>
    /// <param name="localizationData">Optional group localization. (Usually provided via localization files!)</param>
    /// <returns>The existing or newly created group.</returns>
    public static FeatureGroup GetOrCreateTopLevelGroup(string identifier, GroupLocalization localizationData = null) =>
        GetOrCreateTopLevelGroupInternal(identifier, dev: false, localizationData);

    private static FeatureGroup GetOrCreateTopLevelGroupInternal(string identifier, bool dev = false, GroupLocalization localizationData = null)
    {
        var group = TopLevelGroups.FirstOrDefault(g => g.Identifier == identifier, null);

        if (group != null)
            group.IsNewlyCreated = false;

        group ??= new FeatureGroup(identifier, false, null, dev);

        if (localizationData != null)
            group.SetLanguage(localizationData);
        
        TopLevelGroups.Add(group);

        return group;
    }

    /// <summary>
    /// Gets an existing or creates a new module/add-on feature group.
    /// </summary>
    /// <param name="identifier">The identifier of the group.</param>
    /// <param name="localizationData">Optional group localization. (Usually provided via localization files!)</param>
    /// <returns>The existing or newly created group.</returns>
    public static FeatureGroup GetOrCreateModuleGroup(string identifier, GroupLocalization localizationData = null)
    {
        var group = AddonGroups.FirstOrDefault(g => g.Identifier == identifier, null);

        if (group != null)
            group.IsNewlyCreated = false;

        group ??= new FeatureGroup(identifier, true, null);

        if (localizationData != null)
            group.SetLanguage(localizationData);

        return group;
    }

    internal static FeatureGroup GetGroup(string identifier) => AllGroups.FirstOrDefault(g => g.Identifier == identifier);
    internal static FeatureGroup GetModuleGroup(string identifier) => AddonGroups.FirstOrDefault(g => g.Identifier == identifier);
    internal static HashSet<FeatureGroup> TopLevelGroups { get; private set; } = new();
    internal static HashSet<FeatureGroup> AddonGroups { get; private set; } = new();
    internal static HashSet<FeatureGroup> AllGroups { get; private set; } = new();

    #region Archive Groups
    internal static FeatureGroup ArchiveCore { get; private set; } = GetOrCreateTopLevelGroupInternal(ArchiveMod.ARCHIVE_CORE_FEATUREGROUP);
    /// <summary> A feature group for accessibility and ease-of-use features. </summary>
    public static FeatureGroup Accessibility { get; private set; } = GetOrCreateTopLevelGroupInternal("Core.Accessibility");
    /// <summary> A feature group for audio related features. </summary>
    public static FeatureGroup Audio { get; private set; } = GetOrCreateTopLevelGroupInternal("Core.Audio");
    /// <summary> A feature group for cosmetic features. </summary>
    public static FeatureGroup Cosmetic { get; private set; } = GetOrCreateTopLevelGroupInternal("Core.Cosmetic");
    /// <summary> A feature group for developer tools and similar. </summary>
    /// <remarks> This group is hidden by default! </remarks>
    public static FeatureGroup Dev { get; private set; } = GetOrCreateTopLevelGroupInternal("Core.Developer", true);
    /// <summary> A feature group for features that fix things. </summary>
    public static FeatureGroup Fixes { get; private set; } = GetOrCreateTopLevelGroupInternal("Core.Fixes");
    /// <summary> A feature group for HUD altering features. </summary>
    public static FeatureGroup Hud { get; private set; } = GetOrCreateTopLevelGroupInternal("Core.HUD");
    /// <summary>
    /// A feature group for features related to local progression functionality.<br/>
    /// <i>Mostly unused now.</i>
    /// </summary>
    internal static FeatureGroup LocalProgression { get; private set; } = GetOrCreateTopLevelGroupInternal("Core.LocalProgression"); // , group => group.InlineSettings = true
    /// <summary> A feature group for any special or miscellaneous features that don't fit anywhere else. </summary>
    public static FeatureGroup Special { get; private set; } = GetOrCreateTopLevelGroupInternal("Core.Misc");
    /// <summary> A feature group for steam/discord rich presence related features. </summary>
    public static FeatureGroup Presence { get; private set; } = GetOrCreateTopLevelGroupInternal("Core.Presence");
    /// <summary> A feature group for security related features. </summary>
    public static FeatureGroup Security { get; private set; } = GetOrCreateTopLevelGroupInternal("Core.Security");
    /// <summary> A feature group for QoL features. </summary>
    public static FeatureGroup QualityOfLife { get; private set; } = GetOrCreateTopLevelGroupInternal("Core.QoL");
    #endregion
}

/// <summary>
/// A feature group.
/// </summary>
public class FeatureGroup
{
    internal string DisplayName =>
        _localization?.GetLocalizedDisplayName(LocalizationCoreService.CurrentLanguage, Identifier) ?? Identifier;

    internal string Description =>
        _localization?.GetLocalizedDescription(LocalizationCoreService.CurrentLanguage, string.Empty) ?? string.Empty;
    
    /// <summary> The name of this feature group. </summary>
    public string Identifier { get; private set; } = string.Empty;
    /// <summary> If this feature group has been newly created. </summary>
    public bool IsNewlyCreated { get; internal set; } = true;
    /// <summary> If this feature group is a default module group. </summary>
    public bool IsModuleGroup { get; internal set; }
    /// <summary> Should this feature group be hidden in mod settings? </summary>
    public bool IsHidden { get; private set; }
    /// <summary> The parent group if this is a subgroup. </summary>
    public FeatureGroup ParentGroup { get; }
    /// <summary> Set of subgroups. </summary>
    public HashSet<FeatureGroup> SubGroups { get; } = new();
    internal HashSet<Feature> Features { get; } = new();
    
    private GroupLocalization _localization;

    /// <summary>
    /// Gets or creates a new subgroup inside of this feature group.
    /// </summary>
    /// <param name="name">The subgroup name.</param>
    /// <param name="isHidden">Should the group be hidden (only visible in dev mode)?</param>
    /// <returns>The already existing subgroup with the provided name or a newly created one if no existing one was found.</returns>
    public FeatureGroup GetOrCreateSubGroup(string name, bool isHidden = false)
    {
        var subGroup = SubGroups.FirstOrDefault(g => g.Identifier == name);

        if (subGroup != null)
            IsNewlyCreated = false;

        subGroup ??= new FeatureGroup(name, false, this, isHidden);

        return subGroup;
    }

    /// <summary>
    /// Set the groups language translation values using a translation dictionary.
    /// </summary>
    /// <param name="localizationData">Localization data.</param>
    public void SetLanguage(GroupLocalization localizationData)
    {
        _localization = localizationData;
    }

    internal FeatureGroup(string identifier, bool moduleGroup = false, FeatureGroup parentGroup = null, bool isHidden = false)
    {
        Identifier = identifier;
        IsHidden = isHidden;
        if (moduleGroup)
        {
            IsModuleGroup = true;
            FeatureGroups.AddonGroups.Add(this);
        }
        else if (parentGroup != null)
        {
            ParentGroup = parentGroup;
            ParentGroup.SubGroups.Add(this);
        }
        FeatureGroups.AllGroups.Add(this);
    }

    private FeatureGroup() { }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Identifier;
    }

    /// <summary>
    /// Returns a string that represents the passed feature group.
    /// </summary>
    /// <param name="g">The feature group.</param>
    /// <returns>A string that represents this feature group.</returns>
    public static implicit operator string(FeatureGroup g) => g?.Identifier;

    /// <summary>
    /// Gets or creates a new top level feature group.
    /// </summary>
    /// <param name="identifier">The identifier of the group.</param>
    /// <returns>The existing or newly created group.</returns>
    public static implicit operator FeatureGroup(string identifier) =>
        FeatureGroups.GetOrCreateTopLevelGroup(identifier);
}