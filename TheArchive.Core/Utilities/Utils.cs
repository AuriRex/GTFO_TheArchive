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
using TheArchive.Core.Localization;
using TheArchive.Core.Managers;
using TheArchive.Loader;

namespace TheArchive.Utilities;

/// <summary>
/// Random utility methods.
/// </summary>
public static partial class Utils
{
    /// <summary>
    /// sssssssssssss
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public const BindingFlags AnyBindingFlagss = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    /// <summary>
    /// Safely invoke an event delegate.<br/>
    /// All subscribers will be executed and any exceptions are logged to console.
    /// </summary>
    /// <param name="action">The event delegate</param>
    /// <param name="args">Optional arguments</param>
    /// <typeparam name="T">Delegate type</typeparam>
    public static void SafeInvoke<T>(T action, params object[] args) where T : Delegate
    {
        if (action == null)
            return;
        
        foreach (var del in action.GetInvocationList())
        {
            try
            {
                del.DynamicInvoke(args);
            }
            catch (Exception ex)
            {
                ArchiveLogger.Warning($"Event {action.Method.Name} threw an exception: {ex.Message}");
                ArchiveLogger.Exception(ex);
            }
        }
    }

    /// <summary>
    /// Pick a random entry from an array.
    /// </summary>
    /// <param name="array">The set of inputs.</param>
    /// <typeparam name="T">Array type.</typeparam>
    /// <returns>A random array entry.</returns>
    public static T PickRandom<T>(this T[] array)
    {
        if (array.Length == 0)
            return default;
        return (T)array.GetValue(UnityEngine.Random.Range(0, array.Length));
    }

    /// <summary>
    /// Removes all <c>TextMeshPro</c> rich text tags from a given string.
    /// </summary>
    /// <param name="input">String to sanitize</param>
    /// <returns>String without any TMP tags</returns>
    public static string StripTMPTagsRegex(string input)
    {
        return Regex.Replace(input, "<.*?>", string.Empty);
    }

    /// <inheritdoc cref="PickRandom{T}(T[])"/>
    public static T PickRandom<T>(this List<T> list) => list.ToArray().PickRandom();

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

    /// <summary>
    /// Get an enum value by name.
    /// </summary>
    /// <param name="name">The enum name</param>
    /// <typeparam name="T">The enum type</typeparam>
    /// <returns>The found enum or default</returns>
    public static T GetEnumFromName<T>(string name) where T : struct
    {
        if(Enum.TryParse<T>(name, out var value))
        {
            return value;
        }
        ArchiveLogger.Warning($"{nameof(GetEnumFromName)} couldn't resolve enum \"{name}\" from type \"{typeof(T).FullName}\"!");
        return default;
    }

