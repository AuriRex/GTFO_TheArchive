using System;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SubModuleAttribute : Attribute
    {
        public RundownFlags Rundowns { get; set; }

        public SubModuleAttribute(RundownFlags from, RundownFlags to)
        {
            Rundowns = from.To(to);
        }

    }
}
