using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Loader
{
    internal static class ModuleInitializer
    {
        internal static void Run()
        {
            CoreModLoader.LoadMainModASM();
        }
    }
}
