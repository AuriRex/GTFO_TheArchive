using System.Collections.Generic;

namespace TheArchive.Core.Definitions.Data;

/// <summary>
/// A single module definition.
/// </summary>
public class ModuleGroupsDefinition
{
    /// <summary>
    /// The module group definition.
    /// </summary>
    public ModuleGroupDefinition ModuleGroup { get; set; }

    /// <summary>
    /// The top-level group definitions.
    /// </summary>
    public List<TopLevelGroupDefinition> TopLevelGroups { get; set; } = new();
}
