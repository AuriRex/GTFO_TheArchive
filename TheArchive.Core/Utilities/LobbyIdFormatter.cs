using System;
using System.Collections.Generic;
using System.Reflection;

namespace TheArchive.Utilities
{
    public class LobbyIdFormatter
    {

        private static Dictionary<string, PropertyInfo> _formatters = new Dictionary<string, PropertyInfo>();

        public static void RegisterFormatter(string identifier, PropertyInfo getStatePI)
        {
            if(!(getStatePI?.GetGetMethod()?.IsStatic ?? false))
            {
                throw new ArgumentException("Invalid PropertyInfo, getter must be static and public!");
            }

            if(_formatters.TryGetValue(identifier, out _))
            {
                throw new ArgumentException($"Duplicate formatter identifier: \"{identifier}\"");
            }

            _formatters.Add(identifier, getStatePI);
        }

        public static string FormatLobbyId(string formatString, string lobbyId)
        {
            string formatted = formatString;

            foreach(var former in _formatters)
            {

                formatted = formatted.ReplaceCaseInsensitive($"%{former.Key}%", former.Value.GetValue(null)?.ToString() ?? "null");

            }

            return formatted;
        }

    }
}
