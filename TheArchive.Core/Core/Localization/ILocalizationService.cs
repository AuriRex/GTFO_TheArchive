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
    /// Get extra localization texts for your feature.
    /// </summary>
    /// <param name="id">The localization ID to get.</param>
    /// <returns>The localized string.</returns>
    string Get(uint id);

    /// <summary>
    /// Get the localized name of an enum.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>The localized enum name.</returns>
    string Get<T>(T value) where T : Enum;

    /// <summary>
    /// Gets the localized text for <paramref name="id"/> and then formats it using the given <paramref name="args"/>.
    /// </summary>
    /// <param name="id">Localization ID to format.</param>
    /// <param name="args">Arguments to insert into the format string.</param>
    /// <returns>The localized, formatted string.</returns>
    string Format(uint id, params object[] args);

    /// <summary>
    /// Registers a text setter.
    /// </summary>
    /// <param name="textSetter">The text setter to register.</param>
    /// <param name="textId">The localization ID.</param>
    /// <exception cref="ArgumentException">The text setter has already been added.</exception>
    void AddTextSetter(ILocalizedTextSetter textSetter, uint textId);

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