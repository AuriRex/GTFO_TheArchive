using System;
using System.Linq;

namespace TheArchive.Core.FeaturesAPI.Groups;

/// <summary>
/// A top level group
/// </summary>
public sealed class TopLevelGroup : GroupBase
{
    internal TopLevelGroup(string name, bool isHidden = false)
        : base(null, $"Core.{name}", null, isHidden)
    {
        GroupManager.TopLevelGroups[$"Core.{name}"] = this;
    }

    /// <inheritdoc/>
    public override GroupBase GetOrCreateSubGroup(string name, bool isHidden = false)
    {
        throw new InvalidOperationException($"Top-Level Group does not allow subgroups");

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


    /// <summary>
    /// Gets or creates a new top-level feature group.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <returns>The existing or newly created group.</returns>
    public static implicit operator TopLevelGroup(string name) =>
        GroupManager.GetOrCreateTopLevelGroup(name);
}
