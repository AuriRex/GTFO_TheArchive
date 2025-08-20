using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Loader;

namespace TheArchive.Utilities;

/// <summary>
/// A custom tag based string formatter.
/// </summary>
/// <remarks>
/// <list>
/// <item>Tags are in the following format: <c>%Identifier%</c></item>
/// <item>Tags can be registered by decorating a public static property with the <c>[PresenceFormatProvider]</c> attribute and registering the containing type using <see cref="RegisterAllPresenceFormatProviders"/>.</item>
/// </list>
/// </remarks>
public static class PresenceFormatter
{
    private static readonly Dictionary<string, PresenceFormatProvider> _formatters = new();

    private static readonly List<Type> _typesToCheckForProviders = new();

    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= LoaderWrapper.CreateArSubLoggerInstance(nameof(PresenceFormatter), ConsoleColor.DarkMagenta);

    /// <summary>
    /// TODO: Not have this public :)
    /// </summary>
    public static void Setup()
    {
        Logger.Debug($"Setting up providers ...");

        foreach(var type in _typesToCheckForProviders)
        {
            CheckTypeForProviders(type);
        }

        foreach(var former in _formatters
                    .Where(kvp => kvp.Value is FallbackPresenceFormatProvider provider
                                  && !provider.NoNotImplementedWarning))
        {
            Logger.Warning($"Identifier \"{former.Key}\" has not been implemented! Using Fallback default values!");
        }
    }

    private static void CheckTypeForProviders(Type type)
    {
        foreach(var prop in type.GetProperties())
        {
            try
            {
                var formatProviderAttribute = prop.GetCustomAttribute<PresenceFormatProvider>();
                if (formatProviderAttribute == null)
                    continue;
                formatProviderAttribute.SetPropertyInfo(prop);
                RegisterFormatter(formatProviderAttribute);
            }
            catch(Exception ex)
            {
                Logger.Exception(ex);
            }
        }
    }

    /// <summary>
    /// Register or queue a type for public presence format provider properties inspection.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="throwOnDuplicate">Should an exception be thrown if a duplicate type were to be checked?</param>
    /// <exception cref="ArgumentException">Type is null or a duplicate.</exception>
    public static void RegisterAllPresenceFormatProviders(this Type type, bool throwOnDuplicate = true)
    {
        if (type == null) throw new ArgumentException("Type must not be null!");
        if (_typesToCheckForProviders.Contains(type))
        {
            if (throwOnDuplicate)
                throw new ArgumentException($"Duplicate Type registered: \"{type.FullName}\"");
            return;
        }

        _typesToCheckForProviders.Add(type);

        if (_formatters.Count <= 0)
            return;
        
        // Setup has already executed, run checks immediately.
        Logger.Debug($"Late call of {nameof(RegisterAllPresenceFormatProviders)} for {type.FullName} - running checks now.");
        CheckTypeForProviders(type);
    }

    private static void RegisterFormatter(PresenceFormatProvider pfp)
    {
        if (!pfp.IsValid)
            return;

        bool fallbackOverridden = false;

        if(_formatters.TryGetValue(pfp.Identifier, out var registeredPfp))
        {
            if (pfp is FallbackPresenceFormatProvider) return;

            if(registeredPfp is FallbackPresenceFormatProvider)
            {
                _formatters.Remove(pfp.Identifier);
                fallbackOverridden = true;
            }
            else
            {
                throw new ArgumentException($"Duplicate formatter identifier: \"{pfp.Identifier}\" (\"{pfp.DebugIdentifier}\")");
            }
        }

        _formatters.Add(pfp.Identifier, pfp);
        Logger.Debug($"{(fallbackOverridden ? " (Fallback Overridden)" : ((pfp is FallbackPresenceFormatProvider) ? " (Fallback)" : string.Empty))} Registered: \"{pfp.Identifier}\" => {pfp.DebugIdentifier}");
    }

    /// <summary>
    /// Get the value of the presence format provider with the given identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the format provider.</param>
    /// <returns>The value.</returns>
    public static object Get(string identifier)
    {
        _formatters.TryGetValue(identifier, out var former);

        if (former == null) return null;

        return former.GetValue();
    }

    /// <summary>
    /// Get the value of the presence format provider with the given identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the format provider.</param>
    /// <typeparam name="T">The type to cast to.</typeparam>
    /// <returns>The value.</returns>
    /// <exception cref="ArgumentException">Type doesn't match or isn't castable to string.</exception>
    public static T Get<T>(string identifier)
    {
        _formatters.TryGetValue(identifier, out var former);

        if (former == null) return default(T);

        if (!former.PropertyType.IsAssignableFrom(typeof(T)) && typeof(T) != typeof(string)) throw new ArgumentException($"The property at identifier \"{identifier}\" is not declared as Type \"{typeof(T).Name}\"!");

        return (T) former.GetValue();
    }

    /// <summary>
    /// Format a string using all loaded format providers to replace <c>%Identifier%</c> tags.
    /// </summary>
    /// <param name="formatString">The string to format.</param>
    /// <param name="extraFormatters">Any extra format tags to replace.</param>
    /// <returns>The formatted string.</returns>
    /// <remarks>
    /// Removes all TextMeshPro rich text tags.
    /// </remarks>
    public static string Format(this string formatString, params (string search, string replace)[] extraFormatters) => FormatPresenceString(formatString, extraFormatters);

