using System;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.Attributes;

/// <summary>
/// Used to restrict this <c>Feature</c>/<c>ArchivePatch</c>/patch method to only run if the overall 'rundown version' matches with the currently running game version.
/// </summary>
/// <seealso cref="RundownFlags"/>
/// <remarks>
/// Can only be used on your <see cref="Feature.Patches.ArchivePatch"/> classes or member methods.
/// </remarks>
/// <example><code>
/// // This patch will only be applied on alt rundown five game builds or later
/// [RundownConstraint(RundownFlags.RundownAltFive, RundownFlags.Latest)]
/// [ArchivePatch(typeof(TypeToPatch), nameof(TypeToPatch.MethodToPatch))]
/// internal static class MyArchivePatch
/// {
///     [IsPrefix]
///     [RundownConstraint(RundownFlags.RundownAltFive)]
///     public static void PrefixAR5()
///     {
///         // This prefix is used on alt rundown five game builds.
///     }
///
///     [IsPrefix]
///     [RundownConstraint(RundownFlags.RundownAltSix, RundownFlags.Latest)]
///     public static void PrefixNew()
///     {
///         // This prefix is used on alt rundown six and later game builds.
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RundownConstraint : Attribute
{
    /// <summary>
    /// The rundowns this patch should apply to.
    /// </summary>
    /// <seealso cref="RundownFlags"/>
    public RundownFlags Rundowns { get; private set; }

    /// <summary>
    /// Used to restrict this <c>Feature</c>/<c>ArchivePatch</c>/patch method to only run if the overall 'rundown version' matches with the currently running game version.
    /// </summary>
    /// <seealso cref="RundownFlags"/>
    /// <remarks>
    /// Can only be used on your <see cref="Feature.Patches.ArchivePatch"/> classes or member methods.
    /// </remarks>
    /// <param name="flags"></param>
    public RundownConstraint(RundownFlags flags)
    {
        Rundowns = flags;
    }

    /// <summary>
    /// Used to restrict this <c>Feature</c>/<c>ArchivePatch</c>/patch method to only run if the overall 'rundown version' matches with the currently running game version.
    /// </summary>
    /// <seealso cref="RundownFlags"/>
    /// <remarks>
    /// Can only be used on your <see cref="Feature.Patches.ArchivePatch"/> classes or member methods.
    /// </remarks>
    /// <param name="from">RundownFlags from (inclusive)</param>
    /// <param name="to">RundownFlags to (inclusive)</param>
    public RundownConstraint(RundownFlags from, RundownFlags to)
    {
        Rundowns = from.To(to);
    }

    /// <summary>
    /// Checks if the <c>Rundowns</c> provided in this attribute contain the <c>RundownID</c> passed in.
    /// </summary>
    /// <seealso cref="RundownFlags"/>
    /// <seealso cref="RundownID"/>
    /// <param name="value">RundownID to check against</param>
    /// <returns><c>True</c> if <c>Rundowns</c> include <c>value</c></returns>
    public bool Matches(RundownID value)
    {
        return value.IsIncludedIn(Rundowns);
    }

}