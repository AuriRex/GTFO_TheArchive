namespace TheArchive.Core.Localization;

/// <summary>
/// The different languages that are supported.
/// </summary>
[Localized]
public enum Language
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    English = 0,
    Chinese = 1,
    French,
    Italian,
    German,
    Spanish,
    Russian,
    Portuguese_Brazil,
    Polish,
    Japanese,
    Korean
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}