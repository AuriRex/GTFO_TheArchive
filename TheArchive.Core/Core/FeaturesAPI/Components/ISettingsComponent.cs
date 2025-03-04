using TMPro;

namespace TheArchive.Core.FeaturesAPI.Components;

public interface ISettingsComponent
{
    public bool HasPrimaryText { get; }
    public TextMeshPro PrimaryText { get; set; }
    public bool HasSecondaryText { get; }
    public TextMeshPro SecondaryText { get; set; }
}