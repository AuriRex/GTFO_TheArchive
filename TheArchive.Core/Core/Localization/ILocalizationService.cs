using System;

namespace TheArchive.Core.Localization;

/// <summary>
/// Localization service.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Get the currently set language.
    /// </summary>
    Language CurrentLanguage { get; }

    /// <summary>
    /// Get generic localization texts for your feature.
    /// </summary>
    /// <param name="id">The localization ID to get.</param>
    /// <param name="fallback"></param>
    /// <returns>The localized string.</returns>
    string GetById(uint id, string fallback = null);

    /// <summary>
    /// Get the localization text of a value.
    /// </summary>
    /// <param name="value">The value to localize.</param>
    /// <returns>The localized string.</returns>
    string Get<T>(T value);

    /// <summary>
    /// Gets the localized text for <paramref name="id"/> and then formats it using the given <paramref name="args"/>.
    /// </summary>
    /// <param name="id">Localization ID to format.</param>
    /// <param name="fallback"></param>
    /// <param name="args">Arguments to insert into the format string.</param>
    /// <returns>The localized, formatted string.</returns>
    string Format(uint id, string fallback = null, params object[] args);

    /// <summary>
    /// Registers a text setter.
    /// </summary>
    /// <param name="textSetter">The text setter to register.</param>
    /// <param name="textId">The localization ID.</param>
    /// <param name="fallback"></param>
    /// <exception cref="ArgumentException">The text setter has already been added.</exception>
    void AddTextSetter(ILocalizedTextSetter textSetter, uint textId, string fallback = null);

    /// <summary>
    /// Registers a text updater.
    /// </summary>
    /// <param name="textUpdater">The text updater to register.</param>
    void AddTextUpdater(ILocalizedTextUpdater textUpdater);

    /// <summary>
    /// Removes an already registered text setter.
    /// </summary>
    /// <param name="textSetter">The text setter to remove.</param>
    void RemoveTextSetter(ILocalizedTextSetter textSetter);

    /// <summary>
    /// Removes an already registered text updater.
    /// </summary>
    /// <param name="textUpdater">The text updater to remove.</param>
    void RemoveTextUpdater(ILocalizedTextUpdater textUpdater);
}