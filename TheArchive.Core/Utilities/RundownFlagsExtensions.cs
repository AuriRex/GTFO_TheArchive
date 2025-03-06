using System;
using System.Collections.Generic;
using System.Linq;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Utilities;

public static class RundownFlagsExtensions
{
    /// <summary>
    /// Check if the <paramref name="rundownID"/> is included in the specified <paramref name="flags"/>
    /// </summary>
    /// <param name="rundownID"></param>
    /// <param name="flags"></param>
    /// <returns>True if <paramref name="flags"/> contains the <paramref name="rundownID"/></returns>
    public static bool IsIncludedIn(this RundownID rundownID, RundownFlags flags)
    {
        return FlagsContain(flags, rundownID);
    }

    /// <summary>
    /// Get all flags between <paramref name="flags"/> (including) and <paramref name="to"/> (including) set.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static RundownFlags To(this RundownFlags flags, RundownFlags to)
    {
        if (to == RundownFlags.Latest)
            return flags.ToLatest();
        if (((int) flags) > ((int) to))
            return FlagsFromTo(to, flags);
        return FlagsFromTo(flags, to);
    }

    /// <summary>
    /// Returns <see cref="RundownFlags"/> set from <paramref name="flags"/> to <seealso cref="RundownFlags.Latest"/>
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public static RundownFlags ToLatest(this RundownFlags flags)
    {
        return FlagsFromTo(flags, RundownFlags.Latest);
    }

    private static IEnumerable<RundownFlags> _allFlagsOrdered;
    // https://stackoverflow.com/a/2344594
    public static IEnumerable<RundownFlags> AllFlagsOrdered
    {
        get
        {
            if (_allFlagsOrdered == null)
            {
                _allFlagsOrdered = Enum.GetValues(typeof(RundownFlags))
                    .Cast<RundownFlags>()
                    .OrderBy(x => x)
                    .Skip(2);
            }
            return _allFlagsOrdered;
        }
    }

    /// <summary>
    /// Get the lowest flag in the given <paramref name="flags"/>, excluding <see cref="RundownFlags.None"/>
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public static RundownFlags LowestRundownFlag(this RundownFlags flags)
    {
        return AllFlagsOrdered.FirstOrDefault(x => flags.HasFlag(x));
    }

    /// <summary>
    /// Get the highest flag in the given <paramref name="flags"/>
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public static RundownFlags HighestRundownFlag(this RundownFlags flags)
    {
        return AllFlagsOrdered.LastOrDefault(x => flags.HasFlag(x));
    }
}