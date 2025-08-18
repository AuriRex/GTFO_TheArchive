using System;
using TheArchive.Core.Localization;
using TheArchive.Core.Models;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Inserts generic header above the decorated setting.
/// </summary>
/// <remarks>
/// <list>
/// <item>Use on a member of a type that's used by the feature settings system. (<c>[FeatureConfig]</c>)</item>
/// <item>Accepts TextMeshPro rich text tags.</item>
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
///         [FSHeader("Some Header Above")]
///         public string MyCustomString { get; set; } = "Something";
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public class FSHeader : Localized
{
    internal string Title => UntranslatedText;
    internal SColor Color { get; }
    internal bool Bold { get; }

    /// <summary>
    /// Inserts generic header above the decorated setting.
    /// </summary>
    /// <param name="title">The header title</param>
    /// <param name="bold">Bold text?</param>
    public FSHeader(string title, bool bold = true) : base(title)
    {
        Color = SColor.DARK_ORANGE.WithAlpha(0.8f);
        Bold = bold;
    }

    /// <summary>
    /// Inserts generic header above the decorated setting.
    /// </summary>
    /// <param name="title">The header title</param>
    /// <param name="color">The text color</param>
    /// <param name="bold">Bold text?</param>
    public FSHeader(string title, SColor color, bool bold = true) : base(title)
    {
        Color = color;
        Bold = bold;
    }
}