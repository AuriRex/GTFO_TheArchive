using System.Diagnostics.CodeAnalysis;
using TheArchive.Core.Models;
using UnityEngine;

namespace TheArchive.Utilities;

/// <summary>
/// SColor related extension methods.
/// </summary>
/// <seealso cref="SColor"/>
public static class SColorExtensions
{
    /// <summary>
    /// Convert a <c>SColor</c> to a <c>UnityEngine.Color</c>.
    /// </summary>
    /// <param name="col">The SColor to convert to a unity color.</param>
    /// <returns>A unity color.</returns>
    public static Color ToUnityColor(this SColor col)
    {
        return new Color
        {
            r = col.R,
            g = col.G,
            b = col.B,
            a = col.A,
        };
    }

    /// <summary>
    /// Convert a <c>SColor</c> to a <c>UnityEngine.Color</c>.<br/>
    /// Null preserving.
    /// </summary>
    /// <param name="col">The SColor to convert to a unity color.</param>
    /// <returns>A unity color.</returns>
    public static Color? ToUnityColor(this SColor? col)
    {
        return col?.ToUnityColor();
    }

    /// <summary>
    /// Convert a <c>UnityEngine.Color</c> to a <c>SColor</c>.
    /// </summary>
    /// <param name="col">The unity color to convert to a SColor.</param>
    /// <returns>A SColor</returns>
    public static SColor ToSColor(this Color col)
    {
        return new SColor
        {
            R = col.r,
            G = col.g,
            B = col.b,
            A = col.a,
        };
    }

    /// <summary>
    /// Create a <c>SColor</c> from a hex string.
    /// </summary>
    /// <param name="col">The color hex string.</param>
    /// <returns>The created SColor.</returns>
    /// <remarks>
    /// Leading hash is optional.
    /// </remarks>
    public static SColor FromHexString(string col)
    {
        col = EnsureLeadingHash(col);

        if (ColorUtility.TryParseHtmlString(col, out var color))
        {
            return color.ToSColor();
        }
        return SColor.WHITE;
    }

    /// <summary>
    /// Appends a <c>#</c> at the start of the string if it doesn't do so already.
    /// </summary>
    /// <param name="hexString">The hex string.</param>
    /// <returns>The hex string prefixed with a <c>#</c>.</returns>
    public static string EnsureLeadingHash(string hexString)
    {
        if (!hexString.StartsWith("#"))
            hexString = $"#{hexString}";
        return hexString;
    }

    /// <summary>
    /// Convert a <c>SColor</c> to a hex string.
    /// </summary>
    /// <param name="col">The color to convert.</param>
    /// <returns>The hex string representing the SColor.</returns>
    public static string ToHexString(this SColor col)
    {
        return ToHexString(col.ToUnityColor());
    }

    /// <summary>
    /// Convert a <c>UnityEngine.Color</c> to a hex string.
    /// </summary>
    /// <param name="col">The color to convert.</param>
    /// <returns>The hex string representing the unity color.</returns>
    public static string ToHexString(this Color col)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(col)}";
    }

    /// <summary>
    /// Convert a <c>SColor</c> to a three character hex string.
    /// </summary>
    /// <param name="col">The color to convert.</param>
    /// <returns>The three character hex string representing the SColor.</returns>
    public static string ToShortHexString(this SColor col)
    {
        return $"#{ComponentToHex(col.R)}{ComponentToHex(col.G)}{ComponentToHex(col.B)}";
    }

    /// <summary>
    /// Convert a <c>UnityEngine.Color</c> to a three character hex string.
    /// </summary>
    /// <param name="col">The color to convert.</param>
    /// <returns>The three character hex string representing the unity color.</returns>
    public static string ToShortHexString(this Color col)
    {
        return $"#{ComponentToHex(col.r)}{ComponentToHex(col.g)}{ComponentToHex(col.b)}";
    }

    /// <summary>
    /// Convert a color component to a single hexadecimal character string.
    /// </summary>
    /// <param name="component">The component value (0 to 1).</param>
    /// <returns>The single hexadecimal character string.</returns>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static string ComponentToHex(float component)
    {
        return $"{Mathf.Clamp((int)(component * 16f), 0, 15):X1}";
    }
}