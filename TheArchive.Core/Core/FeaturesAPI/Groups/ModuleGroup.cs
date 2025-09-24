using System;
using System.Linq;

namespace TheArchive.Core.FeaturesAPI.Groups;

/// <summary>
/// A module group
/// </summary>
public sealed class ModuleGroup : GroupBase
{
    /// <summary>
    /// Constructor for ModuleGroup.
    /// </summary>
    /// <param name="owner">The module that owns this group.</param>
    internal ModuleGroup(IArchiveModule owner)
        : base(owner, $"{owner.GetType().Assembly.GetName().Name}.ModuleGroup", null, false)
    {
        GroupManager.ModuleGroups[$"{owner.GetType().Assembly.GetName().Name}.ModuleGroup"] = this;
    }


    /// <inheritdoc/>
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
