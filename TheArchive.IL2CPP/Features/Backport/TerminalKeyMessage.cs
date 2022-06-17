using LevelGeneration;
using System;
using System.Collections.Generic;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Utilities;

namespace TheArchive.Features.Backport
{
    [EnableFeatureByDefault]
    [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFive)]
    public class TerminalKeyMessage : Feature
    {
        public override string Name => "R6+ Terminal Key / Zone Info";

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
                try
                {
                    if (line.Equals("---------------------------------------------------------------"))
                    {
                        var terminal = __instance.m_terminal;
                        if (_interpreterSet.Contains(__instance.Pointer))
                        {
                            ArchiveLogger.Debug($"Key & Zone in Terminal: Step 2/2 [{GetKey(terminal)}]");
                            _interpreterSet.Remove(__instance.Pointer);
                            __instance.AddOutput($"Welcome to {GetKey(terminal)}, located in {terminal.SpawnNode.m_zone.NavInfo.PrefixLong}_{terminal.SpawnNode.m_zone.NavInfo.Number}", true);
                        }
                        else
                        {
                            ArchiveLogger.Debug($"Key & Zone in Terminal: Step 1/2 [{GetKey(terminal)}]");
                            _interpreterSet.Add(__instance.Pointer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
#else
            public static void Postfix(LG_ComputerTerminalCommandInterpreter __instance, ref LG_ComputerTerminal ___m_terminal, string line)
            {
                try
                {
                    if (line.Equals("---------------------------------------------------------------"))
                    {

                        if (_interpreterSet.Contains(__instance))
                        {
                            ArchiveLogger.Debug($"Key & Zone in Terminal: Step 2/2 [{GetKey(___m_terminal)}]");
                            _interpreterSet.Remove(__instance);
                            __instance.AddOutput($"Welcome to {GetKey(___m_terminal)}, located in {___m_terminal.SpawnNode.m_zone.NavInfo.PrefixLong}_{___m_terminal.SpawnNode.m_zone.NavInfo.Number}", true);
                        }
                        else
                        {
                            ArchiveLogger.Debug($"Key & Zone in Terminal: Step 1/2 [{GetKey(___m_terminal)}]");
                            _interpreterSet.Add(__instance);
                        }

                    }
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
#endif
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
