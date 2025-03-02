using Mono.Cecil;
using System;
using System.Linq;
using TheArchive.Utilities;
using Version = SemanticVersioning.Version;

namespace TheArchive.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ArchiveModule : Attribute
    {
        public string GUID { get; protected set; }
        public string Name { get; protected set; }
        public Version Version { get; protected set; }

        public ArchiveModule(string GUID, string Name, string Version)
        {
            this.GUID = GUID;
            this.Name = Name;
            this.Version = TryParseLongVersion(Version);
        }

        private static Version TryParseLongVersion(string version)
        {
            if (Version.TryParse(version, out var v))
            {
                return v;
            }
            try
            {
                System.Version longVersion = new System.Version(version);
                return new Version(longVersion.Major, longVersion.Minor, (longVersion.Build != -1) ? longVersion.Build : 0, null, null);
            }
            catch
            {
            }
            return null;
        }

        internal static ArchiveModule FromCecilType(TypeDefinition td)
        {
            CustomAttribute attr = MetadataHelper.GetCustomAttributes<ArchiveModule>(td, false).FirstOrDefault();
            if (attr == null)
            {
                return null;
            }
            return new ArchiveModule((string)attr.ConstructorArguments[0].Value, (string)attr.ConstructorArguments[1].Value, (string)attr.ConstructorArguments[2].Value);
        }
    }
}