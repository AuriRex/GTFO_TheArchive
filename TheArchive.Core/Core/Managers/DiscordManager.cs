using Discord;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using TheArchive.Core.Models;
using TheArchive.Core.Settings;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Core.Managers
{
    public class DiscordManager
    {
        #region native_methods
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);
        #endregion native_methods

        private static IntPtr _discordLibPointer;

        public static bool HasBeenSetup => _settings != null;

        private static Discord.Activity _lastActivity;

        private static bool _hasDiscordDllBeenLoaded = false;
        private static float _lastCheckedTime = 0f;

        public static Guid PartyGuid { get; private set; } = new Guid();

        private static RichPresenceSettings _settings = null;
        private static bool _internalDisabled = false;

        public static event Action<string> OnActivityJoin;

        private static IArchiveLogger _logger;
        private static IArchiveLogger Logger
        {
            get
            {
                return _logger ??= Loader.LoaderWrapper.CreateLoggerInstance(nameof(DiscordManager), ConsoleColor.Magenta);
            }
        }

        public static void Enable(RichPresenceSettings rpcSettings)
        {
            if (_internalDisabled) return;
            if (rpcSettings == null) throw new ArgumentNullException($"{nameof(rpcSettings)}");
            _settings = rpcSettings;
            
            if(!_hasDiscordDllBeenLoaded)
            {
                try
                {
                    //Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + LocalFiles.ModLocalLowPath);
                    var path = Path.Combine(LocalFiles.ModLocalLowPath, "discord_game_sdk.dll");
                    string hashExisting = null;
                    if(File.Exists(path))
                    {
                        hashExisting = Utilities.Utils.GetHash(File.ReadAllBytes(path)).ToUpper();
                    }
                    
                    if (!File.Exists(path) || hashExisting != null)
                    {
                        var discord_game_sdk_bytes = Utils.GetResource(Assembly.GetExecutingAssembly(), "TheArchive.Resources.discord_game_sdk.dll");

                        var hashResource = Utils.GetHash(discord_game_sdk_bytes).ToUpper();

                        if(hashExisting == null || hashExisting != hashResource)
                        {
                            if (File.Exists(path))
                            {
                                Logger.Notice($"Updating discord sdk ... [old:{hashExisting}] vs [new:{hashResource}]");
                                File.Delete(path);
                            }
                            Logger.Notice($"Extracting discord_game_sdk.dll into \"{path}\" ...");
                            File.WriteAllBytes(path, discord_game_sdk_bytes);
                        }
                        
                    }
                    
                    _discordLibPointer = LoadLibrary(path);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error while trying to load the native discord dll! {ex}: {ex.Message}");
                    Logger.Exception(ex);
                    _internalDisabled = true;
                }
                finally
                {
                    _hasDiscordDllBeenLoaded = true;
                }
            }

            try
            {
                DiscordClient.Initialize();

                if (_lastCheckedTime + 5 <= Utils.Time)
                {
                    _lastCheckedTime = Utils.Time;
                    var activity = DiscordClient.BuildActivity(PresenceManager.CurrentState, PresenceManager.CurrentStateStartTime);
                    if (DiscordClient.TryUpdateActivity(activity))
                    {
                        _lastActivity = activity;
                    }
                }
                DiscordClient.RunCallbacks();
            }
            catch(Discord.ResultException ex)
            {
                Logger.Warning($"Discord seems to be closed, disabling Rich Presence Features ... ({ex}: {ex.Message})");
                _internalDisabled = true;
            }
            catch(Exception ex)
            {
                Logger.Error($"Exception has been thrown in {nameof(DiscordManager)}. {ex}: {ex.Message}");
                Logger.Exception(ex);
            }
        }

        public static void Disable()
        {
            DiscordClient.Dispose();
        }

        public static void Update()
        {
            if (_internalDisabled) return;

            if(_lastCheckedTime + 5 <= Utils.Time)
            {
                _lastCheckedTime = Utils.Time;

                Discord.Activity activity = DiscordClient.BuildActivity(PresenceManager.CurrentState, PresenceManager.CurrentStateStartTime);

                if(!activity.Equals(_lastActivity))
                {
                    if(DiscordClient.TryUpdateActivity(activity))
                    {
                        _lastActivity = activity;
                    }
                }
            }

            DiscordClient.RunCallbacks();
        }

        internal static void OnApplicationQuit()
        {
            DiscordClient.Dispose();
        }

        public static class DiscordClient
        {
            public const long CLIENT_ID = 946141176338190346L;

            private static long _clientId = 0L;
            private static Discord.Discord _discordClient;
            private static Discord.ActivityManager _activityManager;

            private static IArchiveLogger _clientLogger;
            private static IArchiveLogger ClientLogger
            {
                get
                {
                    return _clientLogger ??= Loader.LoaderWrapper.CreateLoggerInstance("DiscordClient", ConsoleColor.Magenta);
                }
            }

            public static void Initialize(long clientId = CLIENT_ID)
            {
                _clientId = clientId;
                _discordClient = new Discord.Discord(_clientId, (UInt64) CreateFlags.NoRequireDiscord);

                _discordClient.SetLogHook(_settings.DEBUG_RichPresenceLogSpam ? LogLevel.Debug : LogLevel.Info, LogHook);

                _activityManager = _discordClient.GetActivityManager();
#warning todo: replace with command that runs steam:// maybe?
                _activityManager.RegisterSteam(493520); // GTFO App ID

                _activityManager.OnActivityJoin += _activityManager_OnActivityJoin;
            }

            private static void _activityManager_OnActivityJoin(string secret)
            {
                OnActivityJoin?.Invoke(secret);
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

            public static ActivitySecrets? GetSecrets(string joinSecret = null)
            {
                if (joinSecret == null) return null;
                return new ActivitySecrets
                {
                    Join = joinSecret,
                };
            }

            public static ActivityTimestamps GetTimestamp(DateTimeOffset startTime, DateTimeOffset? endTime = null)
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

            internal static Activity BuildActivity(PresenceGameState state, DateTimeOffset startTime)
            {
                if(_settings.DiscordRPCFormat.TryGetValue(state, out var format))
                {
                    return ActivityFromFormat(format.GetNext(), state, startTime);
                }
                return DefaultFallbackActivity;
            }

            private static Activity ActivityFromFormat(RichPresenceSettings.GSActivityFormat format, PresenceGameState state, DateTimeOffset startTime)
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
                    activity.Party = GetParty(PartyGuid.ToString());
                    if(PresenceFormatter.Get<bool>(nameof(PresenceManager.HasLobby)))
                    {
                        var secrets = GetSecrets(PresenceFormatter.Get(nameof(PresenceManager.LobbyID)).ToString());
                        if (secrets.HasValue)
                            activity.Secrets = secrets.Value;
                    }
                }

                return activity;
            }

            internal static bool TryUpdateActivity(Discord.Activity activity)
            {
                if (_activityManager == null) return false;
                
                if(_settings.DEBUG_RichPresenceLogSpam)
                {
                    ClientLogger.Notice($"Activity updated: Details:{activity.Details} State:{activity.State}");
                    _activityManager.UpdateActivity(activity, ActivityUpdateDebugLog);
                    return true;
                }

                _activityManager.UpdateActivity(activity, ActivityVoidLog);
                return true;
            }

            public static void Dispose()
            {
                if(_activityManager != null)
                {
                    _activityManager.OnActivityJoin -= _activityManager_OnActivityJoin;
                    _activityManager.ClearActivity((result) => ClientLogger.Debug($"Activity clear result: {result}"));
                }
                
                _discordClient?.Dispose();
                _discordClient = null;
            }

            private static void ActivityUpdateDebugLog(Result result)
            {
                ClientLogger.Debug($"Activity update result: {result}");
            }

            private static void ActivityVoidLog(Result result)
            {
                if(result != Result.Ok)
                {
                    ClientLogger.Error("Update Activity failed!");
                }
            }

            private static void LogHook(LogLevel level, string message)
            {
                var msg = $"{level}: {message}";
                switch (level)
                {
                    case LogLevel.Error:
                        ClientLogger.Error(msg);
                        return;
                    case LogLevel.Warn:
                        ClientLogger.Warning(msg);
                        return;
                    default:
                    case LogLevel.Info:
                        ClientLogger.Notice(msg);
                        return;
                    case LogLevel.Debug:
                        ClientLogger.Debug(msg);
                        return;
                }
            }

            public static void RunCallbacks()
            {
                _discordClient?.RunCallbacks();
            }
        }
    }
}
