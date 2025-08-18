using System;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Adds the little <c>[R4-RL]</c> hints on the right side to a feature setting.<br/>
/// This is only a visual indicator and does not prevent features/settings from activating.
/// </summary>
/// <remarks>
/// <list>
/// <item>Use on a member of a type that's used by the feature settings system. (<c>[FeatureConfig]</c>)</item>
/// </list>
/// </remarks>
/// <example><code>
/// public class MyFeature : Feature
/// {
///     [FeatureConfig]
///     public static MyCustomSettings Settings { get; set; }
///
///     public class MyCustomSettings
///     {
///         // The hint is going to show `[A5-RL]`
///         [FSRundownHint(RundownFlags.RundownAltFive, RundownFlags.Latest)]
///         public string MyCustomString { get; set; } = "Something";
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public class FSRundownHint : Attribute
{
    /// <summary>
    /// The rundowns this setting applies to.
    /// </summary>
    public RundownFlags Rundowns { get; private set; }
    
    /// <param name="flags">The rundowns this setting applies to</param>
    public FSRundownHint(RundownFlags flags)
    {
        Rundowns = flags;
    }
    
    /// <param name="from">Rundown from (inclusive)</param>
    /// <param name="to">Rundown to (inclusive)</param>
    public FSRundownHint(RundownFlags from, RundownFlags to)
    {
        Rundowns = from.To(to);
    }
}