using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using DiscordRPC;
using TheArchive.Core.Settings;
using TheArchive.Interfaces;
using UnityEngine;

namespace TheArchive.Core.Managers;

public partial class ArchiveDiscordManager
{
    public static bool HasBeenSetup => _settings != null;

    private static RichPresence _lastActivity;
    
    private static float _lastCheckedTime;
    public static bool IsEnabled { get; private set; }

    public static Guid PartyGuid { get; private set; } = Guid.NewGuid();

    private static RichPresenceSettings _settings;

    public static event Action<string> OnActivityJoin;

    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= Loader.LoaderWrapper.CreateLoggerInstance(nameof(ArchiveDiscordManager), ConsoleColor.Magenta);

    public static void Enable(RichPresenceSettings rpcSettings)
    {
        if (rpcSettings == null) throw new ArgumentNullException($"{nameof(rpcSettings)}");

        if (rpcSettings.DEBUG_UseDefaultSettings)
        {
            _settings = RichPresenceSettings.Default;
        }
        else
        {
            _settings = rpcSettings;
        }

        try
        {
            DiscordClient.Initialize();

            
            DiscordClient.RunCallbacks();
            IsEnabled = true;
        }
        catch(Exception ex)
        {
            Logger.Error($"Exception has been thrown in {nameof(ArchiveDiscordManager)}. {ex}: {ex.Message}");
            Logger.Exception(ex);
        }
    }

    public static void Disable()
    {
        if (!IsEnabled)
            return;
        
        DiscordClient.Dispose();
        IsEnabled = false;
    }

    public static void Update()
    {
        if (!IsEnabled) return;
        
        var time = Time.time;
        if(_lastCheckedTime + 5 <= time)
        {
            _lastCheckedTime = time;

            var activity = DiscordClient.BuildActivity(PresenceManager.CurrentState, PresenceManager.CurrentStateStartTime);

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

    public static void RenewPartyGuid()
    {
        PartyGuid = Guid.NewGuid();
    }
}