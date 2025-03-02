using System.Reflection;
using TheArchive.Core.FeaturesAPI.Components;

namespace TheArchive.Core;

internal class ArchiveContractResolver : DefaultContractResolver
{
    public static readonly ArchiveContractResolver Instance = new ArchiveContractResolver();

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        if (typeof(ISettingsComponent).IsAssignableFrom(property.PropertyType))
        {
            property.Ignored = true;
        }

        return property;
    }
}