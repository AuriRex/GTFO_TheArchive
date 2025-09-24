using System;
using System.Collections.Generic;
using TheArchive.Core.Localization;
using TheArchive.Core.Localization.Data;

namespace TheArchive.Core.FeaturesAPI.Groups;

/// <summary>
/// The base of ModuleGroup, TopLevelGroup and FeatureGroup.
/// </summary>
public abstract class GroupBase
{
    internal string DisplayName =>
        _localization.GetLocalizedDisplayName(ArchiveLocalizationService.CurrentLanguage, Identifier) ?? Identifier;

    internal string Description =>
        _localization.GetLocalizedDescription(ArchiveLocalizationService.CurrentLanguage, string.Empty) ?? string.Empty;

    /// <summary> The identifier of this group. </summary>
    public string Identifier { get; private set; } = string.Empty;

    /// <summary> If this group has been newly created. </summary>
    public bool IsNewlyCreated { get; internal set; } = true;

    /// <summary> Should this group be hidden in mod settings? </summary>
    public bool IsHidden { get; private set; }

    /// <summary> The parent group if this is a subgroup. </summary>
    public GroupBase ParentGroup { get; }

    /// <summary> The module that owns this group. </summary>
    public IArchiveModule Owner { get; }

    /// <summary> Collection of features in this group. </summary>
    internal HashSet<Feature> Features { get; } = new();

    /// <summary> Set of subgroups. </summary>
    public HashSet<GroupBase> SubGroups { get; } = new();

    private GroupLocalizationData _localization = new();

    /// <summary>
    /// Append the group's language translation values using a translation dictionary.
    /// </summary>
    /// <param name="localizationData">Localization data.</param>
    internal void AppendLocalization(GroupLocalizationData localizationData)
    {
        _localization.AppendData(localizationData);
    }

    /// <summary>
    /// Constructor for GroupBase.
    /// </summary>
    /// <param name="owner">The module that owns this group.</param>
    /// <param name="identifier">The identifier for this group.</param>
    /// <param name="parentGroup">The parent group, if any.</param>
    /// <param name="isHidden">Should the group be hidden?</param>
    protected GroupBase(IArchiveModule owner, string identifier, GroupBase parentGroup = null, bool isHidden = false)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException($"Parameter '{nameof(identifier)}' can not be null or white space");
        }

        Owner = owner;
        Identifier = identifier;
        IsHidden = isHidden;
        ParentGroup = parentGroup;

        if (parentGroup != null)
        {
            parentGroup.SubGroups.Add(this);
        }

        GroupManager.AllGroups[Identifier] = this;
    }

    private GroupBase() { }

    /// <summary>
    /// Gets or creates a new group inside of this group.
    /// </summary>
    /// <param name="name">The group name.</param>
    /// <param name="isHidden">Should the group be hidden (only visible in dev mode)?</param>
    /// <returns>The already existing group with the provided name or a newly created one if no existing one was found.</returns>
    public abstract GroupBase GetOrCreateSubGroup(string name, bool isHidden = false);

    /// <summary>
    /// Returns a string that represents the passed group.
    /// </summary>
    /// <param name="group">The group.</param>
    /// <returns>A string that represents this group.</returns>
    public static implicit operator string(GroupBase group) => group?.Identifier;

    /// <summary>
    /// Gets or creates a new top-level feature group.
    /// </summary>
    /// <param name="identifier">The identifier of the group.</param>
    /// <returns>The existing or newly created group.</returns>
    public static implicit operator GroupBase(string identifier) =>
        GroupManager.GetOrCreateTopLevelGroup(identifier);
}
