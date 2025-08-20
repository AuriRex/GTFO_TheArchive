using TMPro;

namespace TheArchive.Core.FeaturesAPI.Components;

/// <summary>
/// Used to define a Mod Settings Label element.<br/>
/// Labels span the entire length of the settings scroll window.
/// </summary>
/// <remarks>
/// <list>
/// <item>Do <b>not</b> implement a setter on your property!</item>
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
///         // Use FS... Attributes like usual.
///         public FLabel MyCustomButton => new FLabel("I am a label :)");
///     }
/// }
/// </code></example>
public class FLabel : ISettingsComponent
{
    /// <summary>
    /// The labels text
    /// </summary>
    public string LabelText { get; set; }
    internal string LabelID { get; set; }

    /// <inheritdoc/>
    public bool HasPrimaryText => PrimaryText != null;

    /// <inheritdoc/>
    public TextMeshPro PrimaryText { get; set; }

    /// <inheritdoc/>
    public bool HasSecondaryText => false;
    
    /// <inheritdoc/>
    public TextMeshPro SecondaryText { get; set; }

    /// <summary>
    /// Creates a label
    /// </summary>
    public FLabel() { }

    /// <summary>
    /// Creates a label
    /// </summary>
    /// <param name="labelText">The labels text</param>
    /// <param name="labelId">The labels ID, default is the property name</param>
    public FLabel(string labelText, string labelId = null)
    {
        LabelText = labelText;
        LabelID = labelId;
    }
}