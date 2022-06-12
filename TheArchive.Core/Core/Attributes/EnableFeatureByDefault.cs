using System;

namespace TheArchive.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EnableFeatureByDefault : Attribute
    {
        public bool ShouldEnableByDefault { get; private set; }

        public EnableFeatureByDefault(bool shouldEnable)
        {
            ShouldEnableByDefault = shouldEnable;
        }
    }
}
