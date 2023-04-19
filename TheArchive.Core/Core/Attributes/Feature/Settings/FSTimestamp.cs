using System;
using TheArchive.Utilities;

namespace TheArchive.Core.Attributes.Feature.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FSTimestamp : Attribute
    {
        public string Format { get; private set; } = "U";

        public FSTimestamp(string customFormat = "")
        {
            try
            {
                Format = customFormat;

                DateTime.Now.ToString(Format);
            }
            catch (Exception)
            {
                ArchiveLogger.Warning($"A {nameof(FSTimestamp)}s custom format threw an exception! Format String: \"{Format}\"");
                Format = "U";
            }
        }
    }
}
