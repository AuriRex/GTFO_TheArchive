using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TheArchive.Utilities
{
    public static class Utils
    {
        public const BindingFlags AnyBindingFlagss = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        private static Type _UnityEngine_Random = Type.GetType("UnityEngine.Random, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        private static MethodInfo _UnityEngine_Random_RandomRangeInt;
        private static MethodInfo _UnityEngine_Random_Range;

        static Utils()
        {
            try
            {
                _UnityEngine_Random_RandomRangeInt = _UnityEngine_Random.GetMethod("RandomRangeInt");
            } catch(Exception) { }

            try
            {
                _UnityEngine_Random_Range = _UnityEngine_Random.GetMethod("Range", new Type[] { typeof(int), typeof(int) });
            }
            catch (Exception) { }
        }

        private static Type _UnityEngine_Time = Type.GetType("UnityEngine.Time, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        private static PropertyInfo _UnityEngine_time_PI = _UnityEngine_Time.GetProperty("time");

        internal static float Time
        {
            get
            {
                return (float) _UnityEngine_time_PI.GetValue(null);
            }
        }

        internal static int RandomRangeInt(int min, int max)
        {
            if(_UnityEngine_Random_RandomRangeInt != null)
                return (int) _UnityEngine_Random_RandomRangeInt.Invoke(null, new object[] { min, max });
            return (int) _UnityEngine_Random_Range.Invoke(null, new object[] { min, max });
        }

        public static T PickRandom<T>(this T[] array)
        {
            if (array.Length == 0)
                return default;
            return (T)array.GetValue(RandomRangeInt(0, array.Length - 1));
        }

        public static string StripTMPTagsRegex(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        public static T PickRandom<T>(this List<T> list) => list.ToArray().PickRandom();

        public static T PickRandomExcept<T>(this T[] array, T except)
        {
            if (array.Length == 1) return array[0];
            int c = 0;
            T random;
            do
            {
                random = array.PickRandom();
                c++;
            }
            while (random.Equals(except) && c < 20);
            return random;
        }

        public static T PickRandomExcept<T>(this List<T> list, T except) => list == null ? default : list.ToArray().PickRandomExcept(except);

        /// <summary>
        /// Pick a Random value in the list using a <paramref name="selectFunction"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="selectFunction">Return true to select.</param>
        /// <returns></returns>
        public static T PickRandomExcept<T>(this T[] array, Func<T, bool> selectFunction)
        {
            if (array.Length == 1) return array[0];
            int c = 0;
            T random;
            do
            {
                random = array.PickRandom();
                c++;
            }
            while (!selectFunction.Invoke(random) && c < 20);
            return random;
        }

        /// <summary>
        /// Pick a Random value in the list using a <paramref name="selectFunction"/>
        /// (calls <seealso cref="List{T}.ToArray"/>)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="selectFunction">Return true to select.</param>
        /// <returns></returns>
        public static T PickRandomExcept<T>(this List<T> list, Func<T, bool> selectFunction) => list == null ? default : list.ToArray().PickRandomExcept(selectFunction);

        public static bool TryPickRandom<T>(this T[] array, out T value)
        {
            if (array.Length == 0)
            {
                value = default;
                return false;
            }

            value = array[RandomRangeInt(0, array.Length - 1)];
            return true;
        }

        public static bool TryPickRandom<T>(this List<T> list, out T value)
        {
            if (list.Count == 0)
            {
                value = default;
                return false;
            }

            value = list[RandomRangeInt(0, list.Count - 1)];
            return true;
        }

        public static T GetEnumFromName<T>(string name) where T : struct
        {
            if(Enum.TryParse<T>(name, out var value))
            {
                return value;
            }
            ArchiveLogger.Warning($"{nameof(GetEnumFromName)} couldn't resolve enum \"{name}\" from type \"{typeof(T).FullName}\"!");
            return default;
        }

        public static object StartCoroutine(System.Collections.IEnumerator routine) => LoaderWrapper.StartCoroutine(routine);

        public static void StopCoroutine(object coroutineToken) => LoaderWrapper.StopCoroutine(coroutineToken);

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
                case RundownID.RundownSeven:
                    return "Rise";
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

        public static string GetStartupTextForRundown(RundownID currentRundownID)
        {
            StringBuilder sb = new StringBuilder();
            switch (currentRundownID)
            {
                case RundownID.RundownOne:
                    sb.Append("<color=red>Rundown #001</color>");
                    break;
                case RundownID.RundownTwo:
                    sb.Append("<color=red>Rundown #002 Infection</color>");
                    break;
                case RundownID.RundownThree:
                    sb.Append("<color=red>Rundown #003 The Vessel</color>");
                    break;
                case RundownID.RundownFour:
                    sb.Append("<color=red>Rundown #004 Contact</color>");
                    break;
                case RundownID.RundownFive:
                    sb.Append("<color=red>Rundown #005 Rebirth</color>");
                    break;
                default:
                case RundownID.RundownUnknown:
                    return "<color=red>Rundown #??? Yo Waddup?!</color>\n\nThis shouldn't happen unless you somehow modified the datablocks in R1 to R5 builds ...\nAnyways, things are probably gonna break :)";
            }

            if(false)
            {
                sb.Append("<color=orange>://Extended</color>");
            }

            sb.Append("\n");

            sb.Append("<size=80%><color=#8211b2>The Archive active.</color></size>\n\n");

            return sb.ToString();
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
            RundownUnitialized = -1,
            [Value(0)]
            RundownUnknown = 0,
            [Value(17)]
            RundownOne = 1,
            [Value(19)]
            RundownTwo = 2,
            [Value(22)]
            RundownThree = 3,
            [Value(25)]
            RundownFour = 4,
            [Value(26)]
            RundownFive = 5,
            [Value(29)]
            RundownSix = 6,
            [Value(31)]
            RundownSeven = 7,

            Latest = RundownSeven,
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
            RundownSeven = 1 << 6,
            All = RundownOne | RundownTwo | RundownThree | RundownFour | RundownFive | RundownSix | RundownSeven,
            Latest = RundownSeven,
        }

        public static bool FlagsContain(RundownFlags flags, RundownID id)
        {
            if (id == RundownID.RundownUnknown) return false;

            if (!Enum.TryParse(id.ToString(), out RundownFlags currentRundown))
            {
                return false;
            }

            return (flags & currentRundown) == currentRundown;
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

        public static bool TryGetMethodByName(Type type, string name, out MethodInfo methodInfo)
        {
            if (type.GetMethods(AnyBindingFlagss).Any(x => x.Name.Equals(name)))
            {
                methodInfo = type.GetMethod(name, AnyBindingFlagss);
                return true;
            }
            methodInfo = null;
            return false;
        }
    }
}
