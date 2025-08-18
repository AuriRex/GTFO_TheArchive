using System;

namespace TheArchive.Interfaces;

/// <summary>
/// Archives custom logging interface
/// </summary>
public interface IArchiveLogger
{
    /// <summary>
    /// Logs a green success info message.
    /// </summary>
    /// <param name="msg">Message</param>
    public void Success(string msg);
    
    /// <summary>
    /// Logs a light-blue notice message.
    /// </summary>
    /// <param name="msg">Message</param>
    public void Notice(string msg);
    
    /// <summary>
    /// Logs a custom colored message.
    /// </summary>
    /// <param name="col">Console Color to use.</param>
    /// <param name="msg">Message</param>
    public void Msg(ConsoleColor col, string msg);
    
    /// <summary>
    /// Logs a white colored info message.
    /// </summary>
    /// <param name="msg">Message</param>
    public void Info(string msg);
    
    /// <summary>
    /// Logs a red colored fail info message.
    /// </summary>
    /// <param name="msg">Message</param>
    public void Fail(string msg);
    
    /// <summary>
    /// Logs a gray debug message.
    /// </summary>
    /// <param name="msg">Message</param>
    public void Debug(string msg);
    
    /// <summary>
    /// Logs a yellow colored warning message.
    /// </summary>
    /// <param name="msg">Message</param>
    public void Warning(string msg);
    
    /// <summary>
    /// Logs a red colored error message.
    /// </summary>
    /// <param name="msg">Message</param>
    public void Error(string msg);
    
    /// <summary>
    /// Logs the exception message and the stacktrace.
    /// </summary>
    /// <param name="ex">Thrown exception to log</param>
    public void Exception(Exception ex);
}