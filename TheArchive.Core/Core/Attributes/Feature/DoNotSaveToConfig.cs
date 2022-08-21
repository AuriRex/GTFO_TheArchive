using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DoNotSaveToConfig : Attribute
    {
    }
}
