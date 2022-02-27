using Discord;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using TheArchive.Core.Models;
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

        public static PresenceGameState LastState { get; private set; }
        public static PresenceGameState CurrentState { get; private set; }

        public static RichPresenceSettings Settings { get; private set; } = new RichPresenceSettings();

        public static DateTimeOffset CurrentStateStartTime { get; private set; }

        private static Discord.Activity _lastActivity;

        private static bool _hasDiscordDllBeenLoaded = false;
        private static float _lastCheckedTime = 0f;


        private static DiscordClient _discord;

        public static void UpdateGameState(PresenceGameState state, bool keepTimer = false)
        {
            ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"[{nameof(DiscordManager)}] UpdateGameState(): {CurrentState} --> {state}, keepTimer: {keepTimer}");
            LastState = CurrentState;
            CurrentState = state;
            if(!keepTimer)
            {
                CurrentStateStartTime = DateTimeOffset.UtcNow;
            }
        }

        internal static void Setup()
        {
            Settings = LocalFiles.LoadConfig<RichPresenceSettings>(out var fileExists, false).FillDefaultDictValues();
            if (!fileExists)
            {
                LocalFiles.SaveConfig(Settings);
            }

            ArchiveMod.Settings.EnableDiscordRichPresence = Settings.EnableDiscordRichPresence;

            if (!Settings.EnableDiscordRichPresence)
            {
                ArchiveLogger.Notice($"[{nameof(DiscordManager)}] Discord Rich Presence disabled, skipping setup!");
                return;
            }

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
                _hasDiscordDllBeenLoaded = true;
            }

            _discord = new DiscordClient();

            _discord.Initialize();
        }

        internal static void Update()
        {
            if (_discord == null) return;

            if(_lastCheckedTime + 5 <= Utils.Time)
            {
                _lastCheckedTime = Utils.Time;

                Discord.Activity activity = _discord.BuildActivity(CurrentState, CurrentStateStartTime);

                if(!activity.Equals(_lastActivity))
                {
                    if(_discord.TryUpdateActivity(activity))
                    {
                        _lastActivity = activity;
                    }
                }
            }

            _discord.RunCallbacks();
        }

        internal static void OnApplicationQuit()
        {
            _discord?.Dispose();
            _discord = null;
            LocalFiles.SaveConfig(Settings);
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
#warning todo: replace with command that runs steam:// maybe?
                _activityManager.RegisterSteam(493520); // GTFO App ID

                UpdateGameState(PresenceGameState.Startup, false);
                TryUpdateActivity(BuildActivity(PresenceGameState.Startup, CurrentStateStartTime));
                _discord.RunCallbacks();
            }

            private static Activity DefaultFallbackActivity = new Activity
            {
                Details = "???",
                State = "err:// no c0nnec7ion",
                ApplicationId = CLIENT_ID,
                Assets = new ActivityAssets
                {
                    LargeImage = "gtfo_icon",
                    LargeText = "GTFO",
                }
            };

            public static ActivityParty GetParty(string partyId = null)
            {
                return new ActivityParty
                {
                    Id = partyId,
                    Size = new PartySize
                    {
                        CurrentSize = (int) PresenceFormatter.Get("MaxPlayerSlots") - (int) PresenceFormatter.Get("OpenSlots"),
                        MaxSize = (int) PresenceFormatter.Get("MaxPlayerSlots")
                    }
                };
            }

            public ActivityTimestamps GetTimestamp(DateTimeOffset startTime, DateTimeOffset? endTime = null)
            {
                if (endTime.HasValue)
                {
                    return new ActivityTimestamps
                    {
                        Start = startTime.ToUnixTimeSeconds(),
                        End = endTime.Value.ToUnixTimeSeconds()
                    };
                }
                return new ActivityTimestamps
                {
                    Start = startTime.ToUnixTimeSeconds()
                };
            }

            internal Activity BuildActivity(PresenceGameState state, DateTimeOffset startTime)
            {
                if(Settings.DiscordRPCFormat.TryGetValue(state, out var format))
                {
                    return ActivityFromFormat(format.GetNext(), state, startTime);
                }
                return DefaultFallbackActivity;
            }

            private Activity ActivityFromFormat(RichPresenceSettings.GSActivityFormat format, PresenceGameState state, DateTimeOffset startTime)
            {
                if (format == null) return DefaultFallbackActivity;

                var extra = ("state", state.ToString());

                var activity = new Activity
                {
                    ApplicationId = _clientId,
                    Details = format.Details?.Format(extra),
                    State = format.Status?.Format(extra),
                };

                activity.Assets = new ActivityAssets
                {
                    LargeImage = format.Assets.LargeImageKey?.Format(extra),
                    LargeText = format.Assets.LargeTooltip?.Format(extra),
                    SmallImage = format.Assets.SmallImageKey?.Format(extra),
                    SmallText = format.Assets.SmallTooltip?.Format(extra)
                };

                if (format.DisplayTimeElapsed)
                {
                    activity.Timestamps = GetTimestamp(startTime);
                }

                if (format.DisplayPartyInfo)
                {
                    activity.Party = GetParty();
                }

                 return activity;
            }

            internal bool TryUpdateActivity(Discord.Activity activity)
            {
                if (_activityManager == null) return false;
                ArchiveLogger.Notice($"[{nameof(DiscordManager)}] Activity updated: Details:{activity.Details} State:{activity.State}");
                _activityManager.UpdateActivity(activity, ActivityUpdateDebugLog);
                return true;
            }

            public void Dispose()
            {
                _discordClient.Dispose();
                _discordClient = null;
            }

            private void ActivityUpdateDebugLog(Result result)
            {
                ArchiveLogger.Debug($"[{nameof(DiscordManager)}] Activity update result: {result}");
            }

            private static void LogHook(LogLevel level, string message)
            {
                Action<string> log;
                switch(level)
                {
                    case LogLevel.Error:
                        log = ArchiveLogger.Error;
                        break;
                    case LogLevel.Warn:
                        log = ArchiveLogger.Warning;
                        break;
                    default:
                    case LogLevel.Info:
                        log = ArchiveLogger.Notice;
                        break;
                    case LogLevel.Debug:
                        log = ArchiveLogger.Debug;
                        break;
                }

                log.Invoke($"[{nameof(DiscordClient)}] {level}: {message}");
            }

            public void RunCallbacks()
            {
                _discordClient?.RunCallbacks();
            }



        }

    }
}
