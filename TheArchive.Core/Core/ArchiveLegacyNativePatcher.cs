using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using TheArchive.Loader;
using TheArchive.Utilities;
#if MelonLoader
using UnhollowerBaseLib;
#endif
using static TheArchive.Core.ArchiveLegacyPatcher;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core;
#pragma warning disable CS0618 // Type or member is obsolete
public class ArchiveLegacyNativePatcher
{
    private RundownID _currentRundown;
    private Type[] _patchTypes;
    private List<LegacyNativePatchInstance> _patchInstances = new List<LegacyNativePatchInstance>();

    public ArchiveLegacyNativePatcher()
    {

    }

    public void ApplyNativePatches(RundownID currentRundown, Assembly assembly)
    {
        _currentRundown = currentRundown;

        ArchiveLogger.Msg(ConsoleColor.Magenta, $"Applying Native Patches for assembly \"{assembly.GetName().Name}\" ...");

        if(_patchTypes == null)
            _patchTypes = GetAllTypesWithPatchAttribute(typeof(ArchiveLegacyNativePatch) ,assembly);

        foreach (var patchContainingType in _patchTypes)
        {
            ArchiveLegacyNativePatch nativePatchInfo;
            try
            {
                nativePatchInfo = patchContainingType.GetCustomAttribute<ArchiveLegacyNativePatch>();

                BindPatchToSetting bindPatchToSettingsInfo;
                TryGetBindToSettingsAttribute(patchContainingType, out bindPatchToSettingsInfo);


                if (!IsPatchEnabledInSettings(ArchiveMod.Settings, bindPatchToSettingsInfo, patchContainingType.FullName))
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"[{bindPatchToSettingsInfo.BindToSetting}==false] Skipped native patch: \"{patchContainingType.FullName}\". ({nativePatchInfo.RundownsToPatch})");
                    continue;
                }


                if (!nativePatchInfo.GeneralPurposePatch && !FlagsContain(nativePatchInfo.RundownsToPatch, currentRundown))
                {
                    ArchiveLogger.Warning($"Not native patching method \"{nativePatchInfo.MethodName}\" in type \"{(nativePatchInfo.HasType ? nativePatchInfo.Type?.FullName : "(not yet set for type load reasons)")}\" from patch class: \"{patchContainingType.FullName}\". ({nativePatchInfo.RundownsToPatch})");
                    continue;
                }

                if (!nativePatchInfo.HasType)
                {
                    if (ArchiveLegacyPatcher.TryGetMethodByName(patchContainingType, "Type", out var typeMethod) && typeMethod.ReturnType == typeof(Type))
                    {
                        nativePatchInfo.Type = (Type)typeMethod.Invoke(null, new object[0]);
                        ArchiveLogger.Debug($"Discovered target Type for native Patch \"{patchContainingType.FullName}\" to be \"{nativePatchInfo.Type.FullName}\"");
                    }
                    else
                    {
                        throw new ArgumentException($"Native Patch \"{patchContainingType.FullName}\" has no static method Type() returning the Type to patch or none set in its Attribute!");
                    }
                }

                if (ArchiveLegacyPatcher.TryGetMethodByName(patchContainingType, "ParameterTypes", out var paramTypesMethod))
                {
                    ArchiveLogger.Debug($"Patch \"{patchContainingType.FullName}\" - found ParameterTypes method.");
                    var parameterTypes = (Type[])paramTypesMethod.Invoke(null, null);
                    nativePatchInfo.ParameterTypes = parameterTypes;
                }


                var nPatch = LegacyNativePatchInstance.CreatePatch(nativePatchInfo, patchContainingType);

                _patchInstances.Add(nPatch);

                var logPrefix = GetPatchPrefix(bindPatchToSettingsInfo, nativePatchInfo.GeneralPurposePatch);

                var logMessage = $"{logPrefix}Native Patching \"{nativePatchInfo.Type.FullName}.{nativePatchInfo.MethodName}()\" ({patchContainingType.Name})";

                if (bindPatchToSettingsInfo?.CustomLogColor != null)
                {
                    ArchiveLogger.Msg(bindPatchToSettingsInfo.CustomLogColor.Value, logMessage);
                }
                else
                {
                    ArchiveLogger.Notice(logMessage);
                }

                if(nPatch.WasNotAbleToSetProperty)
                {
                    ArchiveLogger.Warning($"Native Patch \"{nativePatchInfo.Type.FullName}\" has not gotten its {nameof(LegacyNativePatchInstance)} injected! Create a static set-able property called \"{nameof(LegacyNativePatchInstance)}\" to get it injected.");
                }

            }
            catch(Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }
                

        }


            

    }

    /// <summary>
    /// Patch things using MelonUtils.NativeHookAttach, WIP
    /// </summary>
    [Obsolete("Just a janky mess, wouldn't recommend using")]
    public class ArchiveLegacyNativePatch : Attribute
    {
        public bool HasType
        {
            get
            {
                return Type != null;
            }
        }

        public int ParameterCount
        {
            get
            {
                return ParameterTypes?.Length ?? 0;
            }
        }

        public string ReturnTypeString
        {
            get
            {
                return ReturnType?.Name ?? "void";
            }
        }

        public Type Type { get; internal set; }

        public Type ReturnType { get; set; }

        public string MethodName { get; private set; }

        public RundownFlags RundownsToPatch { get; private set; }

        public bool GeneralPurposePatch { get; private set; } = false;

        public Type[] ParameterTypes { get; internal set; }

        public ArchiveLegacyNativePatch(Type type, string methodName, RundownFlags rundowns, Type[] parameterTypes = null, Type returnType = null)
        {
            Type = type;
            MethodName = methodName;
            RundownsToPatch = rundowns;
            ParameterTypes = parameterTypes;
            ReturnType = returnType;
        }

        public ArchiveLegacyNativePatch(Type type, string methodName, RundownFlags from, RundownFlags to, Type[] parameterTypes = null, Type returnType = null)
        {
            Type = type;
            MethodName = methodName;
            RundownsToPatch = from.To(to);
            ParameterTypes = parameterTypes;
            ReturnType = returnType;
        }

        public ArchiveLegacyNativePatch(Type type, string methodName, Type[] parameterTypes = null, Type returnType = null)
        {
            Type = type;
            MethodName = methodName;
            ParameterTypes = parameterTypes;
            ReturnType = returnType;
            GeneralPurposePatch = true;
        }
    }

    public class LegacyNativePatchInstance
    {
        public Type DelegateType { get; }
        public Delegate OriginalMethod { get; }

        public bool WasNotAbleToSetProperty { get; } = false;

        public const string kReplacementMethodName = "Replacement";

        private LegacyNativePatchInstance(ArchiveLegacyNativePatch nativePatchInfo, Type patchContainingType)
        {
            if (!ArchiveLegacyPatcher.TryGetMethodByName(nativePatchInfo.Type, nativePatchInfo.MethodName, out var originalMethodInfo))
            {
                throw new ArgumentException($"Could not find the original method \"{nativePatchInfo.Type.FullName}.{nativePatchInfo.MethodName}\" targeted by patch \"{patchContainingType.FullName}\"");
            }

            if (!ArchiveLegacyPatcher.TryGetMethodByName(patchContainingType, kReplacementMethodName, out var replacementMethodInfo))
            {
                throw new ArgumentException($"Could not find \"{kReplacementMethodName}\" method for patch \"{patchContainingType.FullName}\"!");
            }

            DelegateType = NativeDelegates.Get(nativePatchInfo);

#if MelonLoader
                unsafe
                {
                    var ptr = *(IntPtr*) (IntPtr) UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(originalMethodInfo).GetValue(null);

                    // Create a dynamic assembly, type and static method with our parameters maybe?? + the delegate type
                    // and use that to provide a better experience for native patches (similar to how harmony does it with Pre & Postfix)
                    // https://www.c-sharpcorner.com/UploadFile/87b416/dynamically-create-a-class-at-runtime/

                    // original method calls -> new class static method
                    // -> converts IntPtr to actual objects and passes them to -> Prefix (and Postfix later)
                    // -> if prefix returnes true -> don't call original method, else do
                    // -> call Postfix

                    var patch = replacementMethodInfo.MethodHandle.GetFunctionPointer();

                    LoaderWrapper.NativeHookAttach((IntPtr) (&ptr), patch);

                    OriginalMethod = Marshal.GetDelegateForFunctionPointer(ptr, DelegateType);
                }

                var prop = patchContainingType.GetProperty(nameof(LegacyNativePatchInstance), ArchiveLegacyPatcher.AnyBindingFlags);

                if(prop != null && prop.SetMethod != null && prop.SetMethod.IsStatic)
                {
                    prop.SetValue(null, this);
                }
                else
                {
                    WasNotAbleToSetProperty = true;
                }
#endif
        }

        internal static LegacyNativePatchInstance CreatePatch(ArchiveLegacyNativePatch nativePatchInfo, Type patchContainingType)
        {
            return new LegacyNativePatchInstance(nativePatchInfo, patchContainingType);
        }
    }

    public static class NativeDelegates
    {
        internal static Type Get(ArchiveLegacyNativePatch nativePatchInfo)
        {
            string delName = $"{nativePatchInfo.ReturnTypeString}_Param_{nativePatchInfo.ParameterCount}";

            var del = typeof(NativeDelegates).GetNestedType(delName, ArchiveLegacyPatcher.AnyBindingFlags);

            if (del == null) throw new NotImplementedException(delName);

            return del;
        }

        public delegate /*byte*/ void void_Param_0(IntPtr s, IntPtr n);
        public delegate void void_Param_1(IntPtr s, IntPtr a, IntPtr n);
        public delegate void void_Param_2(IntPtr s, IntPtr a, IntPtr b, IntPtr n);
        public delegate void void_Param_3(IntPtr s, IntPtr a, IntPtr b, IntPtr c, IntPtr n);
    }
}
#pragma warning restore CS0618 // Type or member is obsolete