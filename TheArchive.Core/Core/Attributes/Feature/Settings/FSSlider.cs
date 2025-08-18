using System;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Localization;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Used on <c>NumberSettings</c> to convert the string input box into a slider.
/// </summary>
/// <seealso cref="NumberSetting"/>
/// <remarks>
/// <list>
/// <item>Use on a member of a type that's used by the feature settings system. (<c>[FeatureConfig]</c>)</item>
/// </list>
/// </remarks>
/// <example><code>
/// public class MyFeature : Feature
/// {
///     [FeatureConfig]
///     public static MyCustomSettings Settings { get; set; }
///
///     public class MyCustomSettings
///     {
///         // Creates a slider instead of an input field.
///         [FSSlider(0f, 1f)]
///         public float MyCustomFloat { get; set; } = 0.2f;
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public class FSSlider : Attribute
{
    /// <summary>
    /// The minimum value.
    /// </summary>
    public float Min { get; set; }
    
    /// <summary>
    /// The maximum value.
    /// </summary>
    public float Max { get; set; }

    /// <summary>
    /// The slider style.
    /// </summary>
    public SliderStyle Style { get; set; }
    
    /// <summary>
    /// How to handle rounding.
    /// </summary>
    public RoundTo Rounding { get; set; }

    /// <summary>
    /// Creates a slider in the range from <paramref name="min"/> to <paramref name="max"/> value.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="style">The slider style.</param>
    /// <param name="rounding">How to handle rounding.</param>
    public FSSlider(float min, float max, SliderStyle style = SliderStyle.FloatPercent, RoundTo rounding = RoundTo.TwoDecimal)
    {
        Min = min;
        Max = max;

        Style = style;
        Rounding = rounding;
    }

    /// <summary>
    /// The display style of the slider.
    /// </summary>
    [Localized]
    public enum SliderStyle
    {
        /// <summary> Percentage </summary>
        FloatPercent,
        /// <summary> Float value, no decimals shown. </summary>
        FloatNoDecimal,
        /// <summary> Float value, one decimal shown. </summary>
        FloatOneDecimal,
        /// <summary> Float value, two decimals shown. </summary>
        FloatTwoDecimal,
        /// <summary> Int value. </summary>
        [Obsolete("Not yet implemented properly! Use a float value instead!")]
        IntMinMax
    }

    /// <summary>
    /// Rounding behaviour.
    /// </summary>
    [Localized]
    public enum RoundTo
    {
        /// <summary> No rounding - keep as is. </summary>
        NoRounding = -1,
        /// <summary> No decimals. </summary>
        NoDecimal = 0,
        /// <summary> One decimal. </summary>
        OneDecimal,
        /// <summary> Two decimals. </summary>
        TwoDecimal,
        /// <summary> Three decimals. </summary>
        ThreeDecimal,
    }
}