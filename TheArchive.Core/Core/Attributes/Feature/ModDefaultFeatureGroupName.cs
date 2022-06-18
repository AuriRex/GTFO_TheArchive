using System;

namespace TheArchive.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ModDefaultFeatureGroupName : Attribute
    {
        public string DefaultGroupName { get; private set; }

        public ModDefaultFeatureGroupName(string defaultGroupName)
        {
            DefaultGroupName = defaultGroupName;
        }
    }
}
