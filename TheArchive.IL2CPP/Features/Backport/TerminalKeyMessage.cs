using HarmonyLib;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using TMPro;

namespace TheArchive.Features.Backport
{
    [EnableFeatureByDefault]
    [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFive)]
    public class TerminalKeyMessage : Feature
    {
        public override string Name => "R6+ Terminal Key / Zone Info";

        public override string Group => FeatureGroups.Backport;

        public override string Description => "Adds the following text at the start of every terminal:\n\"Welcome to <b>TERMINAL_XYZ</b>, located in <b>ZONE_XY</b>\"\n<size=75%>(except for reactor terminals ...)</size>";

        public new static IArchiveLogger FeatureLogger { get; set; }

        public static string GetKey(LG_ComputerTerminal terminal) => "TERMINAL_" + terminal.m_serialNumber;

#if IL2CPP
        private static readonly HashSet<IntPtr> _interpreterSet = new HashSet<IntPtr>();

        public override void OnDisable()
        {
            _interpreterSet.Clear();
        }

        // Add the current Terminal Key as well as the Zone you're in to the terminal text
        [RundownConstraint(Utils.RundownFlags.RundownFour, Utils.RundownFlags.RundownFive)]
        [ArchivePatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.AddOutput), new Type[] { typeof(string), typeof(bool) })]
        internal static class LG_ComputerTerminalCommandInterpreter_AddOutput_Patch
        {
            public static void Postfix(LG_ComputerTerminalCommandInterpreter __instance, string line)
            {
                var setKey = __instance.Pointer;
                var terminal = __instance.m_terminal;

                try
                {
                    if (line.Equals("---------------------------------------------------------------"))
                    {
                        if (!_interpreterSet.Contains(setKey))
                        {
                            FeatureLogger.Debug($"Key & Zone in Terminal: Step 1/2 [{GetKey(terminal)}]");
                            _interpreterSet.Add(setKey);
                        }
                        else
                        {
                            FeatureLogger.Debug($"Key & Zone in Terminal: Step 2/2 [{GetKey(terminal)}]");
                            _interpreterSet.Remove(setKey);

                            string extra = string.Empty;

                            if(terminal.SpawnNode?.m_zone != null) {
                                // Reactor terminals get setup before nav info is available => no location :x
                                extra = $", located in <b>{terminal.SpawnNode.m_zone.NavInfo.PrefixLong}_{terminal.SpawnNode.m_zone.NavInfo.Number}</b>";
                            }

                            __instance.AddOutput($"Welcome to <b>{GetKey(terminal)}</b>{extra}", true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
        }
#endif

#if MONO
        [ArchiveConstructorPatch(typeof(LG_ComputerTerminalCommandInterpreter), new Type[] { typeof(LG_ComputerTerminal), typeof(TextMeshPro) })]
        internal static class LG_ComputerTerminalCommandInterpreter_Constructor_Patch
        {
            static FieldAccessor<LG_ComputerTerminalCommandInterpreter, LG_ComputerTerminal> A_LG_ComputerTerminalCommandInterpreter_m_terminal = FieldAccessor<LG_ComputerTerminalCommandInterpreter, LG_ComputerTerminal>.GetAccessor("m_terminal");

            public static void AddInfo(LG_ComputerTerminalCommandInterpreter cmdInterpreter)
            {
                try
                {
                    var terminal = A_LG_ComputerTerminalCommandInterpreter_m_terminal.Get(cmdInterpreter);

                    var area = terminal.SpawnNode?.m_area;
                    string extra = string.Empty;

                    if(area != null)
                    {
                        // Reactor terminals get setup before nav info is available => no location :x
                        extra = $", located in <b>{area.m_zone.NavInfo.PrefixLong}_{area.m_zone.NavInfo.Number}</b>";
                    }

                    FeatureLogger.Debug($"Key & Zone in Terminal: [{GetKey(terminal)}]");
                    cmdInterpreter.AddOutput($"Welcome to <b>{GetKey(terminal)}</b>{extra}", true);
                }
                catch(Exception ex)
                {
                    FeatureLogger.Exception(ex);
                }
                
            }

            private static MethodInfo _MI_addInfo;
            public static void Init()
            {
                _MI_addInfo = typeof(LG_ComputerTerminalCommandInterpreter_Constructor_Patch).GetMethod(nameof(AddInfo));
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                bool secondLineFound = false;
                int counter = 0;
                foreach (var instr in instructions)
                {
                    // Look for 2nd load of string containing only a line of dashes (start of terminal output)
                    if (counter <= 2 && instr.opcode == OpCodes.Ldstr && instr.operand.ToString().Contains("-----------------------------------"))
                    {
                        counter++;
                        if (counter == 2)
                        {
                            counter++;
                            yield return instr;
                            secondLineFound = true;
                            continue;
                        }
                    }

                    if(secondLineFound) // wait for next call to AddOuput
                    {
                        if(instr.opcode == OpCodes.Call)
                        {
                            secondLineFound = false;
                            yield return instr;
                            yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                            yield return new CodeInstruction(OpCodes.Call, _MI_addInfo); //AddInfo(this);
                            continue;
                        }
                    }

                    
                    yield return instr;
                }
            }
        }
#endif
    }
}
