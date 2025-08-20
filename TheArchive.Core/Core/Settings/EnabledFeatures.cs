using System.Collections.Generic;

namespace TheArchive.Core.Settings;

/// <summary>
/// Enabled features config.
/// </summary>
public class EnabledFeatures
{
    /// <summary>
    /// A dictionary of all features and their enabled state.
    /// </summary>
    public Dictionary<string, bool> Features { get; set; } = new();
}