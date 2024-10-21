using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Core.Attributes;

namespace TheArchive.Utilities;

public static class MetadataHelper
{
    internal static IEnumerable<CustomAttribute> GetCustomAttributes<T>(TypeDefinition td, bool inherit) where T : Attribute
    {
        List<CustomAttribute> result = new List<CustomAttribute>();
        Type type = typeof(T);
        TypeDefinition currentType = td;
        do
        {
            result.AddRange(currentType.CustomAttributes.Where((CustomAttribute ca) => ca.AttributeType.FullName == type.FullName));
            TypeReference baseType = currentType.BaseType;
            currentType = ((baseType != null) ? baseType.Resolve() : null);
        }
        while (inherit && ((currentType != null) ? currentType.FullName : null) != typeof(object).FullName);
        return result;
    }

    public static ArchiveModule GetMetadata(Type pluginType)
    {
        object[] attributes = pluginType.GetCustomAttributes(typeof(ArchiveModule), false);
        if (attributes.Length == 0)
        {
            return null;
        }
        return (ArchiveModule)attributes[0];
    }

    public static ArchiveModule GetMetadata(object plugin)
    {
        return MetadataHelper.GetMetadata(plugin.GetType());
    }

    public static T[] GetAttributes<T>(Type pluginType) where T : Attribute
    {
        return (T[])pluginType.GetCustomAttributes(typeof(T), true);
    }

    public static T[] GetAttributes<T>(Assembly assembly) where T : Attribute
    {
        return (T[])assembly.GetCustomAttributes(typeof(T), true);
    }

    public static IEnumerable<T> GetAttributes<T>(object plugin) where T : Attribute
    {
        return MetadataHelper.GetAttributes<T>(plugin.GetType());
    }

    public static T[] GetAttributes<T>(MemberInfo member) where T : Attribute
    {
        return (T[])member.GetCustomAttributes(typeof(T), true);
    }

    public static IEnumerable<ArchiveDependency> GetDependencies(Type plugin)
    {
        return plugin.GetCustomAttributes(typeof(ArchiveDependency), true).Cast<ArchiveDependency>();
    }
}