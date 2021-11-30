using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Utilities
{

    public static class RundownFlagsExtensions
    {
        public static RundownFlags To(this RundownFlags flags, RundownFlags to)
        {
            if (((int) flags) > ((int) to))
                return FlagsFromTo(to, flags);
            return FlagsFromTo(flags, to);
        }

        public static int GetIntValue<T>(this T thisEnum) where T : IConvertible
        {
            if (thisEnum is Enum)
            {
                Type type = thisEnum.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == thisEnum.ToInt32(System.Globalization.CultureInfo.InvariantCulture))
                    {
                        var valueAttribute = type.GetField(type.GetEnumName(val))
                            .GetCustomAttributes(typeof(ValueAttribute), false)
                            .FirstOrDefault() as ValueAttribute;

                        if (valueAttribute != null && valueAttribute.Type == typeof(int))
                        {
                            return (int) valueAttribute.Value;
                        }
                    }
                }
            }

            return -1;
        }
    }

    
}
