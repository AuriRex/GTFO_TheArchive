using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TheArchive.Utilities
{
    public static class Utils
    {

        private static Type _UnityEngine_Time = Type.GetType("UnityEngine.Time, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        private static PropertyInfo _UnityEngine_time_PI = _UnityEngine_Time.GetProperty("time");

        internal static float Time
        {
            get
            {
                return (float) _UnityEngine_time_PI.GetValue(null);
            }
        }

        public static T GetEnumFromName<T>(string name) where T : struct
        {
            if(Enum.TryParse<T>(name, out var value))
            {
                return value;
            }

            return default;
        }

        public static object StartCoroutine(System.Collections.IEnumerator routine)
        {
            return MelonLoader.MelonCoroutines.Start(routine);
        }

        public static void StopCoroutine(object coroutineToken)
        {
            MelonLoader.MelonCoroutines.Stop(coroutineToken);
        }

        public static string GetRundownTitle() => GetRundownTitle(ArchiveMod.CurrentRundown);
        public static string GetRundownTitle(RundownID rundown)
        {
            switch(rundown)
            {
                case RundownID.RundownOne:
                    return "Rundown Protocol #001";
                case RundownID.RundownTwo:
                    return "Infection";
                case RundownID.RundownThree:
                    return "The Vessel";
                case RundownID.RundownFour:
                    return "Contact";
                case RundownID.RundownFive:
                    return "Rebirth";
                case RundownID.RundownSix:
                    return "Destination";
            }
            return "Unknown";
        }

        public const string EXTENDED_WITHOUT_TMP_TAGS = "://EXTENDED";
        public const string EXTENDED = "<color=orange><size=80%>" + EXTENDED_WITHOUT_TMP_TAGS + "</size></color>";

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

        // https://stackoverflow.com/a/6276029
        public static string ReplaceCaseInsensitive(this string input, string search, string replacement)
        {
            string result = Regex.Replace(
                input,
                Regex.Escape(search),
                replacement.Replace("$", "$$"),
                RegexOptions.IgnoreCase
            );
            return result;
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

        public static string GetStartupTextForRundown(int rundownID) => GetStartupTextForRundown(IntToRundownEnum(rundownID));
        public static string GetStartupTextForRundown(RundownID currentRundownID)
        {
            switch (currentRundownID)
            {
                case RundownID.RundownOne:
                    return "<color=red>Rundown #001</color>\n<size=80%><color=#8211b2>The Archive active.</color></size>\n\n";
                case RundownID.RundownTwo:
                    return "<color=red>Rundown #002 Infection</color>\n<size=80%><color=#8211b2>The Archive active.</color></size>\n\n";
                case RundownID.RundownThree:
                    return "<color=red>Rundown #003 The Vessel</color>\n<size=80%><color=#8211b2>The Archive active.</color></size>\n\n";
                case RundownID.RundownFour:
                    return "<color=red>Rundown #004 Contact</color><color=orange>://Extended</color>\n<size=80%><color=#8211b2>The Archive active.</color></size>\n\n";
                case RundownID.RundownFive:
                    return "<color=red>Rundown #005 Rebirth</color><color=orange>://Extended</color>\n<size=80%><color=#8211b2>The Archive active.</color></size>\n\n";
                default:
                case RundownID.RundownUnknown:
                    return "<color=red>Rundown #??? Yo Waddup?!</color>\n<size=80%><color=#8211b2>The Archive active.</color></size>\n\nIt's most likely not going to do what it's supposed to ... yet.";
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

        /*
         * Global.RundownIdToLoad
         * 17 = RD#001
         * 19 = RD#002
         * 22 = RD#003
         * 25 = RD#004
         * 26 = RD#005
         * 29 = RD#006
         */

        public enum RundownID : int
        {
            [Value(-1)]
            RundownUnitialized = 0,
            [Value(0)]
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
            RundownFive = 26,
            [Value(6)]
            RundownSix = 29,
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
            RundownSix = 1 << 5,
            All = RundownOne | RundownTwo | RundownThree | RundownFour | RundownFive | RundownSix,
            Latest = RundownSix,
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
            if ((int) from > (int) to) throw new ArgumentException($"{nameof(from)} ({from}) may not be larger than {nameof(to)} ({to})!");

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
