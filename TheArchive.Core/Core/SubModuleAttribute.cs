using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.Core
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
