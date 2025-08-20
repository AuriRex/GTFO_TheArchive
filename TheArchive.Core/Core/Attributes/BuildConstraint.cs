using System;

namespace TheArchive.Core.Attributes;

/// <summary>
/// Used to restrict this <c>Feature</c>/<c>ArchivePatch</c>/patch method to only run if the build number matches with the current game version.
/// </summary>
/// <remarks>
/// Can only be used on your <see cref="Feature.Patches.ArchivePatch"/> classes or member methods.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class BuildConstraint : Attribute
{
    private int BuildNumber { get; set; }
    private MatchMode Mode { get; set; }

    /// <summary>
    /// Used to restrict this <c>Feature</c>/<c>ArchivePatch</c>/patch method to only run if the build number matches with the current game version.
    /// </summary>
    /// <remarks>
    /// Can only be used on your <see cref="Feature.Patches.ArchivePatch"/> classes or member methods.
    /// </remarks>
    /// <param name="build">The build number</param>
    /// <param name="mode">The way it should be compared to the games build number</param>
    public BuildConstraint(int build, MatchMode mode = MatchMode.Exact)
    {
        BuildNumber = build;
        Mode = mode;
    }
    
    /// <summary>
    /// Checks if the <c>BuildNumber</c> matches the passed one depending on the selected <c>MatchMode</c>.
    /// </summary>
    /// <seealso cref="MatchMode"/>
    /// <param name="buildNumber">The build number to check.</param>
    /// <returns><c>True</c> if the build number matches.</returns>
    public bool Matches(int buildNumber)
    {
        switch (Mode)
        {
            default:
            case MatchMode.Exact:
                return buildNumber == BuildNumber;
            case MatchMode.Greater:
                return buildNumber > BuildNumber;
            case MatchMode.GreaterOrEqual:
                return buildNumber >= BuildNumber;
            case MatchMode.Lower:
                return buildNumber < BuildNumber;
            case MatchMode.LowerOrEqual:
                return buildNumber <= BuildNumber;
            case MatchMode.Exclude:
                return buildNumber != BuildNumber;
        }
    }

    /// <summary>
    /// How two numbers should be compared.
    /// </summary>
    public enum MatchMode
    {
        /// <summary> Build numbers must match exactly. </summary>
        Exact,
        /// <summary> The games build number has to be lower than this one. </summary>
        Lower,
        /// <summary> The games build number has to be lower or equal than this one. </summary>
        LowerOrEqual,
        /// <summary> The games build number has to be greater than this one. </summary>
        Greater,
        /// <summary> The games build number has to be greater or equal than this one. </summary>
        GreaterOrEqual,
        /// <summary> The games build number has to be anything but this one. </summary>
        Exclude
    }
}