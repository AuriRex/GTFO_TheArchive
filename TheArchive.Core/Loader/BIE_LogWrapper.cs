#if BepInEx
using BepInEx.Logging;
using System;
using TheArchive.Interfaces;

namespace TheArchive.Loader;

internal class BIE_LogWrapper : IArchiveLogger
{
    private readonly ManualLogSource _logger;
    public BIE_LogWrapper(ManualLogSource logger)
    {
        _logger = logger;
        if (!Logger.Sources.Contains(_logger))
            Logger.Sources.Add(_logger);
    }

    public void Success(string msg)
    {
        BIE_LogSourceColorLookup.SetColor(_logger, ConsoleColor.Green);
        _logger.LogMessage(msg);
    }

    public void Notice(string msg)
    {
        BIE_LogSourceColorLookup.SetColor(_logger, ConsoleColor.Cyan);
        _logger.LogMessage(msg);
    }

    public void Info(string msg) => _logger.LogInfo(msg);

    public void Fail(string msg)
    {
        BIE_LogSourceColorLookup.SetColor(_logger, ConsoleColor.Red);
        _logger.LogInfo(msg);
    }

    public void Msg(ConsoleColor col, string msg)
    {
        BIE_LogSourceColorLookup.SetColor(_logger, col);
        _logger.LogMessage(msg);
    }

    public void Msg(string msg) => _logger.LogMessage(msg);

    public void Debug(string msg) => _logger.LogDebug(msg);

    public void Warning(string msg) => _logger.LogWarning(msg);

    public void Error(string msg) => _logger.LogError(msg);

    public void Exception(Exception ex) => _logger.LogError($"{ex}: {ex.Message}\n{ex.StackTrace}");
}
#endif
