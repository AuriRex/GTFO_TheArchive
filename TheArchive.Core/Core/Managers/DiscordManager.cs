using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Core.Managers
{
    public class DiscordManager
    {
        #region native_methods
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
        #endregion native_methods

        public static bool Active { get; internal set; } = false;

        private static bool _hasDiscordDllBeenLoaded = false;
        private static float _lastCheckedTime = 0f;


        //private static DiscordClient _discord = new DiscordClient();

        internal void Setup()
        {
            if(!_hasDiscordDllBeenLoaded)
            {
                if(!File.Exists("discord_game_sdk.dll"))
                {
                    using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("TheArchive.Resources.discord_game_sdk.dll"))
                    {
                        using (var file = new FileStream("discord_game_sdk.dll", FileMode.Create, FileAccess.Write))
                        {
                            resource.CopyTo(file);
                        }
                    }
                }
                
                LoadLibrary("discord_game_sdk.dll");
            }
        }

        internal static void Update()
        {
            
        }

        public class DiscordClient
        {
            public const long CLIENT_ID = 0L;

            private long _clientId = 0L;
            private Discord.Discord _discordClient;
            private Discord.ActivityManager _activityManager;

            public DiscordClient()
            {
                _clientId = CLIENT_ID;
            }

            public DiscordClient(long clientId)
            {
                _clientId = clientId;
            }

            public void Initialize()
            {
                _discordClient = new Discord.Discord(_clientId, (UInt64) Discord.CreateFlags.NoRequireDiscord);

                _activityManager = _discordClient.GetActivityManager();
                _activityManager.RegisterSteam(493520); // GTFO App ID
            }

            public void RunCallbacks()
            {
                _discordClient?.RunCallbacks();
            }



        }

    }
}
