namespace TheArchive.Core.Localization;

/// <summary>
/// An interface used to update text whenever the language changes.
/// </summary>
public interface ILocalizedTextUpdater
{
    /// <summary>
    /// Called whenever the language changes.
    /// </summary>
    void UpdateText();
}