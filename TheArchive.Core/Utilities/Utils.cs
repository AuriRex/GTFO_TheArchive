﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TheArchive.Core.Attributes;
using TheArchive.Core.Managers;
using TheArchive.Loader;

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

        internal static float Unity_Time
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

        public static bool TryGetEnumFromName<T>(string name, out T value) where T : struct
        {
            if (Enum.TryParse<T>(name, out value))
            {
                return true;
            }
            return false;
        }

        public static object StartCoroutine(System.Collections.IEnumerator routine) => LoaderWrapper.StartCoroutine(routine);

        public static void StopCoroutine(object coroutineToken) => LoaderWrapper.StopCoroutine(coroutineToken);

        public static string GetRundownTag(RundownFlags rundowns, bool generalizeLatest = false)
        {
            Enum.TryParse<RundownID>(rundowns.LowestRundownFlag().ToString(), out var lowestId);
            Enum.TryParse<RundownID>(rundowns.HighestRundownFlag().ToString(), out var highestId);

            if (highestId == 0)
                highestId = GetLatestRundownID();

            var isLatest = highestId == GetLatestRundownID();

            if (lowestId == highestId)
            {
                var R = "R";
                if (lowestId >= RundownID.RundownAltOne && lowestId < RundownID.RundownEight)
                {
                    R = "A";
                    lowestId = lowestId - (int)RundownID.RundownAltOne + 1;
                }
                if (lowestId >= RundownID.RundownEight)
                {
                    lowestId = lowestId - 6;
                }
                return $"{R}{(int)lowestId}";
            }
            else
            {
                var RL = "R";
                if (lowestId >= RundownID.RundownAltOne && lowestId < RundownID.RundownEight)
                {
                    RL = "A";
                    lowestId = lowestId - (int)RundownID.RundownAltOne + 1;
                }
                else if (lowestId >= RundownID.RundownEight)
                {
                    lowestId = lowestId - 6;
                }

                var RH = "R";
                if (highestId >= RundownID.RundownAltOne && highestId < RundownID.RundownEight)
                {
                    RH = "A";
                    highestId = highestId - (int)RundownID.RundownAltOne + 1;
                }
                else if (highestId >= RundownID.RundownEight)
                {
                    highestId = highestId - 6;
                }
                return $"{RL}{(int)lowestId}-{(isLatest && generalizeLatest ? "RL" : RH+(int)highestId)}";
            }
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
                case RundownID.RundownSeven:
                    return "Rise";
            }
            return "Unknown";
        }

        public static string GetHash(byte[] bytes)
        {
            using (SHA256 hasher = SHA256.Create())
            {
                byte[] hashBytes = hasher.ComputeHash(bytes);

                StringBuilder builder = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Replaces format string literals ({0}, {1}, ...) in <paramref name="format"/> with the respective index in the params <paramref name="replacementData"/>
        /// </summary>
        /// <param name="format">String to format</param>
        /// <param name="replacementData">Data to be inserted</param>
        /// <returns>The formatted string with every {i} replaced with <paramref name="replacementData"/>[i]</returns>
        public static string UsersafeFormat(string format, params string[] replacementData)
        {
            for(int i = 0; i < replacementData.Length; i++)
            {
                var thisSequenceOfCharacters = $"{{{i}}}"; // {0}, {1}
                if (format.Contains(thisSequenceOfCharacters))
                    format = format.ReplaceCaseInsensitive(thisSequenceOfCharacters, replacementData[i]);
            }

            return format;
        }

        public static IList ToSystemListSlow(object allBlocks, Type type)
        {
            if(LoaderWrapper.IsGameIL2CPP())
            {
                var genListType = ImplementationManager.GameTypeByIdentifier("GenericList");

                var listType = genListType.MakeGenericType(type);

                IList list = (IList) Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(type));

                // Invoke to get UnhollowerBaseLib.Il2CppArrayBase<T>
                var listAsEnumerable = (IEnumerable) listType.GetMethod("ToArray", AnyBindingFlagss).Invoke(allBlocks, Array.Empty<object>());

                var enumerator = listAsEnumerable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    list.Add(enumerator.Current);
                }

                return list;
            }

            // Mono
            return (IList) allBlocks;
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

            /*if(false)
            {
                sb.Append("<color=orange>://Extended</color>");
            }*/

            sb.Append("\n");

            sb.Append("<size=80%><color=#8211b2>The Archive active.</color></size>\n\n");

            return sb.ToString();
        }

        [Obsolete]
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

        private static RundownID? _latestRundownID = null;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static RundownID GetLatestRundownID()
        {
            return _latestRundownID ??= GetEnumFromName<RundownID>(GetLatestRundownFlags().ToString());
        }

        /// <summary>
        /// This is used to identify the game build in a more broad way.
        /// </summary>
        public enum RundownID : int
        {
            Latest = -2,

            RundownUnitialized = -1,
            RundownUnknown = 0,
            RundownOne = 1,
            RundownTwo = 2,
            RundownThree = 3,
            RundownFour = 4,
            RundownFive = 5,
            RundownSix = 6,
            RundownSeven = 7,
            RundownAltOne = 8,
            RundownAltTwo = 9,
            RundownAltThree = 10,
            RundownAltFour = 11,
            RundownAltFive = 12,
            RundownAltSix = 13,
            RundownEight = 14,
        }

        // IMPORTANT VALUE HERE
        // IMPORTANT VALUE HERE
        // IMPORTANT VALUE HERE
        private const RundownFlags LatestRundownFlags = RundownFlags.RundownEight;
        // IMPORTANT VALUE ABOVE
        // IMPORTANT VALUE ABOVE
        // IMPORTANT VALUE ABOVE

        /// <summary>
        /// <b>Avoid</b> using <seealso cref="RundownFlags.Latest"/> outside of <seealso cref="RundownConstraint"/>, similar Attributes or without using <seealso cref="RundownFlagsExtensions.To(RundownFlags, RundownFlags)"/>
        /// </summary>
        [Flags]
        public enum RundownFlags : int
        {
            Latest = -2,

            None = 0,

            RundownOne = 1 << 0,
            RundownTwo = 1 << 1,
            RundownThree = 1 << 2,
            RundownFour = 1 << 3,
            RundownFive = 1 << 4,
            RundownSix = 1 << 5,
            RundownSeven = 1 << 6,
            RundownAltOne = 1 << 7,
            RundownAltTwo = 1 << 8,
            RundownAltThree = 1 << 9,
            RundownAltFour = 1 << 10,
            RundownAltFive = 1 << 11,
            RundownAltSix = 1 << 12,
            RundownEight = 1 << 13,

            All = RundownOne | RundownTwo | RundownThree | RundownFour | RundownFive
                | RundownSix | RundownSeven | RundownAltOne | RundownAltTwo | RundownAltThree
                | RundownAltFour | RundownAltFive | RundownAltSix | RundownEight,
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static RundownFlags GetLatestRundownFlags()
        {
            return LatestRundownFlags;
        }

        public static bool FlagsContain(RundownFlags flags, RundownID id)
        {
            if (id == RundownID.Latest)
                id = GetLatestRundownID();
            if (id == RundownID.RundownUnknown) return false;

            if (!Enum.TryParse(id.ToString(), out RundownFlags currentRundown))
            {
                return false;
            }

            return (flags & currentRundown) == currentRundown;
        }

        public static RundownFlags FlagsFromTo(RundownFlags from, RundownFlags to)
        {
            if (from == RundownFlags.Latest)
                from = GetLatestRundownFlags();
            if (to == RundownFlags.Latest)
                to = GetLatestRundownFlags();

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

        public static bool AnyRundownConstraintMatches(MemberInfo memberInfo)
        {
            var constraints = memberInfo.GetCustomAttributes<RundownConstraint>();

            if (constraints.Count() == 0)
                return true;

            var rundown = ArchiveMod.CurrentBuildInfo.Rundown;
            foreach (var constraint in constraints)
            {
                if (constraint.Matches(rundown))
                    return true;
            }

            return false;
        }

        public static bool AnyBuildConstraintMatches(MemberInfo memberInfo)
        {
            var constraints = memberInfo.GetCustomAttributes<BuildConstraint>();

            if (constraints.Count() == 0)
                return true;

            int buildNumber = ArchiveMod.CurrentBuildInfo.BuildNumber;
            foreach (var constraint in constraints)
            {
                if (constraint.Matches(buildNumber))
                    return true;
            }

            return false;
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

        public static HashSet<Type> GetNestedClasses(Type type)
        {
            var types = new List<Type> { type };
            foreach (var nestedType in type.GetNestedTypes())
            {
                if (!nestedType.IsClass)
                    continue;

                types.AddRange(GetNestedClasses(nestedType));
            }
            return types.ToHashSet();
        }

        public static string ComputeSHA256(this string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
