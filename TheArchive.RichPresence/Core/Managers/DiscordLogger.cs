using System;
using DiscordRPC.Logging;
using TheArchive.Interfaces;

namespace TheArchive.Core.Managers;

public class DiscordLogger : ILogger
{
    public LogLevel Level { get; set; }
    
    private readonly IArchiveLogger _logger;

    public DiscordLogger(IArchiveLogger archiveLogger, LogLevel logLevel)
    {
        _logger = archiveLogger;
        Level = logLevel;
    }
    
    public void Trace(string message, params object[] args)
    {
        if (Level > LogLevel.Trace)
            return;

        var msg = message;
        if (args.Length > 0)
        {
            msg = string.Format(message, args);
        }
        
        _logger.Debug(msg);
    }

    public void Info(string message, params object[] args)
    {
        if (Level > LogLevel.Info)
            return;
        
        var msg = message;
        if (args.Length > 0)
        {
            msg = string.Format(message, args);
        }
        
        _logger.Msg(ConsoleColor.DarkMagenta, msg);
    }

    public void Warning(string message, params object[] args)
    {
        if (Level > LogLevel.Warning)
            return;
        
        var msg = message;
        if (args.Length > 0)
        {
            msg = string.Format(message, args);
        }
        
        _logger.Warning(msg);
    }

    public void Error(string message, params object[] args)
    {
        if (Level > LogLevel.Error)
            return;
        
        var msg = message;
        if (args.Length > 0)
        {
            msg = string.Format(message, args);
        }
        
        _logger.Error(msg);
    }
}