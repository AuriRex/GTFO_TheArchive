using MelonLoader;
using System;
using TheArchive.Interfaces;

namespace TheArchive.Utilities
{
    internal class ML_LogWrapper : IArchiveLogger
    {
        private MelonLogger.Instance _logger;
        public ML_LogWrapper(MelonLogger.Instance logger)
        {
            _logger = logger;
        }

        public void Success(string msg) => _logger.Msg(ConsoleColor.Green, msg);

        public void Notice(string msg) => _logger.Msg(ConsoleColor.Cyan, msg);

        public void Info(string msg) => _logger.Msg(msg);

        public void Fail(string msg) => _logger.Msg(ConsoleColor.Red, msg);

        public void Msg(ConsoleColor col, string msg) => _logger.Msg(col, msg);

        public void Msg(string msg) => _logger.Msg(msg);

        public void Debug(string msg) => _logger.Msg(ConsoleColor.DarkGray, msg);

        public void Warning(string msg) => _logger.Warning(msg);

        public void Error(string msg) => _logger.Error(msg);

        public void Exception(Exception ex) => _logger.Error($"{ex}: {ex.Message}\n{ex.StackTrace}");
    }
}
