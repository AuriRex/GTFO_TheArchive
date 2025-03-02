using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;

namespace TheArchive.Features.Dev
{
    [HideInModSettings, DoNotSaveToConfig]
    public class SettingsDebug : Feature
    {
        public override string Name => nameof(SettingsDebug);

        public override FeatureGroup Group => FeatureGroups.Dev;

        [FeatureConfig]
        public static SettingsDebugSettings Settings { get; set; }

        public class SettingsDebugSettings
        {
            [FSDisplayName("Slider Settings")]
            public Sliders SliderSettings { get; set; } = new Sliders();

            public class Sliders
            {
                [FSSlider(0, 1, FSSlider.SliderStyle.FloatPercent)]
                public float SliderPercent0_1 { get; set; } = 0.5f;

                [FSSlider(0.1f, 2f, FSSlider.SliderStyle.FloatTwoDecimal, FSSlider.RoundTo.TwoDecimal)]
                public float SliderFloat01_20 { get; set; } = 0.5f;

                [FSSlider(0.1f, 2f, FSSlider.SliderStyle.FloatOneDecimal, FSSlider.RoundTo.OneDecimal)]
                public float SliderFloat01_20_OneDec { get; set; } = 0.5f;

                [FSSlider(0f, 100f, FSSlider.SliderStyle.FloatNoDecimal, FSSlider.RoundTo.NoDecimal)]
                public float SliderFloat0_100_NoDec { get; set; } = 50;

#pragma warning disable CS0618 // Type or member is obsolete
                [FSSlider(0f, 100f, FSSlider.SliderStyle.IntMinMax)]
#pragma warning restore CS0618 // Type or member is obsolete
                public int SliderInt0_100_NoDec { get; set; } = 50;

                [FSSlider(0, 3, FSSlider.SliderStyle.FloatPercent)]
                public float SliderPercent0_3 { get; set; } = 0.5f;

                [FSSlider(0.2f, 2, FSSlider.SliderStyle.FloatPercent)]
                public float SliderPercent02_20 { get; set; } = 0.5f;
            }
            
        }
    }
}
