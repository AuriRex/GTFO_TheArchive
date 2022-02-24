using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Utilities;

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


        private static DiscordClient _discord;

        internal static void Setup()
        {
            if(!_hasDiscordDllBeenLoaded)
            {
                if(!File.Exists("discord_game_sdk.dll"))
                {
                    ArchiveLogger.Notice("Extracting discord_game_sdk.dll into game folder ...");
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

            _discord = new DiscordClient();

            _discord.Initialize();
        }

        internal static void Update()
        {
            _discord?.RunCallbacks();
        }

        public class DiscordClient
        {
            public const long CLIENT_ID = 946141176338190346L;

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

                _discordClient.SetLogHook(Discord.LogLevel.Debug, LogHook);

                _activityManager = _discordClient.GetActivityManager();
                _activityManager.RegisterSteam(493520); // GTFO App ID

                _activityManager.UpdateActivity(new Activity
                {
                    State = "Waking prisoners ...",
                    Details = $"Rundown #{ArchiveMod.CurrentRundown.GetIntValue()}: \"{Utils.GetRundownTitle()}\"",
                    ApplicationId = _clientId,
                    Timestamps = new ActivityTimestamps
                    {
                        Start = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    },
                    Assets = new ActivityAssets
                    {
                        LargeImage = "gtfo_icon",
                        LargeText = "GTFO",
                    }
                }, callbackthing);
                /*_activityManager.UpdateActivity(new Activity
                {
                    State = "In Expedition",
                    Details = "R1 B2 \"The Officer\" [Zone 48]",
                    ApplicationId = _clientId,
                    Timestamps = new ActivityTimestamps
                    {
                        Start = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    },
                    Assets = new ActivityAssets
                    {
                        LargeImage = "gtfo_icon",
                        LargeText = "GTFO",
                        SmallImage = "weapon_maul",
                        SmallText = "Maul Mafia - Playing as Woods",
                    },
                    Party = new ActivityParty
                    {
                        Size = new PartySize
                        {
                            CurrentSize = (int) PresenceFormatter.Get("MaxPlayerSlots") - (int) PresenceFormatter.Get("OpenSlots"),
                            MaxSize = (int) PresenceFormatter.Get("MaxPlayerSlots")
                        }
                    }
                }, callbackthing);*/
            }

            private void callbackthing(Result result)
            {
                ArchiveLogger.Notice($"Activity Updated: {result}");
            }

            private static void LogHook(LogLevel level, string message)
            {
                ArchiveLogger.Notice($"[{nameof(DiscordClient)}] {level}: {message}");
            }

            public void RunCallbacks()
            {
                _discordClient?.RunCallbacks();
            }



        }

    }
}
