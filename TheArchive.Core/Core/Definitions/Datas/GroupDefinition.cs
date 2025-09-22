using System.Collections.Generic;
using TheArchive.Core.Localization.Datas;

namespace TheArchive.Core.Definitions.Datas;

public enum GroupType
{
    TopLevel,
    Module,
    Feature
}

/// <summary>
/// A single group definition.
/// </summary>
public abstract class GroupDefinitionBase
{
    /// <summary>
    /// The name of a group.
    /// </summary>
    public virtual string Name { get; set; } = null;

    /// <summary>
    /// The type of a group.
    /// </summary>
    public virtual GroupType Type { get; set; }

    /// <summary>
    /// The group is hidden.
    /// </summary>
    public virtual bool IsHidden { get; set; } = false;

    /// <summary>
    /// The group localization data.
    /// </summary>
    public virtual GroupLocalizationData LocalizationData { get; set; } = new();
}

/// <summary>
/// A single top-level group definition.
/// </summary>
public class TopLevelGroupDefinition : GroupDefinitionBase
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override GroupType Type => GroupType.TopLevel;

    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsHidden => false;
}

/// <summary>
/// A single feature group definition.
/// </summary>
public class FeatureGroupDefinition : GroupDefinitionBase
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override GroupType Type => GroupType.Feature;


    public List<FeatureGroupDefinition> SubGroups { get; set; } = new();
}

/// <summary>
/// A single module group definition.
/// </summary>
public class ModuleGroupDefinition : GroupDefinitionBase
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override string Name => null;

    /// <inheritdoc/>
    [JsonIgnore]
    public override GroupType Type => GroupType.Module;

    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsHidden => false;

    public List<FeatureGroupDefinition> SubGroups { get; set; } = new();
}
