using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Loader;

namespace TheArchive.Utilities;

public static class PresenceFormatter
{
    private static readonly Dictionary<string, PresenceFormatProvider> _formatters = new Dictionary<string, PresenceFormatProvider>();


    private static readonly List<Type> _typesToCheckForProviders = new List<Type>();

    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= LoaderWrapper.CreateArSubLoggerInstance(nameof(PresenceFormatter), ConsoleColor.DarkMagenta);

    internal static void Setup()
    {
        Logger.Debug($"Setting up providers ...");

        foreach(var type in _typesToCheckForProviders)
        {
            foreach(var prop in type.GetProperties())
            {
                try
                {
                    var formatProviderAttribute = prop.GetCustomAttribute<PresenceFormatProvider>();
                    if (formatProviderAttribute != null)
                    {
                        formatProviderAttribute.SetPropertyInfo(prop);
                        RegisterFormatter(formatProviderAttribute);
                    }
                }
                catch(Exception ex)
                {
                    Logger.Exception(ex);
                }
            }
        }

        foreach(var former in _formatters.Where(kvp => kvp.Value is FallbackPresenceFormatProvider && !((FallbackPresenceFormatProvider)kvp.Value).NoNotImplementedWarning))
        {
            Logger.Warning($"Identifier \"{former.Key}\" has not been implemented! Using Fallback default values!");
        }

    }

    public static void RegisterAllPresenceFormatProviders(this Type type, bool throwOnDuplicate = true)
    {
        if (type == null) throw new ArgumentException("Type must not be null!");
        if (_typesToCheckForProviders.Contains(type))
        {
            if (throwOnDuplicate)
                throw new ArgumentException($"Duplicate Type registered: \"{type?.FullName}\"");
            else return;
        }

        _typesToCheckForProviders.Add(type);
    }

    public static void RegisterFormatter(PresenceFormatProvider pfp)
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

    internal static object Get(string identifier)
    {
        _formatters.TryGetValue(identifier, out var former);

        if (former == null) return null;

        return former.GetValue();
    }

    internal static T Get<T>(string identifier)
    {
        _formatters.TryGetValue(identifier, out var former);

        if (former == null) return default(T);

        if (!former.PropertyType.IsAssignableFrom(typeof(T)) && typeof(T) != typeof(string)) throw new ArgumentException($"The property at identifier \"{identifier}\" is not declared as Type \"{typeof(T).Name}\"!");

        return (T) former.GetValue();
    }

    public static string Format(this string formatString, params (string search, string replace)[] extraFormatters) => FormatPresenceString(formatString, extraFormatters);

    public static string FormatPresenceString(string formatString) => FormatPresenceString(formatString, null);

    public static string FormatPresenceString(string formatString, params (string search, string replace)[] extraFormatters) => FormatPresenceString(formatString, true, extraFormatters);

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

    [AttributeUsage(AttributeTargets.Property)]
    public class PresenceFormatProvider : Attribute
    {
        public string Identifier { get; private set; }

        public bool IsValid => _propertyInfo != null;
        public Type PropertyType => PropertyInfo.PropertyType;
        public string DebugIdentifier => $"{PropertyInfo.DeclaringType.Name}.{PropertyInfo.Name} (ASM:{PropertyInfo.DeclaringType.Assembly.GetName().Name})";

        private PropertyInfo _propertyInfo;
        internal PropertyInfo PropertyInfo => _propertyInfo ?? throw new Exception($"PropertyInfo not set on {nameof(PresenceFormatProvider)} with ID \"{Identifier}\"!");

        public PresenceFormatProvider(string identifier)
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

        public object GetValue()
        {
            return PropertyInfo.GetValue(null);
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    internal class FallbackPresenceFormatProvider : PresenceFormatProvider
    {
        public bool NoNotImplementedWarning { get; private set; } = false;

        public FallbackPresenceFormatProvider(string identifier, bool noNotImplementedWarning = false) : base(identifier)
        {
            NoNotImplementedWarning = noNotImplementedWarning;
        }
    }

}