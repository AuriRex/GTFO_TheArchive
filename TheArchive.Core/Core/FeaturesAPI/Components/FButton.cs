using System;
using TMPro;

namespace TheArchive.Core.FeaturesAPI.Components;

/// <summary>
/// Used to define a Mod Settings Button element.
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
///         public FButton MyCustomButton => new FButton("Click Me");
///     }
/// }
/// </code></example>
public class FButton : ISettingsComponent
{
    /// <summary>
    /// Button Text
    /// </summary>
    public string ButtonText { get; set; }
    internal string ButtonID { get; set; }

    /// <inheritdoc/>
    public bool HasPrimaryText => PrimaryText != null;

    /// <summary>
    /// The primary text component (The text element on the left side, FSDisplayName)
    /// </summary>
    public TextMeshPro PrimaryText { get; set; }

    /// <inheritdoc/>
    public bool HasSecondaryText => SecondaryText != null;

    /// <summary>
    /// The secondary text component (The text element on the right side, button text)
    /// </summary>
    public TextMeshPro SecondaryText { get; set; }

    /// <summary>
    /// If this button has a callback action set.
    /// </summary>
    public bool HasCallback => Callback != null;

    internal Action Callback { get; }

    /// <summary>
    /// If the submenu the button is in should refresh after pressing.
    /// </summary>
    public bool RefreshSubMenu { get; }

    /// <summary>
    /// Creates a button
    /// </summary>
    /// <seealso cref="FButton(string, string, Action, bool)"/>
    public FButton() { }

    /// <summary>
    /// Creates a button
    /// </summary>
    /// <param name="buttonText">The button text</param>
    /// <param name="buttonId">The buttons ID, default is the property name</param>
    /// <param name="callback">Action invoked on buttonpress.<br/><b>This fires even if the Feature is disabled!</b></param>
    /// <param name="refreshSubMenu">If the submenu the button is in should refresh after pressing.</param>
    public FButton(string buttonText, string buttonId = null, Action callback = null, bool refreshSubMenu = false)
    {
        ButtonText = buttonText;
        ButtonID = buttonId;
        Callback = callback;
        RefreshSubMenu = refreshSubMenu;
    }
}