    /// <summary>
    /// Format a string using all loaded format providers to replace <c>%Identifier%</c> tags.
    /// </summary>
    /// <param name="formatString">The string to format.</param>
    /// <returns>The formatted string.</returns>
    /// <remarks>
    /// Removes all TextMeshPro rich text tags.
    /// </remarks>
    public static string FormatPresenceString(string formatString) => FormatPresenceString(formatString, null);

    /// <summary>
    /// Format a string using all loaded format providers to replace <c>%Identifier%</c> tags.
    /// </summary>
    /// <param name="formatString">The string to format.</param>
    /// <param name="extraFormatters">Any extra format tags to replace.</param>
    /// <returns>The formatted string.</returns>
    /// <remarks>
    /// Removes all TextMeshPro rich text tags.
    /// </remarks>
    public static string FormatPresenceString(string formatString, params (string search, string replace)[] extraFormatters) => FormatPresenceString(formatString, true, extraFormatters);

    /// <summary>
    /// Format a string using all loaded format providers to replace <c>%Identifier%</c> tags.
    /// </summary>
    /// <param name="formatString">The string to format.</param>
    /// <param name="stripAllTMPTags">Should TextMeshPro rich text tags be removed?</param>
    /// <param name="extraFormatters">Any extra format tags to replace.</param>
    /// <returns>The formatted string.</returns>
    public static string FormatPresenceString(string formatString, bool stripAllTMPTags = true, params (string search, string replace)[] extraFormatters)
    {
        string formatted = formatString;

        foreach(var former in _formatters)
        {
            if(formatted.Contains($"%{former.Key}%"))
                formatted = formatted.ReplaceCaseInsensitive($"%{former.Key}%", former.Value.GetValue()?.ToString() ?? "null");
        }

        if(extraFormatters != null)
        {
            foreach (var extraFormer in extraFormatters)
            {
                if (formatted.Contains($"%{extraFormer.search}%"))
                    formatted = formatted.ReplaceCaseInsensitive($"%{extraFormer.search}%", extraFormer.replace);
            }
        }

        if(stripAllTMPTags)
            return Utils.StripTMPTagsRegex(formatted.Trim());

        return formatted.Trim();
    }

    /// <summary>
    /// Marks a property as a presence format provider.
    /// </summary>
    /// <remarks>
    /// The containing type has to be registered using <see cref="PresenceFormatter.RegisterAllPresenceFormatProviders"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class PresenceFormatProvider : Attribute
    {
        /// <summary>
        /// The identifier of this format provider.<br/>
        /// (Usually the property name)
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// If this format provider is valid.
        /// </summary>
        public bool IsValid => _propertyInfo != null;
        
        /// <summary>
        /// The property type of this format provider.
        /// </summary>
        public Type PropertyType => PropertyInfo.PropertyType;
        
        /// <summary>
        /// Identifier used for debugging.
        /// </summary>
        public string DebugIdentifier => $"{PropertyInfo.DeclaringType!.Name}.{PropertyInfo.Name} (ASM:{PropertyInfo.DeclaringType.Assembly.GetName().Name})";

        private PropertyInfo _propertyInfo;
        internal PropertyInfo PropertyInfo => _propertyInfo ?? throw new Exception($"PropertyInfo not set on {nameof(PresenceFormatProvider)} with ID \"{Identifier}\"!");

        /// <summary>
        /// PresenceFormatProvider constructor.
        /// </summary>
        /// <param name="identifier">The identifier of this format provider.</param>
        public PresenceFormatProvider([CallerMemberName] string identifier = "")
        {
            Identifier = identifier;
        }

        internal void SetPropertyInfo(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentException($"Provided {nameof(PresenceFormatProvider)} {nameof(PropertyInfo)} may not be null! (ID:{Identifier})");
            if (!propertyInfo.GetGetMethod()?.IsStatic ?? true)
                throw new ArgumentException($"Provided {nameof(PresenceFormatProvider)} {nameof(PropertyInfo)} has to implement a static get method! (ID:{Identifier})");

            _propertyInfo = propertyInfo;
        }

        /// <summary>
        /// Get the value of this format provider.
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            return PropertyInfo.GetValue(null);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Marks a property as a fallback presence format provider.<br/>
    /// This variant can be overriden by a non-fallback presence format provider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FallbackPresenceFormatProvider : PresenceFormatProvider
    {
        /// <summary>
        /// Should the not-implemented-warning be ignored?
        /// </summary>
        public bool NoNotImplementedWarning { get; }

        /// <summary>
        /// FallbackPresenceFormatProvider constructor.
        /// </summary>
        /// <param name="identifier">The identifier of this format provider.</param>
        /// <param name="noNotImplementedWarning">Should the not-implemented-warning be ignored?</param>
        public FallbackPresenceFormatProvider(string identifier, bool noNotImplementedWarning = false) : base(identifier)
        {
            NoNotImplementedWarning = noNotImplementedWarning;
        }
    }

}