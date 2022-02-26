using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
            LastState = CurrentState;
            CurrentState = state;
            if(!keepTimer)
            {
                CurrentStateStartTime = DateTimeOffset.UtcNow;
            }
        }

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
            if(_lastCheckedTime + 5 <= Utils.Time)
            {
                _lastCheckedTime = Utils.Time;

                Discord.Activity? activity = _discord?.BuildActivity(CurrentState, CurrentStateStartTime);

                _discord?.RunCallbacks();

                if(activity != null && !activity.Equals(_lastActivity))
                {
                    if(_discord?.TryUpdateActivity(activity.Value) ?? false)
                    {
                        _lastActivity = activity.Value;
                    }
                }
            }
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
#warning todo: replace with command that runs steam://
                _activityManager.RegisterSteam(493520); // GTFO App ID

                TryUpdateActivity(BuildActivity(PresenceGameState.Startup, DateTimeOffset.UtcNow));
                /*_activityManager.UpdateActivity(new Activity
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
                }, ActivityUpdateDebugLog);*/

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
                            Timestamps = new ActivityTimestamps
                            {
                                Start = startTime.ToUnixTimeSeconds()
                            },
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
                            Timestamps = new ActivityTimestamps
                            {
                                Start = startTime.ToUnixTimeSeconds()
                            },
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
                            Timestamps = new ActivityTimestamps
                            {
                                Start = startTime.ToUnixTimeSeconds()
                            },
                            Assets = new ActivityAssets
                            {
                                LargeImage = "icon_lobby",
                                LargeText = $"GTFO - R{ArchiveMod.CurrentRundown.GetIntValue()}: \"{Utils.GetRundownTitle()}\"",
                                SmallImage = PresenceManager.GetCharacterImageKey(),
                                SmallText = $"Playing as {PresenceManager.GetCharacterName()}",
                            },
                            Party = new ActivityParty
                            {
                                Size = new PartySize
                                {
                                    CurrentSize = (int) PresenceFormatter.Get("MaxPlayerSlots") - (int) PresenceFormatter.Get("OpenSlots"),
                                    MaxSize = (int) PresenceFormatter.Get("MaxPlayerSlots")
                                }
                            }
                        };
                    case PresenceGameState.Dropping:
                        return new Activity
                        {
                            ApplicationId = _clientId,
                            Details = PresenceFormatter.FormatPresenceString("%Rundown% %Expedition% \"%ExpeditionName%\""),
                            State = "Dropping ...",
                            Timestamps = new ActivityTimestamps
                            {
                                Start = startTime.ToUnixTimeSeconds()
                            },
                            Assets = new ActivityAssets
                            {
                                LargeImage = "icon_dropping",
                                LargeText = "Riding the elevator to hell ...",
                            },
                            Party = new ActivityParty
                            {
                                Size = new PartySize
                                {
                                    CurrentSize = (int) PresenceFormatter.Get("MaxPlayerSlots") - (int) PresenceFormatter.Get("OpenSlots"),
                                    MaxSize = (int) PresenceFormatter.Get("MaxPlayerSlots")
                                }
                            }
                        };
                    case PresenceGameState.LevelGenerationFinished:
                        return new Activity
                        {
                            ApplicationId = _clientId,
                            Details = PresenceFormatter.FormatPresenceString("%Rundown% %Expedition% \"%ExpeditionName%\""),
                            State = "Activating breaks ...",
                            Timestamps = new ActivityTimestamps
                            {
                                Start = startTime.ToUnixTimeSeconds()
                            },
                            Assets = new ActivityAssets
                            {
                                LargeImage = "icon_dropping",
                                LargeText = "Next stop: Hell",
                            },
                            Party = new ActivityParty
                            {
                                Size = new PartySize
                                {
                                    CurrentSize = (int) PresenceFormatter.Get("MaxPlayerSlots") - (int) PresenceFormatter.Get("OpenSlots"),
                                    MaxSize = (int) PresenceFormatter.Get("MaxPlayerSlots")
                                }
                            }
                        };
                    case PresenceGameState.InLevel:
                        return new Activity
                        {
                            ApplicationId = _clientId,
                            Details = PresenceFormatter.FormatPresenceString("%Rundown% %Expedition% \"%ExpeditionName%\""),
                            State = "In Expedition",
                            Timestamps = new ActivityTimestamps
                            {
                                Start = startTime.ToUnixTimeSeconds()
                            },
                            Assets = new ActivityAssets
                            {
                                LargeImage = "gtfo_icon",
                                LargeText = "Exploring ...",
                                SmallImage = PresenceManager.GetMeleeWeaponKey(),
                                SmallText = (string) PresenceFormatter.Get("EquippedMeleeWeaponName")
                            },
                            Party = new ActivityParty
                            {
                                Size = new PartySize
                                {
                                    CurrentSize = (int) PresenceFormatter.Get("MaxPlayerSlots") - (int) PresenceFormatter.Get("OpenSlots"),
                                    MaxSize = (int) PresenceFormatter.Get("MaxPlayerSlots")
                                }
                            }
                        };
                    case PresenceGameState.ExpeditionFailed:
                        return new Activity
                        {
                            ApplicationId = _clientId,
                            Details = PresenceFormatter.FormatPresenceString("%Rundown% %Expedition% \"%ExpeditionName%\""),
                            State = "EXPD FAILED",
                            Timestamps = new ActivityTimestamps
                            {
                                Start = startTime.ToUnixTimeSeconds()
                            },
                            Assets = new ActivityAssets
                            {
                                LargeImage = "icon_failed",
                                LargeText = "Beep, Beep, Beeeeeeeeeeep",
                                SmallImage = PresenceManager.GetCharacterImageKey(),
                                SmallText = $"Prisoner \"{PresenceManager.GetCharacterName()}\", Status: DECEASED",
                            },
                            Party = new ActivityParty
                            {
                                Size = new PartySize
                                {
                                    CurrentSize = (int) PresenceFormatter.Get("MaxPlayerSlots") - (int) PresenceFormatter.Get("OpenSlots"),
                                    MaxSize = (int) PresenceFormatter.Get("MaxPlayerSlots")
                                }
                            }
                        };
                    case PresenceGameState.ExpeditionSuccess:
                        return new Activity
                        {
                            ApplicationId = _clientId,
                            Details = PresenceFormatter.FormatPresenceString("%Rundown% %Expedition% \"%ExpeditionName%\""),
                            State = "EXPD SURVIVED",
                            Timestamps = new ActivityTimestamps
                            {
                                Start = startTime.ToUnixTimeSeconds()
                            },
                            Assets = new ActivityAssets
                            {
                                LargeImage = "icon_survived",
                                LargeText = "Hydrostasis awaits ...",
                                SmallImage = PresenceManager.GetCharacterImageKey(),
                                SmallText = $"Prisoner \"{PresenceManager.GetCharacterName()}\", Status: ALIVE",
                            },
                            Party = new ActivityParty
                            {
                                Size = new PartySize
                                {
                                    CurrentSize = (int) PresenceFormatter.Get("MaxPlayerSlots") - (int) PresenceFormatter.Get("OpenSlots"),
                                    MaxSize = (int) PresenceFormatter.Get("MaxPlayerSlots")
                                }
                            }
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
                ArchiveLogger.Debug($"Activity Updated: {result}");
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
