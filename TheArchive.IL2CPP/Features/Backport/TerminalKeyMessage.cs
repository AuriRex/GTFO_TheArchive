using LevelGeneration;
using System;
using System.Collections.Generic;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.Backport
{
    [EnableFeatureByDefault]
    [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFive)]
    public class TerminalKeyMessage : Feature
    {
        public override string Name => "R6+ Terminal Key / Zone Info";

        public override string Group => FeatureGroups.Backport;

#if IL2CPP
        private static HashSet<IntPtr> _interpreterSet = new HashSet<IntPtr>();
#else
        private static HashSet<LG_ComputerTerminalCommandInterpreter> _interpreterSet = new HashSet<LG_ComputerTerminalCommandInterpreter>();
#endif

        public override void OnDisable()
        {
            _interpreterSet.Clear();
        }

        // Add the current Terminal Key as well as the Zone you're in to the terminal text
        [RundownConstraint(Utils.RundownFlags.RundownTwo, Utils.RundownFlags.RundownFive)]
        [ArchivePatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.AddOutput), new Type[] { typeof(string), typeof(bool) })]
        internal static class LG_ComputerTerminalCommandInterpreter_AddOutputPatch
        {
            public static string GetKey(LG_ComputerTerminal terminal) => "TERMINAL_" + terminal.m_serialNumber;

#if IL2CPP
            public static void Postfix(LG_ComputerTerminalCommandInterpreter __instance, string line)
            {
                var setKey = __instance.Pointer;
                var terminal = __instance.m_terminal;
#else
            public static void Postfix(LG_ComputerTerminalCommandInterpreter __instance, ref LG_ComputerTerminal ___m_terminal, string line)

            {
                var setKey = __instance;
                var terminal = ___m_terminal;
#endif

                try
                {
                    if (line.Equals("---------------------------------------------------------------"))
                    {
                        if (!_interpreterSet.Contains(setKey))
                        {
                            ArchiveLogger.Debug($"Key & Zone in Terminal: Step 1/2 [{GetKey(terminal)}]");
                            _interpreterSet.Add(setKey);
                        }
                        else
                        {
                            ArchiveLogger.Debug($"Key & Zone in Terminal: Step 2/2 [{GetKey(terminal)}]");
                            _interpreterSet.Remove(setKey);
                            __instance.AddOutput($"Welcome to <b>{GetKey(terminal)}</b>, located in <b>{terminal.SpawnNode.m_zone.NavInfo.PrefixLong}_{terminal.SpawnNode.m_zone.NavInfo.Number}</b>", true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
        }

#if MONO
        [RundownConstraint(Utils.RundownFlags.RundownOne)]
        [ArchivePatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.AddOutput), new Type[] { typeof(string), typeof(bool) })]
        internal static class LG_ComputerTerminalCommandInterpreter_AddOutputPatch_R1
        {
            static FieldAccessor<LG_ComputerTerminalCommandInterpreter, LG_ComputerTerminal> A_LG_ComputerTerminalCommandInterpreter_m_terminal = FieldAccessor<LG_ComputerTerminalCommandInterpreter, LG_ComputerTerminal>.GetAccessor("m_terminal");

            public static void Postfix(LG_ComputerTerminalCommandInterpreter __instance, string line)
            {
                try
                {
                    var m_terminal = A_LG_ComputerTerminalCommandInterpreter_m_terminal.Get(__instance);
                    LG_ComputerTerminalCommandInterpreter_AddOutputPatch.Postfix(__instance, ref m_terminal, line);
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
        }
#endif
    }
}
