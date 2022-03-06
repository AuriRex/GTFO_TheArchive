using System;
using System.Linq;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Utilities
{

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
            return Utils.FlagsContain(flags, rundownID);
        }

        /// <summary>
        /// Get all flags between <paramref name="flags"/> (including) and <paramref name="to"/> (including) set.
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static RundownFlags To(this RundownFlags flags, RundownFlags to)
        {
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

        /// <summary>
        /// Get the <seealso cref="int"/> Value of a <see cref="ValueAttribute"/> attached to a specific <seealso cref="Enum"/> entry.
        /// </summary>
        /// <typeparam name="T">An <seealso cref="Enum"/></typeparam>
        /// <param name="thisEnum"></param>
        /// <returns>The <seealso cref="int"/> value represented by the <seealso cref="ValueAttribute"/> or -1 if <typeparamref name="T"/> is not of type <seealso cref="Enum"/></returns>
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
