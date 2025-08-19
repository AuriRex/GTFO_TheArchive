namespace TheArchive.Core.Localization;

/// <summary>
/// An interface used to update text whenever the language changes.
/// </summary>
public interface ILocalizedTextSetter
{
    /// <summary>
    /// Called whenever the language changes.
    /// </summary>
    /// <param name="text">The localized text to set.</param>
    void SetText(string text);
}