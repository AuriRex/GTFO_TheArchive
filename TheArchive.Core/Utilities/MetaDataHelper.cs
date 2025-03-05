// This file is licensed under the LGPL 2.1 LICENSE
// See LICENSE_BepInEx in the projects root folder
// Original code from https://github.com/BepInEx/BepInEx

using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using TheArchive.Core.Attributes;

namespace TheArchive.Utilities;

/// <summary>
///     Helper class to use for retrieving metadata about a module, defined as attributes.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class MetadataHelper
{
    internal static IEnumerable<CustomAttribute> GetCustomAttributes<T>(TypeDefinition td, bool inherit) where T : Attribute
    {
        var result = new List<CustomAttribute>();
        var type = typeof(T);
        var currentType = td;
        
        do
        {
            result.AddRange(currentType!.CustomAttributes.Where(ca => ca.AttributeType.FullName == type.FullName));
            currentType = currentType.BaseType?.Resolve();
        }
        while (inherit && currentType?.FullName != typeof(object).FullName);
        
        return result;
    }

    /// <summary>
    ///     Retrieves the ArchiveModule metadata from a module type.
    /// </summary>
    /// <param name="moduleType">The module type.</param>
    /// <returns>The ArchiveModule metadata of the module type.</returns>
    public static ArchiveModule GetMetadata(Type moduleType)
    {
        var attributes = moduleType.GetCustomAttributes(typeof(ArchiveModule), false);
        
        if (attributes.Length == 0)
            return null;
        
        return (ArchiveModule) attributes[0];
    }

    /// <summary>
    ///     Retrieves the ArchiveModule metadata from a module instance.
    /// </summary>
    /// <param name="module">The module instance.</param>
    /// <returns>The ArchiveModule metadata of the module instance.</returns>
    public static ArchiveModule GetMetadata(object module)
    {
        return GetMetadata(module.GetType());
    }

    /// <summary>
    ///     Gets the specified attributes of a type, if they exist.
    /// </summary>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <param name="moduleType">The module type.</param>
    /// <returns>The attributes of the type, if existing.</returns>
    public static T[] GetAttributes<T>(Type moduleType) where T : Attribute
    {
        return (T[])moduleType.GetCustomAttributes(typeof(T), true);
    }

    /// <summary>
    ///     Gets the specified attributes of an assembly, if they exist.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <returns>The attributes of the type, if existing.</returns>
    public static T[] GetAttributes<T>(Assembly assembly) where T : Attribute
    {
        return (T[])assembly.GetCustomAttributes(typeof(T), true);
    }

    /// <summary>
    ///     Gets the specified attributes of an instance, if they exist.
    /// </summary>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <param name="module">The module instance.</param>
    /// <returns>The attributes of the instance, if existing.</returns>
    public static IEnumerable<T> GetAttributes<T>(object module) where T : Attribute
    {
        return GetAttributes<T>(module.GetType());
    }

    /// <summary>
    ///     Gets the specified attributes of a reflection metadata type, if they exist.
    /// </summary>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <param name="member">The reflection metadata instance.</param>
    /// <returns>The attributes of the instance, if existing.</returns>
    public static T[] GetAttributes<T>(MemberInfo member) where T : Attribute
    {
        return (T[])member.GetCustomAttributes(typeof(T), true);
    }

    /// <summary>
    ///     Retrieves the dependencies of the specified module type.
    /// </summary>
    /// <param name="module">The module type.</param>
    /// <returns>A list of all module types that the specified module type depends upon.</returns>
    public static IEnumerable<ArchiveDependency> GetDependencies(Type module)
    {
        return module.GetCustomAttributes(typeof(ArchiveDependency), true).Cast<ArchiveDependency>();
    }
}