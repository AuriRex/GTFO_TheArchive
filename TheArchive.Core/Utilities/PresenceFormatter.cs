using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TheArchive.Utilities
{
    public static class PresenceFormatter
    {
        private static Dictionary<string, PresenceFormatProvider> _formatters = new Dictionary<string, PresenceFormatProvider>();


        private static List<Assembly> _listOfAssemblies = new List<Assembly>();

        internal static void Setup()
        {
            ArchiveLogger.Debug($"[{nameof(PresenceFormatter)}] Setting up ...");
            Assembly.GetExecutingAssembly().RegisterAllPresenceFormatProviders(false);

            foreach(var asm in _listOfAssemblies)
            {
                foreach(var type in asm.GetTypes())
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
            }

            foreach(var former in _formatters.Where(kvp => kvp.Value is FallbackPresenceFormatProvider))
            {
                ArchiveLogger.Warning($"[{nameof(PresenceFormatter)}] Identifier \"{former.Key}\" has not been implemented! Using Fallback default values!");
            }

        }

        public static void RegisterAllPresenceFormatProviders(this Assembly asm, bool throwOnDuplicate = true)
        {
            if (asm == null) throw new ArgumentException("Assembly must not be null!");
            if (_listOfAssemblies.Contains(asm))
            {
                if (throwOnDuplicate)
                    throw new ArgumentException($"Duplicate Assembly registered: \"{asm?.FullName}\"");
                else return;
            }

            _listOfAssemblies.Add(asm);
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

        public static string FormatPresenceString(string formatString)
        {
            string formatted = formatString;

            foreach(var former in _formatters)
            {

                formatted = formatted.ReplaceCaseInsensitive($"%{former.Key}%", former.Value.PropertyInfo.GetValue(null)?.ToString() ?? "null");

            }

            return formatted;
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class PresenceFormatProvider : Attribute
        {
            public string Identifier { get; set; }

            internal PropertyInfo PropertyInfo { get; set; }

            public PresenceFormatProvider(string identifier)
            {
                Identifier = identifier;
            }
        }

        [AttributeUsage(AttributeTargets.Property)]
        internal class FallbackPresenceFormatProvider : PresenceFormatProvider
        {
            public FallbackPresenceFormatProvider(string identifier) : base(identifier) { }
        }

    }
}
