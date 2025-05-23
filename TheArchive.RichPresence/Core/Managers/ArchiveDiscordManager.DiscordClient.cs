using System;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using DiscordRPC;
using DiscordRPC.Message;
using TheArchive.Core.Models;
using TheArchive.Core.Settings;
using TheArchive.Loader;
using TheArchive.Utilities;
using UnityEngine;
using EventType = DiscordRPC.EventType;
using ILogger = DiscordRPC.Logging.ILogger;

namespace TheArchive.Core.Managers;

public partial class ArchiveDiscordManager
{
    public static class DiscordClient
    {
        public const long DEFAULT_CLIENT_ID = 946141176338190346L;

        public static long UsedClientID { get; private set; }
        private static DiscordRpcClient _discordClient;

        private static ILogger _clientLogger;
        
        private static string _lastLobbyId;
        private static string _lastPartyHash;

        public static void Initialize(long clientId = DEFAULT_CLIENT_ID)
        {
            UsedClientID = clientId;
            
            _clientLogger = new DiscordLogger(LoaderWrapper.CreateLoggerInstance("DiscordClient", ConsoleColor.Magenta), DiscordRPC.Logging.LogLevel.Warning);
            
            _discordClient = new DiscordRpcClient(clientId.ToString(), autoEvents: false, logger: _clientLogger);

            _discordClient.RegisterUriScheme(steamAppID: ArchiveMod.GTFO_STEAM_APPID.ToString());

            _discordClient.OnReady += OnReady;
            _discordClient.OnJoin += OnJoin;

            _discordClient.Subscribe(EventType.Join);
            
            _discordClient.Initialize();
        }

        private static void OnJoin(object sender, JoinMessage args)
        {
            Logger.Notice($"OnJoin received! ({args.Secret})");
            OnActivityJoin?.Invoke(args.Secret);
        }

        private static void OnReady(object sender, ReadyMessage args)
        {
            Logger.Notice("Discord is ready!");
            var time = Time.realtimeSinceStartup;
            if (time < _lastCheckedTime + 5)
                return;
            
            _lastCheckedTime = time;
            var activity = BuildActivity(PresenceManager.CurrentState, PresenceManager.CurrentStateStartTime);
            if (TryUpdateActivity(activity))
            {
                _lastActivity = activity;
            }
        }

        private static readonly RichPresence DefaultFallbackActivity = new()
        {
            Details = "???",
            State = "err:// no c0nnec7ion",
            Assets = new Assets
            {
                LargeImageKey = "gtfo_icon",
                LargeImageText = "GTFO",
            }
        };

        private static Party GetParty(string partyId = null)
        {
            return new Party
            {
                ID = partyId,
                Size = PresenceFormatter.Get<int>("MaxPlayerSlots") - PresenceFormatter.Get<int>("OpenSlots"),
                Max = PresenceFormatter.Get<int>("MaxPlayerSlots")
            };
        }
        
        private static Secrets GetSecrets(string joinSecret = null)
        {
            if (joinSecret == null)
                return null;
            
            return new Secrets
            {
                JoinSecret = joinSecret,
            };
        }

        private static Timestamps GetTimestamp(ulong? startTime = null, ulong? endTime = null)
        {
            return new Timestamps
            {
                StartUnixMilliseconds = startTime * 1000,
                EndUnixMilliseconds = endTime * 1000,
            };
        }

        internal static RichPresence BuildActivity(PresenceGameState state, DateTimeOffset startTime)
        {
            if (!_settings.DiscordRPCFormat.TryGetValue(state, out var format))
                return DefaultFallbackActivity;
            
            RichPresenceSettings.GSActivity nextActivityFormat = format;

            // Check for sub activities!
            if (!format.HasSubActivities)
                return ActivityFromFormat(nextActivityFormat.GetNext(), state, startTime);
            
            foreach(var subAct in format.SubActivities)
            {
                try
                {
                    if (subAct.DisplayConditionsAnyMode)
                    {
                        // Any condition true to enter
                        var anyTrue = false;
                        foreach (var dCond in subAct.DisplayConditions)
                        {
                            var value = dCond.Format();
                            if (value == "True" || value == "!False")
                                anyTrue = true;
                        }
                        if (!anyTrue)
                            throw null!;
                    }
                    else
                    {
                        foreach (var dCond in subAct.DisplayConditions)
                        {
                            var value = dCond.Format();
                            if (value != "True" && value != "!False")
                                throw null!;
                        }
                    }
                    // Use sub activity instead
                    nextActivityFormat = subAct;
                    break;
                }
                catch
                {
                    // ignored
                }
            }

            return ActivityFromFormat(nextActivityFormat.GetNext(), state, startTime);
        }

        private static RichPresence ActivityFromFormat(RichPresenceSettings.GSActivityFormat format, PresenceGameState state, DateTimeOffset startTime)
        {
            if (format == null) return DefaultFallbackActivity;

            var extra = ("state", state.ToString());

            var activity = new RichPresence
            {
                Details = format.Details?.Format(extra),
                State = format.Status?.Format(extra),
            };

            activity.Assets = new Assets
            {
                LargeImageKey = format.Assets.LargeImageKey?.Format(extra),
                LargeImageText = format.Assets.LargeTooltip?.Format(extra),
                SmallImageKey = format.Assets.SmallImageKey?.Format(extra),
                SmallImageText = format.Assets.SmallTooltip?.Format(extra)
            };

            if (format.DisplayStateTimeElapsed)
            {
                activity.Timestamps = GetTimestamp((ulong) startTime.ToUnixTimeSeconds());
            }
            else
            {
                if(!string.IsNullOrWhiteSpace(format.CustomTimeProvider) && ulong.TryParse(format.CustomTimeProvider.Format(), out var unixTime))
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
                var hasLobby = PresenceFormatter.Get<bool>(nameof(PresenceManager.HasLobby));
                
                if(hasLobby)
                {
                    var lobbyId = PresenceFormatter.Get(nameof(PresenceManager.LobbyID)).ToString();
                    activity.Party = GetParty(GetPartyID(lobbyId));
                    activity.Secrets = GetSecrets(lobbyId);
                }
                else
                {
                    activity.Party = GetParty(PartyGuid.ToString());
                }
            }

            return activity;
        }

        private static string GetPartyID(string lobbyId)
        {
            var hasChanged = _lastLobbyId != lobbyId;
            _lastLobbyId = lobbyId;
            
            if (string.IsNullOrWhiteSpace(lobbyId))
                return PartyGuid.ToString();

            if (!hasChanged)
                return _lastPartyHash;
            
            var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(lobbyId));

            var hashedString = Convert.ToBase64String(hashedBytes);

            if (hashedString.Length > 128)
            {
                hashedString = hashedString.Substring(0, 128);
            }

            _lastPartyHash = hashedString;
            
            return hashedString;
        }

        internal static bool TryUpdateActivity(RichPresence activity)
        {
            if (_discordClient == null)
                return false;
                
            if(_settings.DEBUG_RichPresenceLogSpam)
            {
                Logger.Notice($"Activity updated: Details:{activity.Details} State:{activity.State}");
            }

            _discordClient.SetPresence(activity);
            return true;
        }

        public static void Dispose()
        {
            if (_discordClient != null)
            {
                _discordClient.OnReady += OnReady;
                _discordClient.OnJoin += OnJoin;
                _discordClient.Dispose();
            }
  
            _discordClient = null;
        }

        public static void RunCallbacks()
        {
            _discordClient.Invoke();
        }
    }
}