    /// <summary>
    /// Get an enum value by name.
    /// </summary>
    /// <param name="name">The enum name</param>
    /// <param name="value">The found enum or default</param>
    /// <typeparam name="T">The enum type</typeparam>
    /// <returns><c>True</c> if successful</returns>
    public static bool TryGetEnumFromName<T>(string name, out T value) where T : struct
    {
        return Enum.TryParse(name, out value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="routine"></param>
    /// <returns></returns>
    [Obsolete("Use \"LoaderWrapper.StartCoroutine()\" instead.")]
    public static object StartCoroutine(IEnumerator routine) => LoaderWrapper.StartCoroutine(routine);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="coroutineToken"></param>
    [Obsolete("Use \"LoaderWrapper.StopCoroutine()\" instead.")]
    public static void StopCoroutine(object coroutineToken) => LoaderWrapper.StopCoroutine(coroutineToken);

    /// <summary>
    /// Creates a rundown tag (<c>[R3-A5]</c>)
    /// </summary>
    /// <param name="rundowns">The rundown flags used to generate the tag.</param>
    /// <param name="generalizeLatest">Should the tag for the latest game version be replaced with <c>L</c> instead of the number?</param>
    /// <returns>The created rundown tag string</returns>
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

    /// <summary>
    /// Gets the title of the current rundown.
    /// </summary>
    /// <returns>Rundown title</returns>
    [Obsolete("Old game versions only.")]
    public static string GetRundownTitle() => GetRundownTitle(ArchiveMod.CurrentRundown);
    
    /// <summary>
    /// Gets the rundown title of the passed RundownID.
    /// </summary>
    /// <param name="rundown">The rundown id of which to get the title for</param>
    /// <returns>Rundown title</returns>
    [Obsolete("Old game versions only.")]
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

    /// <summary>
    /// Compute a SHA256 hash and return it as a hex string
    /// </summary>
    /// <param name="bytes">The bytes to hash</param>
    /// <returns>Hex string of the hash</returns>
    public static string GetHash(byte[] bytes)
    {
        using var hasher = SHA256.Create();
        
        var hashBytes = hasher.ComputeHash(bytes);

        var builder = new StringBuilder();
        foreach (var b in hashBytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
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

    /// <summary>
    /// Turns a generic Il2Cpp list into a managed one, but slow (because reflection)
    /// </summary>
    /// <param name="il2CppList">The Il2Cpp list</param>
    /// <param name="type">The lists generic type (For <c>List&lt;int&gt;</c> it would be <c>int</c>!)</param>
    /// <returns></returns>
    public static IList ToSystemListSlow(object il2CppList, Type type)
    {
        if(LoaderWrapper.IsGameIL2CPP())
        {
            var genListType = ImplementationManager.GameTypeByIdentifier("GenericList");

            var listType = genListType.MakeGenericType(type);

            var list = (IList) Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(type))!;

            // Invoke to get UnhollowerBaseLib.Il2CppArrayBase<T>
            var listAsEnumerable = (IEnumerable) listType.GetMethod("ToArray", AnyBindingFlagss)!.Invoke(il2CppList, Array.Empty<object>())!;

            var enumerator = listAsEnumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Current);
            }

            return list;
        }

        // Mono
        return (IList) il2CppList;
    }

    /// <summary> Extended string (not TMP tags) </summary>
    public const string EXTENDED_WITHOUT_TMP_TAGS = "://EXTENDED";
    /// <summary> Extended string </summary>
    public const string EXTENDED = "<color=orange><size=80%>" + EXTENDED_WITHOUT_TMP_TAGS + "</size></color>";

    // https://stackoverflow.com/a/11749642
    /// <summary>
    /// Get the roman numeral for a number.
    /// </summary>
    /// <param name="number">Number to turn roman.</param>
    /// <returns>Roman numeral string</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <remarks>
    /// <list>
    /// <item>Number must be greater than zero</item>
    /// <item>Number must be lower than 4000</item>
    /// </list>
    /// </remarks>
    /// <seealso href="https://stackoverflow.com/a/11749642"/>
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
    
    /// <summary>
    /// IsPowerOfTwo
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    /// <seealso href="https://stackoverflow.com/a/600306"/>
    public static bool IsPowerOfTwo(ulong x)
    {
        return (x != 0) && ((x & (x - 1)) == 0);
    }

    /// <summary>
    /// Replace a string case insensitively.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="search"></param>
    /// <param name="replacement"></param>
    /// <returns></returns>
    /// <seealso href="https://stackoverflow.com/a/6276029"/>
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

    /// <summary>
    /// Get a custom startup override for a given <c>RundownID</c>.
    /// </summary>
    /// <param name="currentRundownID"></param>
    /// <returns></returns>
    [Obsolete("Old game versions only.")]
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

    /// <summary>
    /// Coroutine that invokes an action on the next frame.
    /// </summary>
    /// <param name="action">Action to invoke</param>
    /// <returns>Coroutine</returns>
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

    /// <summary>
    /// Get latest rundown id
    /// </summary>
    /// <returns></returns>
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary> All rundown flags </summary>
    public const RundownFlags ALL_RUNDOWN_FLAGS = RundownFlags.RundownOne | RundownFlags.RundownTwo | RundownFlags.RundownThree
                                    | RundownFlags.RundownFour | RundownFlags.RundownFive | RundownFlags.RundownSix
                                    | RundownFlags.RundownSeven | RundownFlags.RundownAltOne | RundownFlags.RundownAltTwo
                                    | RundownFlags.RundownAltThree | RundownFlags.RundownAltFour | RundownFlags.RundownAltFive
                                    | RundownFlags.RundownAltSix | RundownFlags.RundownEight;

    /// <summary>
    /// Get latest rundown flags
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static RundownFlags GetLatestRundownFlags()
    {
        return LATEST_RUNDOWN_FLAGS;
    }

    /// <summary>
    /// Checks if a set of <c>RundownFlags</c> contains the specified <c>RundownID</c>
    /// </summary>
    /// <param name="flags">RundownFlags to check</param>
    /// <param name="id">RundownID</param>
    /// <returns><c>True</c> if flags contains id</returns>
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

    /// <summary>
    /// Get <c>RundownFlags</c> from to
    /// </summary>
    /// <param name="from">RundownFlags from (inclusive)</param>
    /// <param name="to">RundownFlags to (inclusive)</param>
    /// <returns>Flags from to</returns>
    /// <exception cref="ArgumentException"></exception>
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

    /// <summary>
    /// Check if any <c>RundownConstraint</c> matches.
    /// </summary>
    /// <param name="memberInfo">The member to check</param>
    /// <returns><c>True</c> if any matches</returns>
    /// <seealso cref="RundownConstraint"/>
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

    /// <summary>
    /// Check if any <c>BuildConstraint</c> matches.
    /// </summary>
    /// <param name="memberInfo">The member to check</param>
    /// <returns><c>True</c> if any matches</returns>
    /// <seealso cref="BuildConstraint"/>
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

    /// <summary>
    /// Get a HashSet containing all nested classes on a type.
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <returns>HashSet containing nested classes</returns>
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

    /// <summary>
    /// Create a SHA256 hash from an input string.
    /// </summary>
    /// <param name="input">String to hash</param>
    /// <returns>Hash value string</returns>
    public static string HashString(this string input)
    {
        using var sha256 = SHA256.Create();
        
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(inputBytes);
        
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
    }

    /// <summary>
    /// Parse a rundown key: "<c>Local_41</c>"
    /// </summary>
    /// <param name="rundownKey">Rundown key to parse</param>
    /// <param name="rundownID">Rundown datablock id</param>
    /// <returns><c>True</c> if successful</returns>
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

    /// <summary>
    /// Creates a new <c>Dictionary&lt;Language, string&gt;</c> with all Language enum values as keys and the values set to null.
    /// </summary>
    /// <returns>A new <c>Dictionary&lt;Language, string&gt;</c>.</returns>
    public static Dictionary<Language, string> GetEmptyLanguageDictionary()
    {
        var dict = new Dictionary<Language, string>();
        
        foreach (var lang in Enum.GetValues<Language>())
        {
            dict[lang] = null;
        }

        return dict;
    }
    
    /// <summary>
    /// Creates a new <c>Dictionary&lt;Language, T&gt;</c> with all Language enum values as keys and the values set to the output of the <paramref name="generator"/>.
    /// </summary>
    /// <param name="generator">The generator func to populate values.</param>
    /// <typeparam name="T">Generator output type.</typeparam>
    /// <returns></returns>
    public static Dictionary<Language, T> GetEmptyLanguageDictionary<T>(Func<T> generator)
    {
        var dict = new Dictionary<Language, T>();
        
        foreach (var lang in Enum.GetValues<Language>())
        {
            dict[lang] = generator.Invoke();
        }

        return dict;
    }
    
    /// <summary>
    /// Creates a new <c>Dictionary&lt;string, string&gt;</c> with all keys set to the enum values and the values all set to null.
    /// </summary>
    /// <typeparam name="T">The enum type to use.</typeparam>
    /// <returns>A new <c>Dictionary&lt;string, string&gt;</c>.</returns>
    public static Dictionary<string, string> GetEmptyEnumDictionary<T>() where T : struct, Enum
    {
        var dict = new Dictionary<string, string>();

        foreach (var value in Enum.GetValues<T>())
        {
            dict[value.ToString()] = null;
        }

        return dict;
    }
}