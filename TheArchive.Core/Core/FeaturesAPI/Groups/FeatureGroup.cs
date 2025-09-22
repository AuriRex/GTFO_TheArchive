using System;
using System.Linq;

namespace TheArchive.Core.FeaturesAPI.Groups;

/// <summary>
/// A feature group
/// </summary>
public sealed class FeatureGroup : GroupBase
{
    /// <summary>
    /// Constructor for FeatureGroup.
    /// </summary>
    /// <param name="identifier">The identifier for this group.</param>
    /// <param name="parentGroup">The parent group, if any.</param>
    /// <param name="isHidden">Should the group be hidden?</param>
    internal FeatureGroup(string identifier, GroupBase parentGroup, bool isHidden = false)
        : base(parentGroup.Owner, identifier, parentGroup, isHidden)
    {
    }

    /// <summary>
    /// Gets or creates a new subgroup inside of this feature group.
    /// </summary>
    /// <param name="name">The subgroup name.</param>
    /// <param name="isHidden">Should the group be hidden (only visible in dev mode)?</param>
    /// <returns>The already existing subgroup with the provided name or a newly created one if no existing one was found.</returns>
    public override GroupBase GetOrCreateSubGroup(string name, bool isHidden = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"Parameter '{nameof(name)}' can not be null or white space");
        }

        var identifier = $"{Identifier}.{name}";
        var subGroup = SubGroups.FirstOrDefault(g => g.Identifier == identifier);

        if (subGroup != null)
            IsNewlyCreated = false;

        subGroup ??= new FeatureGroup(identifier, this, isHidden);

        return subGroup;
    }
}