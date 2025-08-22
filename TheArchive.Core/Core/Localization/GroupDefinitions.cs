using System.Collections.Generic;

namespace TheArchive.Core.Localization;

/// <summary>
/// Defines groups & their localization data.
/// </summary>
public class GroupDefinitions
{
    /// <summary>
    /// A list of groups.
    /// </summary>
    public List<GroupDef> Groups { get; set; } = new() { new() };

    /// <summary>
    /// A single group definition.
    /// </summary>
    public class GroupDef
    {
        /// <summary>
        /// The ID of a group.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// The groups localization data.
        /// </summary>
        public GroupLocalization LocalizationData { get; set; } = new();
    }
}