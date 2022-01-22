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


        public ArchivePatcher(HarmonyLib.Harmony harmonyInstance, string name)
        {
            _harmonyInstance = harmonyInstance;
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

                    BindPatchToSetting bindPatchToSettingsInfo = type.GetCustomAttribute<BindPatchToSetting>();

                    if(bindPatchToSettingsInfo == null)
                    {
                        try
                        {
                            var containerTypeName = type.FullName.Split('.').Last().Split('+')?.First();
                            var containerType = type.Assembly.GetType($"{type.Namespace}.{containerTypeName}");
                            bindPatchToSettingsInfo = containerType?.GetCustomAttribute<BindPatchToSetting>();
                            ArchiveLogger.Debug($"Determined ContainerType: {containerTypeName}->\"{containerType?.FullName}\" for type \"{type.FullName}\"{(bindPatchToSettingsInfo != null ? $", {nameof(BindPatchToSetting)} {bindPatchToSettingsInfo.BindToSetting} found!": string.Empty)}.");
                        }
                        catch (Exception ex)
                        {
                            ArchiveLogger.Error($"{ex}: {ex.Message}");
                            ArchiveLogger.Error(ex.StackTrace);
                        }
                    }


                    if (!string.IsNullOrEmpty(bindPatchToSettingsInfo?.BindToSetting))
                    {
                        try
                        {
                            if (!settingForString.TryGetValue(bindPatchToSettingsInfo.BindToSetting, out PropertyInfo pi))
                            {
                                pi = typeof(Core.ArchiveSettings).GetProperty(bindPatchToSettingsInfo.BindToSetting, AccessTools.all);
                                if (pi == null || pi.PropertyType != typeof(bool))
                                {
                                    throw new ArgumentException();
                                }
                                settingForString.Add(bindPatchToSettingsInfo.BindToSetting, pi);
                            }

                            var shouldEnable = (bool) pi.GetValue(ArchiveMod.Settings);

                            if (!shouldEnable)
                            {
                                ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"[{bindPatchToSettingsInfo.BindToSetting}==false] Skipped patch: \"{type.FullName}\". ({archivePatchInfo.RundownsToPatch})");
                                continue;
                            }
                        }
                        catch (ArgumentException)
                        {
                            ArchiveLogger.Error($"Patch \"{type.FullName}\" has an invalid Settings string \"{bindPatchToSettingsInfo.BindToSetting}\" set!");
                        }
                    }

                    if (!archivePatchInfo.GeneralPurposePatch && !FlagsContain(archivePatchInfo.RundownsToPatch, currentRundown))
                    {
                        ArchiveLogger.Warning($"Not patching method \"{archivePatchInfo.MethodName}\" in type \"{(archivePatchInfo.HasType ? archivePatchInfo.Type?.FullName : "(not yet set for type load reasons)")}\" from patch class: \"{type.FullName}\". ({archivePatchInfo.RundownsToPatch})");
                        continue;
                    }

                    if(!archivePatchInfo.HasType)
                    {
                        if (TryGetMethodByName(type, "Type", out var typeMethod) && typeMethod.ReturnType == typeof(Type))
                        {
                            archivePatchInfo.Type = (Type) typeMethod.Invoke(null, new object[0]);
                            ArchiveLogger.Debug($"Discovered target Type for Patch \"{type.FullName}\" to be \"{archivePatchInfo.Type.FullName}\"");
                        }
                        else
                        {
                            throw new ArgumentException($"{type.FullName} has no static method Type() returning Type or none set in its Attribute!");
                        }
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

                    var logPrefix = GetPatchPrefix(archivePatchInfo, bindPatchToSettingsInfo);

                    var logMessage = $"{logPrefix}Patching \"{archivePatchInfo.Type.FullName}.{original.Name}()\" ({type.Name})";

                    if(bindPatchToSettingsInfo?.CustomLogColor != null)
                    {
                        ArchiveLogger.Msg(bindPatchToSettingsInfo.CustomLogColor.Value, logMessage);
                    }
                    else
                    {
                        ArchiveLogger.Notice(logMessage);
                    }

                    _harmonyInstance.Patch(original, prefix, postfix);
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Error($"Patch in \"{archivePatchInfo.Type.FullName}\" FAILED! {ex}: {ex.Message}");
                    ArchiveLogger.Error($"{ex.StackTrace}");
                }
            }

            IsPatched = true;
        }

        private string GetPatchPrefix(ArchivePatch archivePatchInfo, BindPatchToSetting bindPatchToSettingsInfo)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(bindPatchToSettingsInfo?.BindToSetting))
            {
                if(!string.IsNullOrEmpty(bindPatchToSettingsInfo.CustomLogPrefix))
                {
                    result += $"[{bindPatchToSettingsInfo.CustomLogPrefix}] ";
                }
                else
                {
                    result += $"[{bindPatchToSettingsInfo.BindToSetting}] ";
                }
            }
            if(archivePatchInfo.GeneralPurposePatch)
            {
                result += $"[GP] ";
            }
            return result;
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

        /// <summary>
        /// Add this to either the patch class or a container class to apply the patch only if a specific setting is set
        /// </summary>
        public class BindPatchToSetting : Attribute
        {
            public string BindToSetting { get; private set; } = string.Empty;
            public string CustomLogPrefix { get; private set; } = null;
            public ConsoleColor? CustomLogColor { get; private set; } = null;

            public BindPatchToSetting(string bindToSetting, string customLogPrefix = null)
            {
                BindToSetting = bindToSetting;
                CustomLogPrefix = customLogPrefix;
            }

            public BindPatchToSetting(string bindToSetting, string customLogPrefix, ConsoleColor customLogColor)
            {
                BindToSetting = bindToSetting;
                CustomLogPrefix = customLogPrefix;
                CustomLogColor = customLogColor;
            }

            public BindPatchToSetting(string bindToSetting, ConsoleColor customLogColor, string customLogPrefix = null)
            {
                BindToSetting = bindToSetting;
                CustomLogPrefix = customLogPrefix;
                CustomLogColor = customLogColor;
            }
        }

        public class ArchivePatch : Attribute
        {
            public bool HasType
            {
                get
                {
                    return Type != null;
                }
            }

            public Type Type { get; internal set; }

            public string MethodName { get; private set; }

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

            public ArchivePatch(Type type, string methodName, RundownFlags from, RundownFlags to, Type[] parameterTypes = null)
            {
                Type = type;
                MethodName = methodName;
                RundownsToPatch = from.To(to);
                ParameterTypes = parameterTypes;
            }

            public ArchivePatch(Type type, string methodName, Type[] parameterTypes = null)
            {
                Type = type;
                MethodName = methodName;
                ParameterTypes = parameterTypes;
                GeneralPurposePatch = true;
            }

        }

    }
}
