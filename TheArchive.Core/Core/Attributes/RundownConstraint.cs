using System;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RundownConstraint : Attribute
{
    public RundownFlags Rundowns { get; private set; }

    public RundownConstraint(RundownFlags flags)
    {
        Rundowns = flags;
    }

    public RundownConstraint(RundownFlags from, RundownFlags to)
    {
        Rundowns = from.To(to);
    }

    public bool Matches(RundownID value)
    {
        return value.IsIncludedIn(Rundowns);
    }

}