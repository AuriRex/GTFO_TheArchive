using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Core.Managers;

namespace TheArchive.Utilities
{
    public static class PresenceFormatter
    {
        private static Dictionary<string, PresenceFormatProvider> _formatters = new Dictionary<string, PresenceFormatProvider>();


        private static List<Type> _typesToCheckForProviders = new List<Type>();

        internal static void Setup()
        {
            ArchiveLogger.Debug($"[{nameof(PresenceFormatter)}] Setting up ...");
            typeof(PresenceManager).RegisterAllPresenceFormatProviders(false);

            foreach(var type in _typesToCheckForProviders)
            {
                foreach(var prop in type.GetProperties())
                {
                    var lidfp = prop.GetCustomAttribute<PresenceFormatProvider>();
                    if (lidfp != null)
                    {
                        lidfp.PropertyInfo = prop;
                        RegisterFormatter(lidfp);
                    }
                }
            }

            foreach(var former in _formatters.Where(kvp => kvp.Value is FallbackPresenceFormatProvider && !((FallbackPresenceFormatProvider)kvp.Value).NoNotImplementedWarning))
            {
                ArchiveLogger.Warning($"[{nameof(PresenceFormatter)}] Identifier \"{former.Key}\" has not been implemented! Using Fallback default values!");
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
            if (!(pfp?.PropertyInfo?.GetGetMethod()?.IsStatic ?? false))
            {
                throw new ArgumentException("Invalid PropertyInfo, getter must be static and public!");
            }

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
                    throw new ArgumentException($"Duplicate formatter identifier: \"{pfp.Identifier}\" (\"{pfp.PropertyInfo.DeclaringType.FullName}.{pfp.PropertyInfo.Name}\")");
                }
            }

            _formatters.Add(pfp.Identifier, pfp);
            ArchiveLogger.Debug($"[{nameof(PresenceFormatter)}]{(fallbackOverridden ? " (Fallback Overridden)" : ((pfp is FallbackPresenceFormatProvider) ? " (Fallback)" : string.Empty))} Registered: \"{pfp.Identifier}\" => {pfp.PropertyInfo.DeclaringType.FullName}.{pfp.PropertyInfo.Name} (ASM:{pfp.PropertyInfo.DeclaringType.Assembly.GetName().Name})");
        }

        internal static object Get(string identifier)
        {
            _formatters.TryGetValue(identifier, out var former);

            if (former == null) return null;

            return former.PropertyInfo?.GetValue(null);
        }

        internal static T Get<T>(string identifier)
        {
            _formatters.TryGetValue(identifier, out var former);

            if (former == null) return default(T);

            if (!former.PropertyInfo.PropertyType.IsAssignableFrom(typeof(T)) && typeof(T) != typeof(string)) throw new ArgumentException($"The property at identifier \"{identifier}\" is not declared as Type \"{typeof(T).Name}\"!");

            return (T) former.PropertyInfo?.GetValue(null);
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
                    formatted = formatted.ReplaceCaseInsensitive($"%{former.Key}%", former.Value.PropertyInfo.GetValue(null)?.ToString() ?? "null");
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

            public PropertyInfo PropertyInfo { get; internal set; }

            public PresenceFormatProvider(string identifier)
            {
                Identifier = identifier;
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
}
