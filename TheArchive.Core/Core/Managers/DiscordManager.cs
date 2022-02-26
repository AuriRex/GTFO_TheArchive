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
            if (!ArchiveMod.Settings.EnableDiscordRichPresence)
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

                TryUpdateActivity(BuildActivity(PresenceGameState.Startup, DateTimeOffset.UtcNow));

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
                switch (state)
                {
                    case PresenceGameState.Startup:
                        return new Activity
                        {
                            ApplicationId = _clientId,
                            Details = $"Rundown {ArchiveMod.CurrentRundown.GetIntValue()}: \"{Utils.GetRundownTitle()}\"",
                            State = "Waking prisoners ...",
                            Timestamps = GetTimestamp(startTime),
                            Assets = new ActivityAssets
                            {
                                LargeImage = "gtfo_icon",
                                LargeText = "GTFO",
                            }
                        };
                    case PresenceGameState.NoLobby:
                        return new Activity
                        {
                            ApplicationId = _clientId,
                            Details = $"Rundown {ArchiveMod.CurrentRundown.GetIntValue()}: \"{Utils.GetRundownTitle()}\"",
                            State = "Deciding what to do",
                            Timestamps = GetTimestamp(startTime),
                            Assets = new ActivityAssets
                            {
                                LargeImage = "gtfo_icon",
                                LargeText = "GTFO",
                            }
                        };
                    case PresenceGameState.InLobby:
                        return new Activity
                        {
                            ApplicationId = _clientId,
                            Details = PresenceFormatter.FormatPresenceString("%Rundown% %Expedition% \"%ExpeditionName%\""),
                            State = "In Lobby",
                            Timestamps = GetTimestamp(startTime),
                            Assets = new ActivityAssets
                            {
                                LargeImage = "icon_lobby",
                                LargeText = $"GTFO - R{ArchiveMod.CurrentRundown.GetIntValue()}: \"{Utils.GetRundownTitle()}\"",
                                SmallImage = PresenceManager.GetCharacterImageKey(),
                                SmallText = $"Playing as {PresenceManager.GetCharacterName()}",
                            },
                            Party = GetParty()
                        };
                    case PresenceGameState.Dropping:
                        return new Activity
                        {
                            ApplicationId = _clientId,
                            Details = PresenceFormatter.FormatPresenceString("%Rundown% %Expedition% \"%ExpeditionName%\""),
                            State = "Dropping ...",
                            Timestamps = GetTimestamp(startTime),
                            Assets = new ActivityAssets
                            {
                                LargeImage = "icon_dropping",
                                LargeText = "Riding the elevator to hell ...",
                            },
                            Party = GetParty()
                        };
                    case PresenceGameState.LevelGenerationFinished:
                        return new Activity
                        {
                            ApplicationId = _clientId,
                            Details = PresenceFormatter.FormatPresenceString("%Rundown% %Expedition% \"%ExpeditionName%\""),
                            State = "Activating breaks ...",
                            Timestamps = GetTimestamp(startTime),
                            Assets = new ActivityAssets
                            {
                                LargeImage = "icon_dropping",
                                LargeText = "Next stop: Hell",
                            },
                            Party = GetParty()
                        };
                    case PresenceGameState.InLevel:
                        return new Activity
                        {
                            ApplicationId = _clientId,
                            Details = PresenceFormatter.FormatPresenceString("%Rundown% %Expedition% \"%ExpeditionName%\""),
                            State = "In Expedition",
                            Timestamps = GetTimestamp(startTime),
                            Assets = new ActivityAssets
                            {
                                LargeImage = "gtfo_icon",
                                LargeText = "Exploring ...",
                                SmallImage = PresenceManager.GetMeleeWeaponKey(),
                                SmallText = (string) PresenceFormatter.Get("EquippedMeleeWeaponName")
                            },
                            Party = GetParty()
                        };
                    case PresenceGameState.ExpeditionFailed:
                        return new Activity
                        {
                            ApplicationId = _clientId,
                            Details = PresenceFormatter.FormatPresenceString("%Rundown% %Expedition% \"%ExpeditionName%\""),
                            State = "EXPD FAILED",
                            Timestamps = GetTimestamp(startTime),
                            Assets = new ActivityAssets
                            {
                                LargeImage = "icon_failed",
                                LargeText = "Beep, Beep, Beeeeeeeeeeep",
                                SmallImage = PresenceManager.GetCharacterImageKey(),
                                SmallText = $"Prisoner \"{PresenceManager.GetCharacterName()}\", Status: DECEASED",
                            },
                            Party = GetParty()
                        };
                    case PresenceGameState.ExpeditionSuccess:
                        return new Activity
                        {
                            ApplicationId = _clientId,
                            Details = PresenceFormatter.FormatPresenceString("%Rundown% %Expedition% \"%ExpeditionName%\""),
                            State = "EXPD SURVIVED",
                            Timestamps = GetTimestamp(startTime),
                            Assets = new ActivityAssets
                            {
                                LargeImage = "icon_survived",
                                LargeText = "Hydrostasis awaits ...",
                                SmallImage = PresenceManager.GetCharacterImageKey(),
                                SmallText = $"Prisoner \"{PresenceManager.GetCharacterName()}\", Status: ALIVE",
                            },
                            Party = GetParty()
                        };
                }
                return DefaultFallbackActivity;
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
