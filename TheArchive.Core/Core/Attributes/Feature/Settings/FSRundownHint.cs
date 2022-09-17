using System;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FSRundownHint : Attribute
    {
        public RundownFlags Rundowns { get; private set; }

        public FSRundownHint(RundownFlags flags)
        {
            Rundowns = flags;
        }

        public FSRundownHint(RundownFlags from, RundownFlags to)
        {
            Rundowns = from.To(to);
        }

        public bool Matches(RundownID value)
        {
            return value.IsIncludedIn(Rundowns);
        }
    }
}


