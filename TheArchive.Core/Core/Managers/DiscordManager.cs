using Discord;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using TheArchive.Core.Models;
using TheArchive.Core.Settings;
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

        private static IntPtr _discordLibPointer;

        public static bool HasBeenSetup => _settings != null;

        private static Discord.Activity _lastActivity;

        private static bool _hasDiscordDllBeenLoaded = false;
        private static float _lastCheckedTime = 0f;


        private static DiscordClient _discordClient;

        private static RichPresenceSettings _settings = null;
        private static bool _internalDisabled = false;

        public static void Enable(RichPresenceSettings rpcSettings)
        {
            if (_internalDisabled) return;
            if (rpcSettings == null) throw new ArgumentNullException($"{nameof(rpcSettings)}");
            _settings = rpcSettings;

            if(!_hasDiscordDllBeenLoaded)
            {
                try
                {
                    var path = Path.Combine(LocalFiles.ModLocalLowPath, "discord_game_sdk.dll");
                    if (!File.Exists(path))
                    {
                        ArchiveLogger.Notice($"Extracting discord_game_sdk.dll into \"{path}\" ...");
                        using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("TheArchive.Resources.discord_game_sdk.dll"))
                        {
                            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
                            {
                                resource.CopyTo(file);
                            }
                        }
                    }

                    _discordLibPointer = LoadLibrary(path);
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Error($"[{nameof(DiscordManager)}] Error while trying to load the native discord dll! {ex}: {ex.Message}");
                    ArchiveLogger.Exception(ex);
                    _internalDisabled = true;
                }
                finally
                {
                    _hasDiscordDllBeenLoaded = true;
                }
            }

            try
            {
                _discordClient = new DiscordClient();

                _discordClient.Initialize();

                if (_lastCheckedTime + 5 <= Utils.Time)
                {
                    _lastCheckedTime = Utils.Time;
                    var activity = _discordClient.BuildActivity(PresenceManager.CurrentState, PresenceManager.CurrentStateStartTime);
                    if (_discordClient.TryUpdateActivity(activity))
                    {
                        _lastActivity = activity;
                    }
                }
                _discordClient.RunCallbacks();
            }
            catch(Discord.ResultException ex)
            {
                ArchiveLogger.Warning($"Discord seems to be closed, disabling Rich Presence Features ... ({ex}: {ex.Message})");
                _discordClient = null;
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"Exception has been thrown in {nameof(DiscordManager)}. {ex}: {ex.Message}");
                ArchiveLogger.Exception(ex);
            }
        }

        public static void Disable()
        {
            _discordClient?.Dispose();
            _discordClient = null;
        }

        public static void Update()
        {
            if (_discordClient == null || _internalDisabled) return;

            if(_lastCheckedTime + 5 <= Utils.Time)
            {
                _lastCheckedTime = Utils.Time;

                Discord.Activity activity = _discordClient.BuildActivity(PresenceManager.CurrentState, PresenceManager.CurrentStateStartTime);

                if(!activity.Equals(_lastActivity))
                {
                    if(_discordClient.TryUpdateActivity(activity))
                    {
                        _lastActivity = activity;
                    }
                }
            }

            _discordClient.RunCallbacks();
        }

        internal static void OnApplicationQuit()
        {
            _discordClient?.Dispose();
            _discordClient = null;
        }

        public class DiscordClient
        {
            public const long CLIENT_ID = 946141176338190346L;

            private readonly long _clientId = 0L;
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
                _discordClient = new Discord.Discord(_clientId, (UInt64) CreateFlags.NoRequireDiscord);

                _discordClient.SetLogHook(_settings.DEBUG_RichPresenceLogSpam ? LogLevel.Debug : LogLevel.Info, LogHook);

                _activityManager = _discordClient.GetActivityManager();
#warning todo: replace with command that runs steam:// maybe?
                _activityManager.RegisterSteam(493520); // GTFO App ID
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
                        CurrentSize = PresenceFormatter.Get<int>("MaxPlayerSlots") - PresenceFormatter.Get<int>("OpenSlots"),
                        MaxSize = PresenceFormatter.Get<int>("MaxPlayerSlots")
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
                if(_settings.DiscordRPCFormat.TryGetValue(state, out var format))
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
                
                if(_settings.DEBUG_RichPresenceLogSpam)
                {
                    ArchiveLogger.Notice($"[{nameof(DiscordManager)}] Activity updated: Details:{activity.Details} State:{activity.State}");
                    _activityManager.UpdateActivity(activity, ActivityUpdateDebugLog);
                    return true;
                }

                _activityManager.UpdateActivity(activity, ActivityVoidLog);
                return true;
            }

            public void Dispose()
            {
                _activityManager?.ClearActivity((result) => ArchiveLogger.Debug($"[{nameof(DiscordManager)}] Activity clear result: {result}"));
                _discordClient?.Dispose();
                _discordClient = null;
            }

            private void ActivityUpdateDebugLog(Result result)
            {
                ArchiveLogger.Debug($"[{nameof(DiscordManager)}] Activity update result: {result}");
            }

            private void ActivityVoidLog(Result result) { }

            private static void LogHook(LogLevel level, string message)
            {
                switch(level)
                {
                    case LogLevel.Error:
                        ArchiveLogger.Error($"[{nameof(DiscordClient)}] {level}: {message}");
                        return;
                    case LogLevel.Warn:
                        ArchiveLogger.Warning($"[{nameof(DiscordClient)}] {level}: {message}");
                        return;
                    default:
                    case LogLevel.Info:
                        ArchiveLogger.Notice($"[{nameof(DiscordClient)}] {level}: {message}");
                        return;
                    case LogLevel.Debug:
                        ArchiveLogger.Debug($"[{nameof(DiscordClient)}] {level}: {message}");
                        return;
                }
            }

            public void RunCallbacks()
            {
                _discordClient?.RunCallbacks();
            }
        }
    }
}
