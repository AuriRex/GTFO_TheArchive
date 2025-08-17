using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Managers;
using TheArchive.Loader;

namespace TheArchive.Utilities;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Utils
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public const BindingFlags AnyBindingFlagss = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    internal static int RandomRangeInt(int min, int max)
    {
        return UnityEngine.Random.RandomRangeInt(min, max);
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
        var c = 0;
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
        var c = 0;
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
        return Enum.TryParse(name, out value);
    }

    public static object StartCoroutine(IEnumerator routine) => LoaderWrapper.StartCoroutine(routine);

    public static void StopCoroutine(object coroutineToken) => LoaderWrapper.StopCoroutine(coroutineToken);

    public static string GetRundownTag(RundownFlags rundowns, bool generalizeLatest = false)
    {
        _ = Enum.TryParse<RundownID>(rundowns.LowestRundownFlag().ToString(), out var lowestId);
        _ = Enum.TryParse<RundownID>(rundowns.HighestRundownFlag().ToString(), out var highestId);
        
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

    public static string GetRundownTitle() => GetRundownTitle(ArchiveMod.CurrentRundown);
    public static string GetRundownTitle(RundownID rundown)
    {
        return rundown switch
        {
            RundownID.RundownOne => "Deviation",
            RundownID.RundownTwo => "Infection",
            RundownID.RundownThree => "The Vessel",
            RundownID.RundownFour => "Contact",
            RundownID.RundownFive => "Rebirth",
            RundownID.RundownSix => "Destination",
            RundownID.RundownSeven => "Rise",
            RundownID.RundownEight => "Duality",
            _ => "Unknown"
        };
    }

    public static string GetHash(byte[] bytes)
    {
        using (var hasher = SHA256.Create())
        {
            var hashBytes = hasher.ComputeHash(bytes);

            var builder = new StringBuilder();
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
        for(var i = 0; i < replacementData.Length; i++)
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

            var list = (IList) Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(type));

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
        return number switch
        {
            < 0 or > 3999 => throw new ArgumentOutOfRangeException(nameof(number), "Value has to be between 1 and 3999"),
            < 1 => string.Empty,
            >= 1000 => "M" + ToRoman(number - 1000),
            >= 900 => "CM" + ToRoman(number - 900),
            >= 500 => "D" + ToRoman(number - 500),
            >= 400 => "CD" + ToRoman(number - 400),
            >= 100 => "C" + ToRoman(number - 100),
            >= 90 => "XC" + ToRoman(number - 90),
            >= 50 => "L" + ToRoman(number - 50),
            >= 40 => "XL" + ToRoman(number - 40),
            >= 10 => "X" + ToRoman(number - 10),
            >= 9 => "IX" + ToRoman(number - 9),
            >= 5 => "V" + ToRoman(number - 5),
            >= 4 => "IV" + ToRoman(number - 4),
            >= 1 => "I" + ToRoman(number - 1)
        };
    }

    // https://stackoverflow.com/a/600306
    public static bool IsPowerOfTwo(ulong x)
    {
        return (x != 0) && ((x & (x - 1)) == 0);
    }

    // https://stackoverflow.com/a/6276029
    public static string ReplaceCaseInsensitive(this string input, string search, string replacement)
    {
        var result = Regex.Replace(
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
        var stream = assembly.GetManifestResourceStream(resourcePath);

        if (stream == null || !stream.CanRead)
        {
            throw new ArgumentException("Resource could not be loaded.", nameof(resourcePath));
        }
        
        var data = new byte[stream.Length];
        _ = stream.Read(data, 0, (int) stream.Length);
        return data;
    }

    public static string GetStartupTextForRundown(RundownID currentRundownID)
    {
        var sb = new StringBuilder();
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

    public static IEnumerator NextFrame(Action action)
    {
        yield return null;
        action?.Invoke();
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

    private static RundownID? _latestRundownID;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static RundownID GetLatestRundownID()
    {
        return _latestRundownID ??= GetEnumFromName<RundownID>(GetLatestRundownFlags().ToString());
    }

    /// <summary>
    /// <c>RundownID</c> is used to identify game builds in a more broad way.<br/>
    /// They apply to multiple game builds.
    /// </summary>
    /// <seealso cref="BuildDB"/>
    /// <seealso cref="RundownFlags"/>
    /// <remarks>
    /// Used in combination with <see cref="RundownFlags"/>.
    /// </remarks>
    public enum RundownID
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
    private const RundownFlags LATEST_RUNDOWN_FLAGS = RundownFlags.RundownEight;
    // IMPORTANT VALUE ABOVE
    // IMPORTANT VALUE ABOVE
    // IMPORTANT VALUE ABOVE

    /// <summary>
    /// <c>RundownFlags</c> are used in combination with <c>RundownID</c> to restrict certain features or patches depending on the currently running game version.
    /// </summary>
    /// <seealso cref="BuildDB"/>
    /// <seealso cref="RundownID"/>
    /// <remarks>
    /// <b>Avoid</b> using <c>RundownFlags.Latest</c> outside of <see cref="RundownConstraint"/>, similar Attributes or without using <see cref="RundownFlagsExtensions.To(RundownFlags, RundownFlags)"/>
    /// </remarks>
    [Flags]
    public enum RundownFlags
    {
        /// <summary> <b>Avoid</b> using <c>RundownFlags.Latest</c> outside of <see cref="RundownConstraint"/>, similar Attributes or without using <see cref="RundownFlagsExtensions.To(RundownFlags, RundownFlags)"/> </summary>
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
    }

    public const RundownFlags ALL_RUNDOWN_FLAGS = RundownFlags.RundownOne | RundownFlags.RundownTwo | RundownFlags.RundownThree
                                    | RundownFlags.RundownFour | RundownFlags.RundownFive | RundownFlags.RundownSix
                                    | RundownFlags.RundownSeven | RundownFlags.RundownAltOne | RundownFlags.RundownAltTwo
                                    | RundownFlags.RundownAltThree | RundownFlags.RundownAltFour | RundownFlags.RundownAltFive
                                    | RundownFlags.RundownAltSix | RundownFlags.RundownEight;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static RundownFlags GetLatestRundownFlags()
    {
        return LATEST_RUNDOWN_FLAGS;
    }

    public static bool FlagsContain(RundownFlags flags, RundownID id)
    {
        if (flags == RundownFlags.None)
            return false;

        if (id == RundownID.RundownUnknown)
            return false;

        var latestFlags = GetLatestRundownFlags();

        if (flags == RundownFlags.Latest)
            flags = latestFlags;

        if (id == RundownID.Latest)
            id = GetLatestRundownID();

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

        for(var i = (int) from; i <= (int) to; i = i * 2)
        {
            if(flags.HasValue)
            {
                flags |= (RundownFlags) i;
                continue;
            }
            flags = (RundownFlags) i;
        }

        return flags!.Value;
    }

    public static bool AnyRundownConstraintMatches(MemberInfo memberInfo)
    {
        var constraints = memberInfo.GetCustomAttributes<RundownConstraint>().ToArray();

        if (constraints.Length == 0)
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
        var constraints = memberInfo.GetCustomAttributes<BuildConstraint>().ToArray();

        if (constraints.Length == 0)
            return true;

        var buildNumber = ArchiveMod.CurrentBuildInfo.BuildNumber;
        foreach (var constraint in constraints)
        {
            if (constraint.Matches(buildNumber))
                return true;
        }

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

    public static string HashString(this string input)
    {
        using var sha256 = SHA256.Create();
        
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(inputBytes);
        
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
    }

    
    public static bool TryParseRundownKey(string rundownKey, out uint rundownID)
    {
        // "Local_31"
        var parts = rundownKey.Split('_');

        if(parts.Length < 2)
        {
            rundownID = 0;
            return false;
        }

        return uint.TryParse(parts[1], out rundownID);
    }
}