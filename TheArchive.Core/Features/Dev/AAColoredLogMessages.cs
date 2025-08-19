#if BepInEx
using System;
using BepInEx;
using BepInEx.Logging;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Loader;

namespace TheArchive.Features.Dev;

[EnableFeatureByDefault]
[HideInModSettings]
internal class AAColoredLogMessages : Feature
{
    public override string Name => "Colored Log Messages";
    
    public override FeatureGroup Group => FeatureGroups.Dev;

    public override string Description => "Allows for colored log messages to be logged.";

    /*
    public new static IArchiveLogger FeatureLogger { get; set; }

    public override void OnDatablocksReady()
    {
        FeatureLogger.Warning("Printing debug messages ...");
        FeatureLogger.Warning("");

        FeatureLogger.Info("Info");
        FeatureLogger.Warning("Warning");
        FeatureLogger.Error("Error");
        FeatureLogger.Notice("Notice");
        FeatureLogger.Success("Success");
        FeatureLogger.Fail("Fail");
        FeatureLogger.Debug("Debug");
        FeatureLogger.Msg(ConsoleColor.DarkMagenta, "Msg (DarkMagenta)");
        FeatureLogger.Msg(ConsoleColor.Blue, "Msg (Blue)");
        FeatureLogger.Exception(new Exception("Intentional exception"));

        FeatureLogger.Warning("");
        FeatureLogger.Warning("debug messages done!");
    }
    */

    [ArchivePatch(typeof(ConsoleLogListener), nameof(ConsoleLogListener.LogEvent))]
    public static class ConsoleLogListener__LogEvent__Patch
    {
        public static bool Prefix(object sender, LogEventArgs eventArgs)
        {
            if (sender is not ArManualLogSource arLogSource)
            {
                return true;
            }
            
            if (!BIE_LogSourceColorLookup.TryGetColor(arLogSource, out var color))
            {
                return true;
            }

            if (ConsoleManager.ConsoleStream == null)
            {
                return true;
            }
            
            var logLevelColor = eventArgs.Level.GetConsoleColor();
            var prefixColor = arLogSource.Color ?? logLevelColor;
            
            ConsoleManager.SetConsoleColor(logLevelColor);
            ConsoleManager.ConsoleStream.Write("[");
            ConsoleManager.SetConsoleColor(prefixColor);
            ConsoleManager.ConsoleStream.Write(GetLevelAndSourceString(eventArgs));
            ConsoleManager.SetConsoleColor(logLevelColor);
            ConsoleManager.ConsoleStream.Write("] ");
            
            ConsoleManager.SetConsoleColor(color);
            ConsoleManager.ConsoleStream.Write($"{eventArgs.Data}{Environment.NewLine}");

            ConsoleManager.SetConsoleColor(ConsoleColor.Gray);
            return false;
        }

        private static string GetLevelAndSourceString(LogEventArgs eventArgs)
        {
            return $"{eventArgs.Level,-7}:{eventArgs.Source.SourceName,10}";
        }
    }
}
#endif