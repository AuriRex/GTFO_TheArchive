using System;
using System.Collections.Generic;
using TheArchive.Interfaces;
using TheArchive.Loader;

namespace TheArchive.Core.ModulesAPI;

internal static class CustomSettingManager
{
    private static readonly IArchiveLogger _logger = LoaderWrapper.CreateArSubLoggerInstance(nameof(CustomSettingManager), ConsoleColor.DarkRed);

    public static void RegisterModuleSetting(ICustomSetting setting)
    {
        if (setting.LoadingTime == LoadingTime.Immediately)
        {
            try
            {
                setting.Load();
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception thrown in {nameof(RegisterModuleSetting)}! {ex}: {ex.Message}");
                _logger.Exception(ex);
            }
        }
        _moduleSettings.Add(setting);
    }

    public static void OnApplicationQuit()
    {
        foreach (var setting in _moduleSettings)
        {
            try
            {
                if (setting.SaveOnQuit)
                    setting.Save();
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception thrown in {nameof(OnApplicationQuit)}! {ex}: {ex.Message}");
                _logger.Exception(ex);
            }
        }
    }

    public static void OnGameDataInited()
    {
        foreach (var setting in _moduleSettings)
        {
            try
            {
                if (setting.LoadingTime == LoadingTime.AfterGameDataInited)
                    setting.Load();
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception thrown in {nameof(OnGameDataInited)}! {ex}: {ex.Message}");
                _logger.Exception(ex);
            }
        }
    }

    private static readonly HashSet<ICustomSetting> _moduleSettings = new();
}
