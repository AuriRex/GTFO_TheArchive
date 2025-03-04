using TMPro;

namespace TheArchive.Core.FeaturesAPI.Components;

public class FLabel : ISettingsComponent
{
    public string LabelText { get; set; }
    internal string LabelID { get; private set; }

    public bool HasPrimaryText => PrimaryText != null;

    public TextMeshPro PrimaryText { get; set; }

    public bool HasSecondaryText => false;

    public TextMeshPro SecondaryText { get; set; }

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