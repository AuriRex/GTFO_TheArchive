using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using TheArchive.Core.Settings;
using TheArchive.Interfaces;
using UnityEngine;

namespace TheArchive.Core.Managers;

public partial class ArchiveDiscordManager
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

    private static bool _hasDiscordDllBeenLoaded;
    private static float _lastCheckedTime;
    public static bool IsEnabled { get; private set; }

    public static Guid PartyGuid { get; private set; } = Guid.NewGuid();

    private static RichPresenceSettings _settings;
    private static bool _internalDisabled;

    public static event Action<string> OnActivityJoin;

    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= Loader.LoaderWrapper.CreateLoggerInstance(nameof(ArchiveDiscordManager), ConsoleColor.Magenta);

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
            IsEnabled = true;
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
        if (!IsEnabled)
            return;
        
        DiscordClient.Dispose();
        IsEnabled = false;
    }

    public static void Update()
    {
        if (_internalDisabled) return;
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