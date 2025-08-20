using TMPro;

namespace TheArchive.Core.FeaturesAPI.Components;

/// <summary>
/// A special feature settings component.
/// </summary>
/// <seealso cref="FButton"/>
/// <seealso cref="FLabel"/>
public interface ISettingsComponent
{
    /// <summary>
    /// If <c>PrimaryText</c> is set.
    /// </summary>
    public bool HasPrimaryText { get; }
    
    /// <summary>
    /// The primary text component.
    /// </summary>
    public TextMeshPro PrimaryText { get; set; }
    
    /// <summary>
    /// If <c>SecondaryText</c> is set.
    /// </summary>
    public bool HasSecondaryText { get; }
    
    /// <summary>
    /// The secondary text component.
    /// </summary>
    public TextMeshPro SecondaryText { get; set; }
}