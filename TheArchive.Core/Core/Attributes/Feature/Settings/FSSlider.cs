using System;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FSSlider : Attribute
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public FSSlider(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
