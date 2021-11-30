using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace TheArchive.Utilities
{
    public partial class Utils
    {
        // https://stackoverflow.com/a/11749642
        public static string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900);
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }

        // https://stackoverflow.com/a/600306
        public static bool IsPowerOfTwo(ulong x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        /// <summary>
        /// Loads an embedded resource from the calling assembly
        /// </summary>
        /// <param name="resourcePath">Path to resource</param>
        public static byte[] LoadFromResource(string resourcePath) => GetResource(Assembly.GetCallingAssembly(), resourcePath);

        /// <summary>
        /// Loads an embedded resource from an assembly
        /// </summary>
        /// <param name="assembly">Assembly to load from</param>
        /// <param name="resourcePath">Path to resource</param>
        public static byte[] GetResource(Assembly assembly, string resourcePath)
        {
            Stream stream = assembly.GetManifestResourceStream(resourcePath);
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int) stream.Length);
            return data;
        }

        public static string GetStartupTextForRundown(int rundownID)
        {
            RundownID currentRundownID = IntToRundownEnum(rundownID);

            switch (currentRundownID)
            {
                case RundownID.RundownOne:
                    return "<color=red>Rundown #001</color>\n\nThe Archive active.";
                case RundownID.RundownTwo:
                    return "<color=red>Rundown #002 Infection</color>\n\nThe Archive active.";
                case RundownID.RundownThree:
                    return "<color=red>Rundown #003 The Vessel</color>\n\nThe Archive active.";
                default:
                case RundownID.RundownUnknown:
                    return "<color=red>Rundown #??? Yo Waddup?!</color>\n\nThe Archive active.\nMight not work however I hope it works anyways lol.";
            }
        }

        public static RundownID IntToRundownEnum(int rundown)
        {
            RundownID currentRundownID;
            try
            {
                currentRundownID = (RundownID) rundown;
            }
            catch (Exception)
            {
                currentRundownID = RundownID.RundownUnknown;
            }

            return currentRundownID;
        }

        public class ValueAttribute : Attribute
        {
            public object Value { get; private set; }
            public Type Type { get; private set; }

            public ValueAttribute(object value)
            {
                Value = value;
                Type = value.GetType();
            }
        }

        public static int RundownIDToNumber(RundownID rundown)
        {
            switch(rundown) { }
            return 0;
        }

        public enum RundownID : int
        {
            RundownUnknown = 1,
            [Value(1)]
            RundownOne = 17,
            [Value(2)]
            RundownTwo = 19,
            [Value(3)]
            RundownThree = 22,
            [Value(4)]
            RundownFour = 25,
            [Value(5)]
            RundownFive = 26
        }

        [Flags]
        public enum RundownFlags : int
        {
            None = 0,
            RundownOne = 1 << 0,
            RundownTwo = 1 << 1,
            RundownThree = 1 << 2,
            RundownFour = 1 << 3,
            RundownFive = 1 << 4,
            All = RundownOne | RundownTwo | RundownThree | RundownFour | RundownFive,
        }

        public static bool FlagsContain(RundownFlags flags, RundownID id)
        {
            if (id == RundownID.RundownUnknown) return false;

            RundownFlags currentRundown;
            if (!Enum.TryParse<RundownFlags>(id.ToString(), out currentRundown))
            {
                return false;
            }

            if((flags & currentRundown) == currentRundown)
            {
                return true;
            }

            return false;
        }

        public static RundownFlags FlagsFromTo(RundownFlags from, RundownFlags to)
        {
            if (from == to) return from;

            if(!IsPowerOfTwo((ulong) from) || !IsPowerOfTwo((ulong) to) || from > to)
            {
                return RundownFlags.None;
            }

            RundownFlags? flags = null;

            for(int i = (int) from; i <= (int) to; i = i * 2)
            {
                if(flags.HasValue)
                {
                    flags |= (RundownFlags) i;
                    continue;
                }
                flags = (RundownFlags) i;
            }

            return flags.Value;
        }
    }
}
