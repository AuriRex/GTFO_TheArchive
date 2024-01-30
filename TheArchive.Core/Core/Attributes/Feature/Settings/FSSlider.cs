using System;
using TheArchive.Core.Localization;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FSSlider : Attribute
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public SliderStyle Style { get; set; }
        public RoundTo Rounding { get; set; }

        public FSSlider(float min, float max, SliderStyle style = SliderStyle.FloatPercent, RoundTo rounding = RoundTo.TwoDecimal)
        {
            Min = min;
            Max = max;

            Style = style;
            Rounding = rounding;
        }

        [Localized]
        public enum SliderStyle
        {
            FloatPercent,
            FloatNoDecimal,
            FloatOneDecimal,
            FloatTwoDecimal,
            [Obsolete("Not yet implemented properly! Use a float value instead!")]
            IntMinMax
        }

        [Localized]
        public enum RoundTo
        {
            NoRounding = -1,
            NoDecimal = 0,
            OneDecimal,
            TwoDecimal,
            ThreeDecimal,
        }
    }
}
