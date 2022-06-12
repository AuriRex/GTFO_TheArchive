using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.Exceptions;
using TheArchive.Core.Models;
using TheArchive.Utilities;

namespace TheArchive.Core
{
    internal class FeatureInternal
    {
        internal GameBuildInfo BuildInfo => _feature.BuildInfo;
        internal bool HasUpdateMethod => UpdateDelegate != null;
        internal Update UpdateDelegate { get; private set; }
        internal bool HasLateUpdateMethod => LateUpdateDelegate != null;
        internal LateUpdate LateUpdateDelegate { get; private set; }

        private Feature _feature;
        private HarmonyLib.Harmony _harmonyInstance;

        private List<Type> _patchTypes = new List<Type>();
        private HashSet<FeaturePatchInfo> _patchInfos = new HashSet<FeaturePatchInfo>();

        private static HashSet<string> _usedIdentifiers = new HashSet<string>();

        private FeatureInternal() { }

        internal static void CreateAndAssign(Feature feature)
        {
            var fi = new FeatureInternal();
            feature.FeatureInternal = fi;
            fi.Init(feature);
        }

        public delegate void Update();
        public delegate void LateUpdate();

        internal void Init(Feature feature)
        {
            _feature = feature;

            ArchiveLogger.Notice($"[{nameof(FeatureInternal)}] Initializing {_feature.Identifier} ...");

            if(_usedIdentifiers.Contains(_feature.Identifier))
            {
                throw new ArchivePatchDuplicateIDException($"Provided feature id \"{_feature.Identifier}\" has already been registered by {FeatureManager.GetById(_feature.Identifier)}!");
            }

            var updateMethod = _feature.GetType().GetMethods()
                .FirstOrDefault(mi => (mi.Name == "Update" || mi.GetCustomAttribute<IsUpdate>() != null)
                    && mi.GetParameters().Length == 0
                    && !mi.IsStatic
                    && RundownConstraintsMatch(mi)
                    && BuildNumberMatches(mi));

            var updateDelegate = updateMethod?.CreateDelegate(typeof(Update), _feature);
            if (updateDelegate != null)
            {
                ArchiveLogger.Debug($"[{nameof(FeatureInternal)}] {nameof(Update)} method found.");
                UpdateDelegate = (Update)updateDelegate;
            }

            var lateUpdateMethod = _feature.GetType().GetMethods()
                .FirstOrDefault(mi => (mi.Name == "LateUpdate" || mi.GetCustomAttribute<IsLateUpdate>() != null)
                    && mi.GetParameters().Length == 0
                    && !mi.IsStatic
                    && RundownConstraintsMatch(mi)
                    && BuildNumberMatches(mi));

            var lateUpdateDelegate = lateUpdateMethod?.CreateDelegate(typeof(LateUpdate), _feature);
            if(lateUpdateDelegate != null)
            {
                ArchiveLogger.Debug($"[{nameof(FeatureInternal)}] {nameof(LateUpdate)} method found.");
                LateUpdateDelegate = (LateUpdate)lateUpdateDelegate;
            }
                


            _harmonyInstance = new HarmonyLib.Harmony(_feature.Identifier);

            var potentialPatchTypes = _feature.GetType().GetNestedTypes(ArchivePatcher.AnyBindingFlags).Where(nt => nt.GetCustomAttribute<ArchivePatch>() != null);

            foreach(var type in potentialPatchTypes)
            {
                if(RundownConstraintsMatch(type) && BuildNumberMatches(type))
                {
                    _patchTypes.Add(type);
                    continue;
                }
                else
                {
                    ArchiveLogger.Debug($"[{nameof(FeatureInternal)}] {_feature.Identifier}: ignoring {type.FullName} (Rundown | Build not matching.)");
                }
            }

            ArchiveLogger.Notice($"[{nameof(FeatureInternal)}] Discovered {_patchTypes.Count} Patches matching constraints.");

            foreach (var patchType in _patchTypes)
            {
                var archivePatchInfo = patchType.GetCustomAttribute<ArchivePatch>();

                try
                {
                    if (!archivePatchInfo.HasType)
                    {
                        var typeMethod = patchType.GetMethods(Utils.AnyBindingFlagss)
                            .FirstOrDefault(mi => mi.ReturnType == typeof(Type)
                                && (mi.Name == "Type" || mi.GetCustomAttribute<IsTypeProvider>() != null)
                                && RundownConstraintsMatch(mi)
                                && BuildNumberMatches(mi));

                        if(typeMethod != null)
                        {
                            archivePatchInfo.Type = (Type) typeMethod.Invoke(null, new object[0]);
                            ArchiveLogger.Debug($"[{nameof(FeatureInternal)}] Discovered target Type for Patch \"{patchType.FullName}\" to be \"{archivePatchInfo.Type.FullName}\"");
                        }
                        else
                        {
                            throw new ArchivePatchNoTypeProvidedException($"Patch \"{patchType.FullName}\" has no Type to patch! Add a static method returning Type and decorate it with the {nameof(IsTypeProvider)} Attribute!");
                        }
                    }

                    MethodInfo original;

                    if (archivePatchInfo.ParameterTypes != null)
                    {
                        original = archivePatchInfo.Type.GetMethod(archivePatchInfo.MethodName, Utils.AnyBindingFlagss, null, archivePatchInfo.ParameterTypes, null);
                    }
                    else
                    {
                        original = archivePatchInfo.Type.GetMethod(archivePatchInfo.MethodName, Utils.AnyBindingFlagss);
                    }

                    if (original == null)
                    {
                        throw new ArchivePatchNoOriginalMethodException($"Method with name \"{archivePatchInfo.MethodName}\" couldn't be found in type \"{archivePatchInfo.Type.FullName}\", PatchClass: {patchType.FullName}.");
                    }

                    var prefixMethodInfo = patchType.GetMethods(Utils.AnyBindingFlagss)
                            .FirstOrDefault(mi => (mi.Name == "Prefix" || mi.GetCustomAttribute<IsPrefix>() != null)
                                && RundownConstraintsMatch(mi)
                                && BuildNumberMatches(mi));

                    var postfixMethodInfo = patchType.GetMethods(Utils.AnyBindingFlagss)
                            .FirstOrDefault(mi => (mi.Name == "Postfix" || mi.GetCustomAttribute<IsPostfix>() != null)
                                && RundownConstraintsMatch(mi)
                                && BuildNumberMatches(mi));

                    if (prefixMethodInfo == null && postfixMethodInfo == null)
                    {
                        throw new ArchivePatchNoPatchMethodException($"Patch class \"{patchType.FullName}\" doesn't contain a Prefix or Postfix method, at least one is required!");
                    }

                    _patchInfos.Add(new FeaturePatchInfo(original, prefixMethodInfo, postfixMethodInfo, archivePatchInfo));

                    try
                    {
                        var initMethod = patchType.GetMethods(Utils.AnyBindingFlagss)
                            .FirstOrDefault(mi => mi.IsStatic 
                                && (mi.Name == "Init" || mi.GetCustomAttribute<IsInitMethod>() != null)
                                && RundownConstraintsMatch(mi)
                                && BuildNumberMatches(mi));

                        var initMethodParameters = initMethod?.GetParameters();
                        if (initMethod != null)
                        {
                            ArchiveLogger.Debug($"[{nameof(FeatureInternal)}] invoking static Init method {initMethod.GetType().Name}.{initMethod.Name} on {_feature.Identifier}");
                            if (initMethodParameters.Length == 1 && initMethodParameters[0].ParameterType == typeof(GameBuildInfo))
                            {
                                initMethod.Invoke(null, new object[] { BuildInfo });
                            }
                            else
                            {
                                initMethod.Invoke(null, null);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        ArchiveLogger.Error($"[{nameof(FeatureInternal)}] static Init method on {_feature.Identifier} failed! - {ex}: {ex.Message}");
                        ArchiveLogger.Exception(ex);
                    }
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Error($"[{nameof(FeatureInternal)}] Patch discovery for \"{archivePatchInfo.Type.FullName}\" failed: {ex}: {ex.Message}");
                    ArchiveLogger.Exception(ex);
                }
            }

#pragma warning TODO: Check for config etc
            ApplyPatches();
            _feature.Enabled = true;

            _feature.Init();
        }

        private void ApplyPatches()
        {
            foreach(var patchInfo in _patchInfos)
            {
                try
                {
                    ArchiveLogger.Notice($"[{nameof(FeatureInternal)}] Patching {_feature.Identifier} : {patchInfo.ArchivePatchInfo.Type.FullName}.{patchInfo.ArchivePatchInfo.MethodName}()");
                    _harmonyInstance.Patch(patchInfo.OriginalMethod, patchInfo.HarmonyRefixMethod, patchInfo.HarmonyPostfixMethod);
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Error($"[{nameof(FeatureInternal)}] Patch {patchInfo.ArchivePatchInfo.Type.FullName}.{patchInfo.ArchivePatchInfo.MethodName}() failed! {ex}: {ex.Message}");
                    ArchiveLogger.Exception(ex);
                }
            }
        }

        internal void Enable()
        {
            if (_feature.Enabled) return;
            ApplyPatches();
            _feature.Enabled = true;
            _feature.OnEnable();
        }

        internal void Disable()
        {
            if (!_feature.Enabled) return;
            _harmonyInstance.UnpatchSelf();
            _feature.Enabled = false;
            _feature.OnDisable();
        }

        private class FeaturePatchInfo
        {
            internal ArchivePatch ArchivePatchInfo { get; private set; }
            internal MethodInfo OriginalMethod { get; private set; }
            internal HarmonyLib.HarmonyMethod HarmonyRefixMethod { get; private set; }
            internal HarmonyLib.HarmonyMethod HarmonyPostfixMethod { get; private set; }
            internal MethodInfo PrefixPatchMethod { get; private set; }
            internal MethodInfo PostfixPatchMethod { get; private set; }

            public FeaturePatchInfo(MethodInfo original, MethodInfo prefix, MethodInfo postfix, ArchivePatch archivePatch)
            {
                OriginalMethod = original;

                PrefixPatchMethod = prefix;
                PostfixPatchMethod = postfix;

                ArchivePatchInfo = archivePatch;

                if (prefix != null)
                    HarmonyRefixMethod = new HarmonyLib.HarmonyMethod(prefix);
                if(postfix != null)
                    HarmonyPostfixMethod = new HarmonyLib.HarmonyMethod(postfix);
            }
        }

        private bool RundownConstraintsMatch(MemberInfo memberInfo)
        {
            var rundownConstraints = memberInfo.GetCustomAttributes<RundownConstraint>().ToArray();

            if (rundownConstraints.Length == 0)
            {
                return true;
            }

            var rundown = BuildInfo.Rundown;
            foreach (var constraint in rundownConstraints)
            {
                if (!constraint.Matches(rundown))
                {
                    return false;
                }
            }
            return true;
        }

        private bool BuildNumberMatches(MemberInfo memberInfo)
        {
            var constraints = memberInfo.GetCustomAttributes<BuildConstraint>().ToArray();

            if (constraints.Length == 0)
                return true;

            int buildNumber = BuildInfo.BuildNumber;
            foreach (var constraint in constraints)
            {
                if (!constraint.Matches(buildNumber))
                    return false;
            }

            return true;
        }
    }
}
