using System;
using TheArchive.Utilities;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Formats a timestamp into a human-readable string.
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
///         // Turns the timestamp into something readable by humans.
///         [FSTimestamp]
///         public long MyCustomTimeStamp { get; set; } = 0;
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public class FSTimestamp : Attribute
{
    /// <summary>
    /// The custom timestamp format.
    /// </summary>
    public string Format { get; } = "U";

    /// <summary>
    /// Formats a timestamp into a readable string.
    /// </summary>
    /// <param name="customFormat">A custom format to use.</param>
    public FSTimestamp(string customFormat = "U")
    {
        try
        {
            Format = customFormat;

            _ = DateTime.Now.ToString(Format);
        }
        catch (Exception)
        {
            ArchiveLogger.Warning($"A {nameof(FSTimestamp)}s custom format threw an exception! Format String: \"{Format}\"");
            Format = "U";
        }
    }
}