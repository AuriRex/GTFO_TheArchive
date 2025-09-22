using System;
using System.Collections.Generic;

namespace TheArchive.Utilities;

/// <summary>
/// Enum related extension methods.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Converts a list of enum values to a single flags enum value by combining them with bitwise OR operation.
    /// </summary>
    /// <typeparam name="T">The enum type that must be decorated with [Flags] attribute.</typeparam>
    /// <param name="enums">The list of enum values to combine.</param>
    /// <returns>A single enum value that represents the combination of all input values.</returns>
    /// <exception cref="ArgumentException">Thrown when any item in the list is not of type T.</exception>
    /// <example>
    /// <code>
    /// [Flags]
    /// public enum FileAccess
    /// {
    ///     None = 0,
    ///     Read = 1,
    ///     Write = 2,
    ///     Execute = 4
    /// }
    /// 
    /// var permissions = new List&lt;FileAccess&gt; { FileAccess.Read, FileAccess.Write };
    /// var combined = permissions.ToFlags(); // Result: FileAccess.Read | FileAccess.Write
    /// </code>
    /// </example>
    public static T ToFlags<T>(this List<T> enums) where T : struct, Enum
    {
        ulong result = 0;

        foreach (var e in enums)
        {
            if (e is T flag)
            {
                result |= Convert.ToUInt64(flag);
            }
            else
            {
                throw new ArgumentException($"All items in the list must be of type {typeof(T).Name}", nameof(enums));
            }
        }

        return (T)Enum.ToObject(typeof(T), result);
    }

    /// <summary>
    /// Gets the highest single flag value from a flags enum. If the enum contains multiple flags,
    /// this method returns the flag with the highest bit position.
    /// </summary>
    /// <typeparam name="T">The enum type that must be decorated with [Flags] attribute.</typeparam>
    /// <param name="value">The flags enum value to analyze.</param>
    /// <returns>The highest single flag value, or the original value if it's already a single flag or zero.</returns>
    /// <example>
    /// <code>
    /// [Flags]
    /// public enum FileAccess
    /// {
    ///     None = 0,
    ///     Read = 1,
    ///     Write = 2,
    ///     Execute = 4,
    ///     Delete = 8
    /// }
    /// 
    /// var permissions = FileAccess.Read | FileAccess.Write | FileAccess.Execute;
    /// var highest = permissions.GetHighestLevel(); // Result: FileAccess.Execute (4)
    /// 
    /// var singleFlag = FileAccess.Read;
    /// var highestSingle = singleFlag.GetHighestLevel(); // Result: FileAccess.Read (1)
    /// </code>
    /// </example>
    public static T GetHighestLevel<T>(this T value) where T : struct, Enum
    {
        var enumValue = Convert.ToUInt64(value);

        // If the value is 0, return it as is
        if (enumValue == 0)
        {
            return value;
        }

        // Find the highest bit set
        ulong highestBit = 0;
        ulong temp = enumValue;

        while (temp != 0)
        {
            highestBit = temp & (~temp + 1); // Get the lowest set bit
            temp &= temp - 1; // Remove the lowest set bit
        }

        return (T)Enum.ToObject(typeof(T), highestBit);
    }

    /// <summary>
    /// Gets all individual flags that are set in the flags enum value.
    /// </summary>
    /// <typeparam name="T">The enum type that must be decorated with [Flags] attribute.</typeparam>
    /// <param name="value">The flags enum value to decompose.</param>
    /// <returns>An enumerable of individual flags that are set in the input value.</returns>
    /// <example>
    /// <code>
    /// [Flags]
    /// public enum FileAccess
    /// {
    ///     None = 0,
    ///     Read = 1,
    ///     Write = 2,
    ///     Execute = 4
    /// }
    /// 
    /// var permissions = FileAccess.Read | FileAccess.Execute;
    /// var individualFlags = permissions.GetFlags().ToList(); 
    /// // Result: [FileAccess.Read, FileAccess.Execute]
    /// </code>
    /// </example>
    public static IEnumerable<T> GetFlags<T>(this T value) where T : struct, Enum
    {
        var enumValue = Convert.ToUInt64(value);

        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagValue = Convert.ToUInt64(flag);
            if (flagValue != 0 && (enumValue & flagValue) == flagValue)
            {
                yield return flag;
            }
        }
    }

    /// <summary>
    /// Gets all individual flags that are set in the flags enum value.
    /// Non-generic version that works with any enum type.
    /// </summary>
    /// <param name="value">The flags enum value to decompose.</param>
    /// <returns>An enumerable of individual flags that are set in the input value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IEnumerable<object> GetFlags(object value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        var enumValue = Convert.ToUInt64(value);
        var enumType = value.GetType();

        foreach (var flag in Enum.GetValues(enumType))
        {
            var flagValue = Convert.ToUInt64(flag);
            if (flagValue != 0 && (enumValue & flagValue) == flagValue)
            {
                yield return flag;
            }
        }
    }
}
