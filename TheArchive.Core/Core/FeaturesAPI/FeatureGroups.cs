using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.Localization;

namespace TheArchive.Core.FeaturesAPI;

/*
 *
 * Feature
 *   -> Group = "a.id.of.some.group"; // ID based assignment of groups for features
 *
 * // Feature Groups loaded from json file next to module dll?
 * FeatureGroup
 *   -> ID = "a.id.of.some.group"
 *   -> DisplayName
 *   -> Description
 *
 * ModuleLocalizationData
 *   -> Other module wide localization data maybe? (not sure if that is needed, would also give the module its own ILocalizationService)
 *   -> ModuleGroup // A default group for your module is created automatically, used by default if group is not specified in a feature?
 *     -> Name
 *       -> English
 *       -> Chinese
 *       -> ...
 *     -> Description (new)
 *       -> English
 *       -> Chinese
 *       -> ...
 * 
 */


/// <summary>
/// Feature groupings
/// </summary>
public static class FeatureGroups
{
    internal static FeatureGroup GetOrCreateTopLevelGroup(string identifier, bool dev = false, GroupLocalization localizationData = null)
    {
        var group = ArchiveCoreGroups.FirstOrDefault(g => g.Identifier == identifier, null);

        if (group != null)
            group.IsNewlyCreated = false;

        group ??= new FeatureGroup(identifier, false, null, dev);

        if (localizationData != null)
            group.SetLanguage(localizationData);
        
        ArchiveCoreGroups.Add(group);

        return group;
    }

    internal static FeatureGroup GetOrCreateModuleGroup(string identifier, GroupLocalization localizationData = null)
    {
        var group = ModuleGroups.FirstOrDefault(g => g.Identifier == identifier, null);

        if (group != null)
            group.IsNewlyCreated = false;

        group ??= new FeatureGroup(identifier, true, null);

        if (localizationData != null)
            group.SetLanguage(localizationData);

        return group;
    }

    internal static FeatureGroup GetGroup(string identifier) => AllGroups.FirstOrDefault(g => g.Identifier == identifier);
    internal static FeatureGroup GetModuleGroup(string identifier) => ModuleGroups.FirstOrDefault(g => g.Identifier == identifier);
    internal static HashSet<FeatureGroup> ArchiveCoreGroups { get; private set; } = new();
    internal static HashSet<FeatureGroup> ModuleGroups { get; private set; } = new();
    internal static HashSet<FeatureGroup> AllGroups { get; private set; } = new();

    #region Archive Groups
    internal static FeatureGroup ArchiveCore { get; private set; } = GetOrCreateTopLevelGroup(ArchiveMod.ARCHIVE_CORE_FEATUREGROUP);
    /// <summary> A feature group for accessibility and ease-of-use features. </summary>
    public static FeatureGroup Accessibility { get; private set; } = GetOrCreateTopLevelGroup("Core.Accessibility");
    /// <summary> A feature group for audio related features. </summary>
    public static FeatureGroup Audio { get; private set; } = GetOrCreateTopLevelGroup("Core.Audio");
    /// <summary> A feature group for cosmetic features. </summary>
    public static FeatureGroup Cosmetic { get; private set; } = GetOrCreateTopLevelGroup("Core.Cosmetic");
    /// <summary> A feature group for developer tools and similar. </summary>
    /// <remarks> This group is hidden by default! </remarks>
    public static FeatureGroup Dev { get; private set; } = GetOrCreateTopLevelGroup("Core.Developer", true);
    /// <summary> A feature group for features that fix things. </summary>
    public static FeatureGroup Fixes { get; private set; } = GetOrCreateTopLevelGroup("Core.Fixes");
    /// <summary> A feature group for HUD altering features. </summary>
    public static FeatureGroup Hud { get; private set; } = GetOrCreateTopLevelGroup("Core.HUD");
    /// <summary>
    /// A feature group for features related to local progression functionality.<br/>
    /// <i>Mostly unused now.</i>
    /// </summary>
    public static FeatureGroup LocalProgression { get; private set; } = GetOrCreateTopLevelGroup("Core.LocalProgression"); // , group => group.InlineSettings = true
    /// <summary> A feature group for any special or miscellaneous features that don't fit anywhere else. </summary>
    public static FeatureGroup Special { get; private set; } = GetOrCreateTopLevelGroup("Core.Misc");
    /// <summary> A feature group for steam/discord rich presence related features. </summary>
    public static FeatureGroup Presence { get; private set; } = GetOrCreateTopLevelGroup("Core.Presence");
    /// <summary> A feature group for security related features. </summary>
    public static FeatureGroup Security { get; private set; } = GetOrCreateTopLevelGroup("Core.Security");
    /// <summary> A feature group for QoL features. </summary>
    public static FeatureGroup QualityOfLife { get; private set; } = GetOrCreateTopLevelGroup("Core.QoL");
    #endregion
}

/// <summary>
/// A feature group.
/// </summary>
public class FeatureGroup
{
    internal string DisplayName =>
        _localization?.GetLocalizedDisplayName(LocalizationCoreService.CurrentLanguage, Identifier);

    internal string Description =>
        _localization?.GetLocalizedDescription(LocalizationCoreService.CurrentLanguage, string.Empty);
    
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
            FeatureGroups.ModuleGroups.Add(this);
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
}