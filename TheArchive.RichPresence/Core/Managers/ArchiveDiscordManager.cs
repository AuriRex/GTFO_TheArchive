using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using TheArchive.Core.Discord;
using TheArchive.Core.Models;
using TheArchive.Core.Settings;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Core.Managers;

public class ArchiveDiscordManager
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

    public static Guid PartyGuid { get; private set; } = Guid.NewGuid();

    private static RichPresenceSettings _settings = null;
    private static bool _internalDisabled = false;

    public static event Action<string> OnActivityJoin;

    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger
    {
        get
        {
            return _logger ??= Loader.LoaderWrapper.CreateLoggerInstance(nameof(ArchiveDiscordManager), ConsoleColor.Magenta);
        }
    }

    public static void Enable(RichPresenceSettings rpcSettings)
    {
        if (_internalDisabled) return;
        if (rpcSettings == null) throw new ArgumentNullException($"{nameof(rpcSettings)}");

        if (rpcSettings.DEBUG_UseDefaultSettings)
        {
            _settings = RichPresenceSettings.Default;
        }
        else
        {
            _settings = rpcSettings;
        }
            
        if(!_hasDiscordDllBeenLoaded)
        {
            try
            {
                var path = Path.Combine(Assembly.GetExecutingAssembly().Location, "discord_game_sdk.dll");
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

            var time = Time.time;
            if (_lastCheckedTime + 5 <= time)
            {
                _lastCheckedTime = time;
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
            Logger.Error($"Exception has been thrown in {nameof(ArchiveDiscordManager)}. {ex}: {ex.Message}");
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

        var time = Time.time;
        if(_lastCheckedTime + 5 <= time)
        {
            _lastCheckedTime = time;

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
            _activityManager.RegisterSteam(ArchiveMod.GTFO_STEAM_APPID); // GTFO App ID

            _activityManager.OnActivityJoin += _activityManager_OnActivityJoin;
        }

        private static void _activityManager_OnActivityJoin(string secret)
        {
            OnActivityJoin?.Invoke(secret);
        }

        private static readonly Activity DefaultFallbackActivity = new()
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

        private static ActivityParty GetParty(string partyId = null)
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

        private static ActivitySecrets? GetSecrets(string joinSecret = null)
        {
            if (joinSecret == null) return null;
            return new ActivitySecrets
            {
                Join = joinSecret,
            };
        }

        private static ActivityTimestamps GetTimestamp(long startTime = 0, long endTime = 0)
        {
            return new ActivityTimestamps
            {
                Start = startTime,
                End = endTime
            };
        }

        internal static Activity BuildActivity(PresenceGameState state, DateTimeOffset startTime)
        {
            if(_settings.DiscordRPCFormat.TryGetValue(state, out var format))
            {
                RichPresenceSettings.GSActivity nextActivityFormat = format;

                // Check for sub activities!
                if(format.HasSubActivities)
                {
                    foreach(var subAct in format.SubActivities)
                    {
                        try
                        {
                            if (subAct.DisplayConditionsAnyMode)
                            {
                                // Any condition true to enter
                                bool anyTrue = false;
                                foreach (var dCond in subAct.DisplayConditions)
                                {
                                    var value = dCond.Format();
                                    if (value == "True" || value == "!False")
                                        anyTrue = true;
                                }
                                if (!anyTrue)
                                    throw null;
                            }
                            else
                            {
                                foreach (var dCond in subAct.DisplayConditions)
                                {
                                    var value = dCond.Format();
                                    if (value != "True" && value != "!False")
                                        throw null;
                                }
                            }
                            // Use sub activity instead
                            nextActivityFormat = subAct;
                            break;
                        } catch { }
                    }
                }

                return ActivityFromFormat(nextActivityFormat.GetNext(), state, startTime);
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

            if (format.DisplayStateTimeElapsed)
            {
                activity.Timestamps = GetTimestamp(startTime.ToUnixTimeSeconds());
            }
            else
            {
                if(!string.IsNullOrWhiteSpace(format.CustomTimeProvider) && long.TryParse(format.CustomTimeProvider.Format(), out var unixTime))
                {
                    if(format.CustomProviderIsEndTime)
                    {
                        activity.Timestamps = GetTimestamp(endTime: unixTime);
                    }
                    else
                    {
                        activity.Timestamps = GetTimestamp(startTime: unixTime);
                    }
                }
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
                _activityManager.ClearActivity((result) =>
                {
                    ClientLogger.Debug($"Activity clear result: {result}");
                    DisposeClient();
                });
                _activityManager = null;
            }
            else
            {
                DisposeClient();
            }
        }

        private static void DisposeClient()
        {
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