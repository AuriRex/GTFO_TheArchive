using System;

namespace TheArchive.Core.Localization;

/// <summary>
/// Marks a feature setting or enum type as localizable.
/// </summary>
/// <remarks>
/// <c>FSDisplayName</c>, <c>FSDescription</c> and <c>FSHeader</c> already inherit from this type.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Enum, Inherited = true)]
public class Localized : Attribute
{
    /// <summary>
    /// The untranslated text.<br/>
    /// (Preferably in english if possible)
    /// </summary>
    public string UntranslatedText { get; }

    /// <summary>
    /// Marks a feature setting as localizable.
    /// </summary>
    /// <param name="text">The untranslated text.</param>
    protected Localized(string text)
    {
        UntranslatedText = text;
    }
    
    /// <summary>
    /// Enum type variant constructor.
    /// </summary>
    public Localized() { }
}