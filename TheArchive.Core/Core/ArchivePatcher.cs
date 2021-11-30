using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core
{
    public class ArchivePatcher
    {
        private readonly HarmonyLib.Harmony _harmonyInstance;
        private readonly string _name;
        private readonly List<(MethodInfo, MethodInfo)> _patchedMethodList = new List<(MethodInfo, MethodInfo)>();
        private Type[] _patchTypes;
        public bool IsPatched { get; private set; } = false;

        private uint? rundownId = null;
        internal void SetRundownFromId(uint rundownId)
        {
            this.rundownId = rundownId;
        }

        public ArchivePatcher(HarmonyLib.Harmony harmonyInstance, string name)
        {
            _harmonyInstance = ArchiveMod.Instance.HarmonyInstance;
            _name = name;
        }

        private Type[] GetAllTypesWithPatchAttribute(Assembly assembly = null) => (assembly ?? Assembly.GetExecutingAssembly()).GetTypes().Where(x => x.GetCustomAttribute(typeof(ArchivePatch)) != null).ToArray();

        private bool TryGetMethodByName(Type type, string name, out MethodInfo methodInfo)
        {
            if (type.GetMethods(AnyBindingFlags).Any(x => x.Name.Equals(name)))
            {
                methodInfo = type.GetMethod(name, AnyBindingFlags);
                return true;
            }
            methodInfo = null;
            return false;
        }

        public void PatchRundownSpecificMethods(Assembly assembly = null)
        {
            if (IsPatched) return;

            ArchiveLogger.Notice($"{_name} - Applying patches ...");

            if (_patchTypes == null)
                _patchTypes = GetAllTypesWithPatchAttribute(assembly);

            _patchedMethodList.Clear();

            RundownID currentRundown = ArchiveMod.CurrentRundown;

            Dictionary<string, PropertyInfo> settingForString = new Dictionary<string, PropertyInfo>();

            foreach (Type type in _patchTypes)
            {
                ArchivePatch archivePatchInfo = null;

                try
                {
                    archivePatchInfo = type.GetCustomAttribute<ArchivePatch>();

                    if (!string.IsNullOrEmpty(archivePatchInfo.BindToSetting))
                    {
                        try
                        {
                            if (!settingForString.TryGetValue(archivePatchInfo.BindToSetting, out PropertyInfo pi))
                            {
                                pi = typeof(Core.ArchiveSettings).GetProperty(archivePatchInfo.BindToSetting, AccessTools.all);
                                if (pi == null || pi.PropertyType != typeof(bool))
                                {
                                    throw new ArgumentException();
                                }
                                settingForString.Add(archivePatchInfo.BindToSetting, pi);
                            }

                            var shouldEnable = (bool) pi.GetValue(ArchiveMod.Settings);

                            if (!shouldEnable)
                            {
                                ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"[{archivePatchInfo.BindToSetting}==false] Skipped patch: \"{type.FullName}\". ({archivePatchInfo.RundownsToPatch})");
                                continue;
                            }
                        }
                        catch (ArgumentException)
                        {
                            ArchiveLogger.Error($"Patch \"{type.FullName}\" has an invalid Settings string \"{archivePatchInfo.BindToSetting}\" set!");
                        }
                    }

                    if (!archivePatchInfo.GeneralPurposePatch && !FlagsContain(archivePatchInfo.RundownsToPatch, currentRundown))
                    {
                        ArchiveLogger.Warning($"Not patching method \"{archivePatchInfo.MethodName}\" in type \"{archivePatchInfo.Type.FullName}\" from patch class: \"{type.FullName}\". ({archivePatchInfo.RundownsToPatch})");
                        continue;
                    }

                    MethodInfo original;

                    if (archivePatchInfo.ParameterTypes != null)
                    {
                        original = archivePatchInfo.Type.GetMethod(archivePatchInfo.MethodName, AnyBindingFlags, null, archivePatchInfo.ParameterTypes, null);
                    }
                    else
                    {
                        original = archivePatchInfo.Type.GetMethod(archivePatchInfo.MethodName, AnyBindingFlags);
                    }

                    if (original == null)
                    {
                        ArchiveLogger.Error($"[ERROR] Method with name \"{archivePatchInfo.MethodName}\" couldn't be found in type \"{archivePatchInfo.Type.FullName}\", PatchClass: {type.FullName}.");
                        continue;
                    }

                    HarmonyMethod prefix = null;
                    HarmonyMethod postfix = null;
                    MethodInfo methodInfo;

                    if (TryGetMethodByName(type, "Prefix", out methodInfo))
                    {
                        prefix = new HarmonyMethod(methodInfo);
                        _patchedMethodList.Add((original, methodInfo));
                    }

                    if (TryGetMethodByName(type, "Postfix", out methodInfo))
                    {
                        postfix = new HarmonyMethod(methodInfo);
                        _patchedMethodList.Add((original, methodInfo));
                    }

                    if (prefix == null && postfix == null)
                    {
                        ArchiveLogger.Error($"Patch class \"{type.FullName}\" doesn't contain a Prefix and/or Postfix method!");
                        continue;
                    }

                    ArchiveLogger.Notice($"{(archivePatchInfo.GeneralPurposePatch ? "[GP] " : string.Empty)}Patching \"{archivePatchInfo.Type.FullName}.{original.Name}()\"");

                    _harmonyInstance.Patch(original, prefix, postfix);
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Error($"[ERROR] Patch in \"{archivePatchInfo.Type.FullName}\" FAILED! {ex.Message}");
                    ArchiveLogger.Error($"[ERROR] {ex.StackTrace}");
                }
            }

            IsPatched = true;
        }

        public void Unpatch()
        {
            if (!IsPatched) return;

            ArchiveLogger.Notice($"Unpatching Manual Patches ...");

            foreach ((MethodInfo Original, MethodInfo Patch) values in _patchedMethodList)
            {
                _harmonyInstance.Unpatch(values.Original, values.Patch);
            }

            _patchedMethodList.Clear();

            IsPatched = false;
        }

        public const BindingFlags AnyBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        public class ArchivePatch : Attribute
        {
            public Type Type { get; private set; }
            public string MethodName { get; private set; }

            public string BindToSetting { get; private set; } = string.Empty;

            public RundownFlags RundownsToPatch { get; private set; }

            public bool GeneralPurposePatch { get; private set; } = false;

            public Type[] ParameterTypes { get; internal set; }

            public ArchivePatch(Type type, string methodName, RundownFlags rundowns, Type[] parameterTypes = null)
            {
                Type = type;
                MethodName = methodName;
                RundownsToPatch = rundowns;
                ParameterTypes = parameterTypes;
            }
            public ArchivePatch(Type type, string methodName, RundownFlags rundowns, string bindToSetting, Type[] parameterTypes = null) : this(type, methodName, rundowns, parameterTypes)
            {
                BindToSetting = bindToSetting;
            }

            public ArchivePatch(Type type, string methodName, RundownFlags from, RundownFlags to, Type[] parameterTypes = null)
            {
                Type = type;
                MethodName = methodName;
                RundownsToPatch = from.To(to);
                ParameterTypes = parameterTypes;
            }

            public ArchivePatch(Type type, string methodName, RundownFlags from, RundownFlags to, string bindToSetting, Type[] parameterTypes = null) : this(type, methodName, from, to, parameterTypes)
            {
                BindToSetting = bindToSetting;
            }

            public ArchivePatch(Type type, string methodName, Type[] parameterTypes = null)
            {
                Type = type;
                MethodName = methodName;
                ParameterTypes = parameterTypes;
                GeneralPurposePatch = true;
            }

            public ArchivePatch(Type type, string methodName, string bindToSetting, Type[] parameterTypes = null) : this(type, methodName, parameterTypes)
            {
                BindToSetting = bindToSetting;
            }

        }

    }
}
