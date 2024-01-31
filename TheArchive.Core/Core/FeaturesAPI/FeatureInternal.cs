using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.Exceptions;
using TheArchive.Core.FeaturesAPI.Components;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Localization;
using TheArchive.Core.Managers;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.FeaturesAPI
{
    internal class FeatureInternal
    {
        internal FeatureLocalizationService Localization { get; } = new();
        internal static GameBuildInfo BuildInfo => Feature.BuildInfo;
        internal bool InternalDisabled { get; private set; } = false;
        internal InternalDisabledReason DisabledReason { get; private set; }
        internal bool HasUpdateMethod => UpdateDelegate != null;
        internal Update UpdateDelegate { get; private set; }
        internal bool HasLateUpdateMethod => LateUpdateDelegate != null;
        internal LateUpdate LateUpdateDelegate { get; private set; }
        internal bool HideInModSettings { get; private set; }
        internal bool DoNotSaveToConfig { get; private set; }
        internal bool AutomatedFeature { get; private set; }
        internal bool DisableModSettingsButton { get; private set; }
        internal bool HasAdditionalSettings => _settingsHelpers.Count > 0;
        internal bool AllAdditionalSettingsAreHidden { get; private set; } = true;
        internal bool InitialEnabledState { get; private set; } = false;
        internal IEnumerable<FeatureSettingsHelper> Settings => _settingsHelpers;
        internal Utils.RundownFlags Rundowns { get; private set; } = Utils.RundownFlags.None;
        internal IArchiveLogger FeatureLoggerInstance { get; private set; }
        internal Assembly OriginAssembly { get; private set; }
        internal IArchiveModule ArchiveModule { get; private set; }
        internal string DisplayName
        {
            get
            {
                string propID = $"{_feature.GetType().FullName}.{nameof(Feature.Name)}";
                if (Localization.TryGetFSText(propID, FSType.FName, out var text))
                {
                    return text;
                }
                return _feature.Name;
            }
        }
        internal string DisplayDescription
        {
            get
            {
                string propID = $"{_feature.GetType().FullName}.{nameof(Feature.Description)}";
                if (Localization.TryGetFSText(propID, FSType.FDescription, out var text))
                {
                    return text;
                }
                return _feature.Description;
            }
        }

        private string _asmGroupName;
        internal string AsmGroupName
        {
            get
            {
                if (string.IsNullOrEmpty(_asmGroupName))
                {
                    _asmGroupName = OriginAssembly.GetCustomAttribute<ModDefaultFeatureGroupName>()?.DefaultGroupName ?? OriginAssembly.GetName().Name;
                }
                return _asmGroupName;
            }
        }
        internal string CriticalInfo
        {
            get
            {
                if (InternalDisabled)
                {
                    return string.Format("<#F00>{0}</color>: {1}", LocalizationCoreService.Get(2, "DISABLED"), LocalizationCoreService.Get(DisabledReason));
                }

                return string.Empty;
            }
        }

        private Feature _feature;
        private HarmonyLib.Harmony _harmonyInstance;

        private readonly List<Type> _patchTypes = new List<Type>();
        private readonly HashSet<FeaturePatchInfo> _patchInfos = new HashSet<FeaturePatchInfo>();
        private readonly HashSet<FeatureSettingsHelper> _settingsHelpers = new HashSet<FeatureSettingsHelper>();
        private readonly Dictionary<string, object> _storage = new Dictionary<string, object>();
        private PropertyInfo _isEnabledPropertyInfo;
        private bool _onGameStateChangedMethodUsesGameEnum = false;
        private MethodInfo _onGameStateChangedMethodInfo;
        private MethodInfo _onLGAreaCullUpdateMethodInfo;

        private static readonly HashSet<string> _usedIdentifiers = new HashSet<string>();
        private static readonly IArchiveLogger _FILogger = LoaderWrapper.CreateArSubLoggerInstance(nameof(FeatureInternal), ConsoleColor.DarkYellow);

        private static Type _gameStateType;
        private static Type _lgAreaType;

        private FeatureInternal() { }

        internal static void CreateAndAssign(Feature feature, IArchiveModule module)
        {
            if (_gameStateType == null)
            {
                _gameStateType = ImplementationManager.GameTypeByIdentifier("eGameStateName");
            }
            if (_lgAreaType == null)
            {
                _lgAreaType = ImplementationManager.GameTypeByIdentifier("LG_Area");
            }
            var fi = new FeatureInternal();
            feature.FeatureInternal = fi;
            try
            {
                fi.Init(feature, module);
            }
            catch (TypeLoadException tle)
            {
                _FILogger.Error($"Initialization of {fi._feature.Identifier} failed! - {tle.GetType().FullName}");
                _FILogger.Warning($"!!! PLEASE FIX THIS !!!");
                _FILogger.Debug($"StackTrace:\n{tle.Message}\n{tle.StackTrace}");
                fi.InternalyDisableFeature(InternalDisabledReason.TypeLoadException);
                _FILogger.Msg(ConsoleColor.Magenta, $"Feature \"{fi._feature.Identifier}\" has been disabled internally! ({fi.DisabledReason})");
                return;
            }
            fi.AfterInit();
        }

        public delegate void Update();
        public delegate void LateUpdate();

        internal void Init(Feature feature, IArchiveModule module)
        {
            _feature = feature;

            ArchiveModule = module;

            var featureType = _feature.GetType();
            OriginAssembly = featureType.Assembly;

            feature.FeatureInternal.Localization.Setup(feature, LocalFiles.LoadFeatureLocalizationText(feature));

            _FILogger.Msg(ConsoleColor.Black, "-");
            _FILogger.Msg(ConsoleColor.Green, $"Initializing {_feature.Identifier} ...");

            if (_usedIdentifiers.Contains(_feature.Identifier))
            {
                throw new ArchivePatchDuplicateIDException($"Provided feature id \"{_feature.Identifier}\" has already been registered by {FeatureManager.GetById(_feature.Identifier)}!");
            }

            FeatureLoggerInstance = LoaderWrapper.CreateArSubLoggerInstance($"F::{_feature.Identifier}", ConsoleColor.Cyan);

            HideInModSettings = featureType.GetCustomAttribute<HideInModSettings>() != null;
            DoNotSaveToConfig = featureType.GetCustomAttribute<DoNotSaveToConfig>() != null;
            AutomatedFeature = featureType.GetCustomAttribute<AutomatedFeature>() != null;
            DisableModSettingsButton = featureType.GetCustomAttribute<DisallowInGameToggle>() != null;

            if (featureType.GetCustomAttribute<ForceDisable>() != null)
            {
                InternalDisabled = true;
                DisabledReason |= InternalDisabledReason.ForceDisabled;
            }

            foreach (var constraint in featureType.GetCustomAttributes<RundownConstraint>())
            {
                Rundowns |= constraint.Rundowns;
            }

            if (!AnyRundownConstraintMatches(featureType))
            {
                InternalDisabled = true;
                DisabledReason |= InternalDisabledReason.RundownConstraintMismatch;
            }

            if (!AnyBuildConstraintMatches(featureType))
            {
                InternalDisabled = true;
                DisabledReason |= InternalDisabledReason.BuildConstraintMismatch;
            }

            try
            {
                if (!_feature.ShouldInit())
                {
                    InternalDisabled = true;
                    DisabledReason |= InternalDisabledReason.DisabledViaShouldInit;
                }
            }
            catch (Exception ex)
            {
                _FILogger.Error($"{nameof(Feature.ShouldInit)} method on {nameof(Feature)} failed: {ex}: {ex.Message}");
                _FILogger.Exception(ex);
                InternalDisabled = true;
                DisabledReason |= InternalDisabledReason.ShouldInitFailed;
            }

            if (InternalDisabled)
            {
                _FILogger.Msg(ConsoleColor.Magenta, $"Feature \"{_feature.Identifier}\" has been disabled internally! ({DisabledReason})");
                return;
            }

            var featureMethods = featureType.GetMethods();

            var updateMethod = featureMethods
                .FirstOrDefault(mi => (mi.Name == "Update" || mi.GetCustomAttribute<IsUpdate>() != null)
                    && mi.GetParameters().Length == 0
                    && !mi.IsStatic
                    && mi.DeclaringType != typeof(Feature)
                    && AnyRundownConstraintMatches(mi)
                    && AnyBuildConstraintMatches(mi));

            var updateDelegate = updateMethod?.CreateDelegate(typeof(Update), _feature);
            if (updateDelegate != null)
            {
                _FILogger.Debug($"{nameof(Update)} method found.");
                UpdateDelegate = (Update)updateDelegate;
            }

            var lateUpdateMethod = featureMethods
                .FirstOrDefault(mi => (mi.Name == "LateUpdate" || mi.GetCustomAttribute<IsLateUpdate>() != null)
                    && mi.GetParameters().Length == 0
                    && !mi.IsStatic
                    && mi.DeclaringType != typeof(Feature)
                    && AnyRundownConstraintMatches(mi)
                    && AnyBuildConstraintMatches(mi));

            var lateUpdateDelegate = lateUpdateMethod?.CreateDelegate(typeof(LateUpdate), _feature);
            if (lateUpdateDelegate != null)
            {
                _FILogger.Debug($"{nameof(LateUpdate)} method found.");
                LateUpdateDelegate = (LateUpdate)lateUpdateDelegate;
            }

            var featureProperties = featureType.GetProperties();

            var settingsProps = featureProperties
                .Where(pi => pi.GetCustomAttribute<FeatureConfig>() != null);

            _isEnabledPropertyInfo = featureProperties
                .FirstOrDefault(pi => (pi.Name == "IsEnabled" || pi.GetCustomAttribute<SetEnabledStatus>() != null)
                    && pi.SetMethod != null
                    && pi.GetMethod.IsStatic
                    && pi.GetMethod.ReturnType == typeof(bool));

            if (_isEnabledPropertyInfo != null)
            {
                _FILogger.Debug($"Found IsEnabled property \"{_isEnabledPropertyInfo.Name}\" on Feature {_feature.Identifier}.");
            }

            var staticLoggerInstancePropertyInfo = featureProperties
                .FirstOrDefault(pi => (pi.Name == "FeatureLogger" || pi.GetCustomAttribute<SetStaticLogger>() != null)
                    && pi.SetMethod != null
                    && pi.GetMethod != null
                    && pi.GetMethod.IsStatic
                    && pi.GetMethod.ReturnType == typeof(IArchiveLogger));

            if (staticLoggerInstancePropertyInfo != null)
            {
                _FILogger.Debug($"Found FeatureLogger property \"{staticLoggerInstancePropertyInfo.Name}\" on Feature {_feature.Identifier}. Populating ...");
                staticLoggerInstancePropertyInfo.SetValue(null, FeatureLoggerInstance);
            }

            _onGameStateChangedMethodInfo = featureMethods
                .FirstOrDefault(mi => (mi.Name == nameof(Feature.OnGameStateChanged) || mi.GetCustomAttribute<IsGameStateChangedMethod>() != null)
                    && !mi.IsStatic
                    && mi.DeclaringType != typeof(Feature)
                    && mi.GetParameters().Length == 1
                    && (mi.GetParameters()[0].ParameterType == _gameStateType || mi.GetParameters()[0].ParameterType == typeof(int)));

            if (_onGameStateChangedMethodInfo != null)
            {
                if (_onGameStateChangedMethodInfo.GetParameters()[0].ParameterType == _gameStateType)
                    _onGameStateChangedMethodUsesGameEnum = true;
                _FILogger.Debug($"Found {nameof(Feature.OnGameStateChanged)} method \"{_onGameStateChangedMethodInfo.Name}\" on Feature {_feature.Identifier}. (Uses {(_onGameStateChangedMethodUsesGameEnum ? "eGameStateName" : "int")})");
            }

            _onLGAreaCullUpdateMethodInfo = featureMethods
                .FirstOrDefault(mi => (mi.Name == "OnAreaCull" || mi.GetCustomAttribute<IsAreaCullUpdateMethod>() != null)
                    && !mi.IsStatic
                    && mi.DeclaringType != typeof(Feature)
                    && mi.GetParameters().Length == 2
                    && (mi.GetParameters()[0].ParameterType == _lgAreaType || mi.GetParameters()[0].ParameterType == typeof(object))
                    && mi.GetParameters()[1].ParameterType == typeof(bool));

            foreach (var prop in settingsProps)
            {
                if ((!prop.SetMethod?.IsStatic ?? true) || (!prop.GetMethod?.IsStatic ?? true))
                {
                    _FILogger.Warning($"Feature \"{_feature.Identifier}\" has an invalid property \"{prop.Name}\" with a {nameof(FeatureConfig)} attribute! Make sure it's static with both a get and set method!");
                }
                else
                {
                    _settingsHelpers.Add(new FeatureSettingsHelper(_feature, prop));
                }
            }

            _harmonyInstance = new HarmonyLib.Harmony($"{ArchiveMod.MOD_NAME}_FeaturesAPI_{_feature.Identifier}");

            var potentialPatchTypes = featureType.GetNestedTypes(Utils.AnyBindingFlagss).Where(nt => nt.GetCustomAttribute<ArchivePatch>() != null);

            foreach (var type in potentialPatchTypes)
            {
                if (AnyRundownConstraintMatches(type) && AnyBuildConstraintMatches(type))
                {
                    _patchTypes.Add(type);
                    continue;
                }
                else
                {
                    _FILogger.Debug($"{_feature.Identifier}: ignoring {type.FullName} (Rundown | Build not matching.)");
                }
            }

            _FILogger.Notice($"Discovered {_patchTypes.Count} Patch{(_patchTypes.Count == 1 ? string.Empty : "es")} matching constraints.");

            foreach (var patchType in _patchTypes)
            {
                var archivePatchInfo = patchType.GetCustomAttribute<ArchivePatch>();

                try
                {
                    var patchTypePatchInfoProperty = patchType.GetProperty("PatchInfo");

                    if (patchTypePatchInfoProperty != null
                        && patchTypePatchInfoProperty.GetMethod != null
                        && patchTypePatchInfoProperty.GetMethod.IsStatic
                        && patchTypePatchInfoProperty.GetMethod.ReturnType == typeof(ArchivePatch)
                        && patchTypePatchInfoProperty.SetMethod != null)
                    {
                        patchTypePatchInfoProperty.SetValue(null, archivePatchInfo);
                        _FILogger.Debug($"Populated PatchInfo Property for Patch \"{patchType.FullName}\".");
                    }

                    var patchTypeMethods = patchType.GetMethods(Utils.AnyBindingFlagss);

                    if (!archivePatchInfo.HasType)
                    {
                        var typeMethod = patchTypeMethods
                            .FirstOrDefault(mi => mi.ReturnType == typeof(Type)
                                && (mi.Name == "Type" || mi.GetCustomAttribute<IsTypeProvider>() != null)
                                && AnyRundownConstraintMatches(mi)
                                && AnyBuildConstraintMatches(mi));

                        if (typeMethod != null)
                        {
                            if (!typeMethod.IsStatic)
                            {
                                throw new ArchivePatchMethodNotStaticException($"Method \"{typeMethod.Name}\" in Feature \"{feature.Identifier}\" must be static!");
                            }

                            archivePatchInfo.Type = (Type)typeMethod.Invoke(null, null);
                            _FILogger.Debug($"Discovered target Type for Patch \"{patchType.FullName}\" to be \"{archivePatchInfo.Type?.FullName ?? "TYPE NOT FOUND"}\"");
                        }
                        else
                        {
                            throw new ArchivePatchNoTypeProvidedException($"Patch \"{patchType.FullName}\" has no Type to patch! Add a static method returning Type and decorate it with the {nameof(IsTypeProvider)} Attribute!");
                        }
                    }

                    var parameterTypesMethod = patchTypeMethods
                            .FirstOrDefault(mi => mi.ReturnType == typeof(Type[])
                                && (mi.Name == "ParameterTypes" || mi.GetCustomAttribute<IsParameterTypesProvider>() != null)
                                && AnyRundownConstraintMatches(mi)
                                && AnyBuildConstraintMatches(mi));

                    if (parameterTypesMethod != null)
                    {
                        if (!parameterTypesMethod.IsStatic)
                            throw new ArchivePatchMethodNotStaticException($"Method \"{parameterTypesMethod.Name}\" in Feature \"{feature.Identifier}\" must be static!");

                        archivePatchInfo.ParameterTypes = (Type[])parameterTypesMethod.Invoke(null, null);
                    }

                    if (string.IsNullOrWhiteSpace(archivePatchInfo.MethodName))
                    {
                        var methodNameMethod = patchTypeMethods
                            .FirstOrDefault(mi => mi.ReturnType == typeof(string)
                                && (mi.Name == "MethodName" || mi.GetCustomAttribute<IsMethodNameProvider>() != null)
                                && AnyRundownConstraintMatches(mi)
                                && AnyBuildConstraintMatches(mi));
                        _FILogger.Debug($"Invoking static MethodNameProvider method {patchType.Name}.{methodNameMethod.Name} on {_feature.Identifier}");
                        archivePatchInfo.MethodName = (string)methodNameMethod.Invoke(null, null);
                    }

                    MethodBase original;

                    switch (archivePatchInfo.MethodType)
                    {
                        default:
                        case ArchivePatch.PatchMethodType.Method:
                            if (archivePatchInfo.ParameterTypes != null)
                            {
                                original = archivePatchInfo.Type.GetMethod(archivePatchInfo.MethodName, Utils.AnyBindingFlagss, null, archivePatchInfo.ParameterTypes, null);
                            }
                            else
                            {
                                original = archivePatchInfo.Type.GetMethod(archivePatchInfo.MethodName, Utils.AnyBindingFlagss);
                            }
                            break;
                        case ArchivePatch.PatchMethodType.Getter:
                            original = archivePatchInfo.Type.GetProperty(archivePatchInfo.MethodName, Utils.AnyBindingFlagss)?.GetMethod;
                            break;
                        case ArchivePatch.PatchMethodType.Setter:
                            original = archivePatchInfo.Type.GetProperty(archivePatchInfo.MethodName, Utils.AnyBindingFlagss)?.SetMethod;
                            break;
                        case ArchivePatch.PatchMethodType.Constructor:
                            original = archivePatchInfo.Type.GetConstructor(Utils.AnyBindingFlagss, null, archivePatchInfo.ParameterTypes, null);
                            break;
                    }


                    if (original == null)
                    {
                        throw new ArchivePatchNoOriginalMethodException($"{archivePatchInfo.MethodType} with name \"{archivePatchInfo.MethodName}\"{(archivePatchInfo.ParameterTypes != null ? $" with parameters [{string.Join(", ", archivePatchInfo.ParameterTypes.Select(type => type.Name))}]" : string.Empty)} couldn't be found in type \"{archivePatchInfo.Type.FullName}\", PatchClass: {patchType.FullName}.");
                    }

                    var originalMethodIsNative = LoaderWrapper.IsIL2CPPType(original.DeclaringType);

                    var prefixMethodInfo = patchTypeMethods
                            .FirstOrDefault(mi => (mi.Name == "Prefix" || mi.GetCustomAttribute<IsPrefix>() != null)
                                && AnyRundownConstraintMatches(mi)
                                && AnyBuildConstraintMatches(mi));

                    var postfixMethodInfo = patchTypeMethods
                            .FirstOrDefault(mi => (mi.Name == "Postfix" || mi.GetCustomAttribute<IsPostfix>() != null)
                                && AnyRundownConstraintMatches(mi)
                                && AnyBuildConstraintMatches(mi));

                    var finalizerMethodInfo = patchTypeMethods
                            .FirstOrDefault(mi => (mi.Name == "Finalizer" || mi.GetCustomAttribute<IsFinalizer>() != null)
                                && AnyRundownConstraintMatches(mi)
                                && AnyBuildConstraintMatches(mi));

                    var transpilerMethodInfo = patchTypeMethods
                            .FirstOrDefault(mi => (mi.Name == "Transpiler" || mi.GetCustomAttribute<IsTranspiler>() != null)
                                && AnyRundownConstraintMatches(mi)
                                && AnyBuildConstraintMatches(mi));

                    var ilManipulatorMethodInfo = patchTypeMethods
                            .FirstOrDefault(mi => (mi.Name == "ILManipulator" || mi.GetCustomAttribute<IsILManipulator>() != null)
                                && AnyRundownConstraintMatches(mi)
                                && AnyBuildConstraintMatches(mi));

                    if (transpilerMethodInfo != null && originalMethodIsNative)
                    {
                        _FILogger.Error($"Can't apply Transpiler \"{transpilerMethodInfo.Name}\" on native method \"{original.Name}\" from IL2CPP Type \"{original.DeclaringType.FullName}\"!");
                        _FILogger.Warning("This Transpiler is going to be skipped, things might break!");
                        transpilerMethodInfo = null;
                    }

                    if (ilManipulatorMethodInfo != null && originalMethodIsNative)
                    {
                        _FILogger.Error($"Can't apply ILManipulator \"{ilManipulatorMethodInfo.Name}\" on native method \"{original.Name}\" from IL2CPP Type \"{original.DeclaringType.FullName}\"!");
                        _FILogger.Warning("This ILManipulator is going to be skipped, things might break!");
                        ilManipulatorMethodInfo = null;
                    }

                    if (prefixMethodInfo == null && postfixMethodInfo == null && transpilerMethodInfo == null)
                    {
                        throw new ArchivePatchNoPatchMethodException($"Patch class \"{patchType.FullName}\" doesn't contain a Prefix, Postfix or Transpiler method, at least one is required!");
                    }

                    _patchInfos.Add(new FeaturePatchInfo(original,
                        prefixMethodInfo,
                        postfixMethodInfo,
                        transpilerMethodInfo,
                        finalizerMethodInfo,
                        ilManipulatorMethodInfo,
                        archivePatchInfo));

                    try
                    {
                        var initMethod = patchTypeMethods
                            .FirstOrDefault(mi => mi.IsStatic
                                && (mi.Name == "Init" || mi.GetCustomAttribute<IsInitMethod>() != null)
                                && AnyRundownConstraintMatches(mi)
                                && AnyBuildConstraintMatches(mi));

                        var initMethodParameters = initMethod?.GetParameters();
                        if (initMethod != null)
                        {
                            _FILogger.Debug($"Invoking static Init method {patchType.Name}.{initMethod.Name} on {_feature.Identifier}");
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
                    catch (Exception ex)
                    {
                        _FILogger.Error($"Static Init method on {_feature.Identifier} failed! - {ex}: {ex.Message}");
                        _FILogger.Exception(ex);
                        InternalyDisableFeature(InternalDisabledReason.PatchInitMethodFailed);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _FILogger.Error($"Patch discovery for \"{archivePatchInfo.Type?.FullName ?? $"TYPE NOT FOUND - PatchType:{patchType.FullName}"}\" failed: {ex}: {ex.Message}");
                    _FILogger.Exception(ex);
                }
            }

            LoadFeatureSettings();

            try
            {
                if (!_feature.LateShouldInit())
                {
                    InternalyDisableFeature(InternalDisabledReason.DisabledViaLateShouldInit);
                    return;
                }
            }
            catch (Exception ex)
            {
                _FILogger.Error($"{nameof(Feature.LateShouldInit)} method on {nameof(Feature)} failed: {ex}: {ex.Message}");
                _FILogger.Exception(ex);
                InternalyDisableFeature(InternalDisabledReason.LateShouldInitFailed);
                return;
            }

            try
            {
                _feature.Init();
            }
            catch (Exception ex)
            {
                _FILogger.Error($"Main Feature Init method on {_feature.Identifier} failed! - {ex}: {ex.Message}");
                _FILogger.Exception(ex);
                InternalyDisableFeature(InternalDisabledReason.MainInitMethodFailed);
                return;
            }

            if (FeatureManager.IsEnabledInConfig(_feature))
            {
                Enable(!_feature.SkipInitialOnEnable);
            }
            else
            {
                _feature.Enabled = false;
            }
        }

        private void AfterInit()
        {
            InitialEnabledState = _feature.Enabled;
            if (_feature.BelongsToGroup)
            {
                FeatureManager.AddGroupedFeature(_feature);
            }
        }

        internal void LoadFeatureSettings(bool refreshDisplayName = false)
        {
            if (InternalDisabled) return;

            foreach (var settingsHelper in _settingsHelpers)
            {
                _FILogger.Info($"Loading config {_feature.Identifier} [{settingsHelper.PropertyName}] ({settingsHelper.TypeName}) ...");

                var configInstance = LocalFiles.LoadFeatureConfig(ArchiveModule.GetType().Assembly.GetName().Name, $"{_feature.Identifier}_{settingsHelper.PropertyName}", settingsHelper.SettingType);
                if (refreshDisplayName)
                    settingsHelper.RefreshDisplayName();
                settingsHelper.SetupViaFeatureInstance(configInstance);

                if (settingsHelper.Settings.Any(fs => !fs.HideInModSettings))
                    AllAdditionalSettingsAreHidden = false;
            }
        }

        internal void SaveFeatureSettings()
        {
            if (InternalDisabled) return;

            foreach (var settingsHelper in _settingsHelpers)
            {
                if (!settingsHelper.IsDirty)
                {
                    _FILogger.Info($"Config {_feature.Identifier} [{settingsHelper.PropertyName}] ({settingsHelper.TypeName}) does not need saving!");
                    continue;
                }

                _FILogger.Success($"Saving config {_feature.Identifier} [{settingsHelper.PropertyName}] ({settingsHelper.TypeName}) ...");

                var configInstance = settingsHelper.GetFeatureInstance();

                LocalFiles.SaveFeatureConfig(ArchiveModule.GetType().Assembly.GetName().Name, $"{_feature.Identifier}_{settingsHelper.PropertyName}", settingsHelper.SettingType, configInstance);

                settingsHelper.IsDirty = false;
            }
        }

        internal void SaveAndReloadFeatureSettings()
        {
            SaveFeatureSettings();
            LoadFeatureSettings(refreshDisplayName: true);
        }

        private void ApplyPatches()
        {
            if (InternalDisabled) return;

            foreach (var patchInfo in _patchInfos)
            {
                try
                {
                    _FILogger.Msg(ConsoleColor.DarkBlue, $"Patching {_feature.Identifier} : {patchInfo.ArchivePatchInfo.Type.FullName}.{patchInfo.ArchivePatchInfo.MethodName}()");
                    _harmonyInstance.Patch(patchInfo.OriginalMethod,
                        prefix: patchInfo.HarmonyPrefixMethod,
                        postfix: patchInfo.HarmonyPostfixMethod,
                        transpiler: patchInfo.HarmonyTranspilerMethod,
                        finalizer: patchInfo.HarmonyFinalizerMethod,
                        ilmanipulator: patchInfo.HarmonyILManipulatorMethod);
                }
                catch (Exception ex)
                {
                    _FILogger.Error($"Patch {patchInfo.ArchivePatchInfo.Type.FullName}.{patchInfo.ArchivePatchInfo.MethodName}() failed! {ex}: {ex.Message}");
                    _FILogger.Exception(ex);
                }
            }
        }

        internal bool Enable(bool callOnEnable = true)
        {
            if (InternalDisabled) return false;

            if (_feature.Enabled) return false;

            ApplyPatches();

            _feature.Enabled = true;
            _isEnabledPropertyInfo?.SetValue(null, true);
            if (callOnEnable)
            {
                try
                {
                    _feature.OnEnable();
                }
                catch (Exception ex)
                {
                    _FILogger.Error($"Exception thrown during {nameof(Feature.OnEnable)} in Feature {_feature.Identifier}!");
                    _FILogger.Exception(ex);
                }
            }
            FeatureManager.Instance.CheckSpecialFeatures();
            return true;
        }

        internal bool Disable()
        {
            if (InternalDisabled) return false;

            if (!_feature.Enabled) return false;
            _harmonyInstance.UnpatchSelf();
            _feature.Enabled = false;
            _isEnabledPropertyInfo?.SetValue(null, false);
            try
            {
                _feature.OnDisable();
            }
            catch (Exception ex)
            {
                _FILogger.Error($"Exception thrown during {nameof(Feature.OnDisable)} in Feature {_feature.Identifier}!");
                _FILogger.Exception(ex);
            }
            FeatureManager.Instance.CheckSpecialFeatures();
            return true;
        }

        internal void GameDataInitialized()
        {
            if (InternalDisabled) return;

            if (!_feature.Enabled) return;

            try
            {
                _feature.OnGameDataInitialized();
            }
            catch (Exception ex)
            {
                _FILogger.Error($"Exception thrown during {nameof(Feature.OnGameDataInitialized)} in Feature {_feature.Identifier}!");
                _FILogger.Exception(ex);
            }
        }

        internal void DatablocksReady()
        {
            if (InternalDisabled) return;

            if (!_feature.Enabled) return;

            try
            {
                _feature.OnDatablocksReady();
            }
            catch (Exception ex)
            {
                _FILogger.Error($"Exception thrown during {nameof(Feature.OnDatablocksReady)} in Feature {_feature.Identifier}!");
                _FILogger.Exception(ex);
            }
        }

        internal void FeatureSettingChanged(FeatureSetting setting)
        {
            if (!_feature.Enabled) return;

            if (setting.RequiresRestart)
                _feature.RequestRestart();

            try
            {
                _feature.OnFeatureSettingChanged(setting);
            }
            catch (Exception ex)
            {
                _FILogger.Error($"Exception thrown during {nameof(Feature.OnFeatureSettingChanged)} in Feature {_feature.Identifier}!");
                _FILogger.Exception(ex);
            }
        }

        internal bool MarkSettingsDirty(object settings)
        {
            foreach (var helper in Settings)
            {
                if (helper.Instance == settings)
                {
                    helper.IsDirty = true;
                    return true;
                }
            }
            return false;
        }

        internal void GameStateChanged(int state)
        {
            if (InternalDisabled) return;

            if (!_feature.Enabled) return;

            try
            {
                object gameState = state;
                if (_onGameStateChangedMethodUsesGameEnum)
                {
                    gameState = Enum.ToObject(_gameStateType, state);
                }
                _onGameStateChangedMethodInfo?.Invoke(_feature, new object[] { gameState });
            }
            catch (Exception ex)
            {
                _FILogger.Error($"Exception thrown during {nameof(Feature.OnGameStateChanged)} in Feature {_feature.Identifier}!");
                _FILogger.Exception(ex);
            }
        }

        internal void ApplicationFocusChanged(bool focus)
        {
            if (InternalDisabled) return;

            if (!_feature.Enabled) return;

            try
            {
                _feature.OnApplicationFocusChanged(focus);
            }
            catch (Exception ex)
            {
                _FILogger.Error($"Exception thrown during {nameof(Feature.OnApplicationFocusChanged)} in Feature {_feature.Identifier}!");
                _FILogger.Exception(ex);
            }
        }

        internal void OnLGAreaCullUpdate(object lg_area, bool active)
        {
            if (InternalDisabled) return;

            if (!_feature.Enabled) return;

            try
            {
                _onLGAreaCullUpdateMethodInfo?.Invoke(_feature, new object[] { lg_area, active });
            }
            catch (Exception ex)
            {
                _FILogger.Error($"Exception thrown during {nameof(Feature.OnAreaCull)} in Feature {_feature.Identifier}!");
                _FILogger.Exception(ex);
            }
        }

        internal void OnButtonPressed(ButtonSetting setting)
        {
            if (InternalDisabled) return;

            if (!_feature.Enabled) return;

            try
            {
                _feature.OnButtonPressed(setting);
            }
            catch (Exception ex)
            {
                _FILogger.Error($"Exception thrown during {nameof(Feature.OnButtonPressed)} in Feature {_feature.Identifier}!");
                _FILogger.Exception(ex);
            }
        }

        internal void Quit()
        {
            try
            {
                _feature.OnQuit();
            }
            catch (Exception ex)
            {
                _FILogger.Error($"Exception thrown during {nameof(Feature.OnQuit)} in Feature {_feature.Identifier}!");
                _FILogger.Exception(ex);
            }
        }

        internal static void ReloadAllFeatureSettings()
        {
            foreach (var feature in FeatureManager.Instance.RegisteredFeatures)
            {
                feature.FeatureInternal.SaveAndReloadFeatureSettings();
            }
        }

        internal static Dictionary<string, PropertyInfo> GetFSProperties(Type type)
        {
            return type.GetProperties()
                    .Where(prop => prop.GetCustomAttribute<FSIgnore>() == null
                    && (prop.GetCustomAttributes<Localized>(true).Any()
                    || (typeof(Feature).IsAssignableFrom(prop.DeclaringType) && prop.DeclaringType.FullName != typeof(Feature).FullName && (prop.Name == nameof(Feature.Name) || prop.Name == nameof(Feature.Description)))
                    || prop.PropertyType == typeof(FLabel) || prop.PropertyType == typeof(FButton)))
                    .ToDictionary(
                        prop => $"{prop.DeclaringType.FullName}.{prop.Name}",
                        prop => prop
                    );
        }

        internal static FeatureLocalizationData GenerateFeatureLocalization(Feature feature, FeatureLocalizationData defaultData = null)
        {
            var parentType = feature.GetType();

            var allproperties = new List<Dictionary<string, PropertyInfo>>();
            var enumTypes = new HashSet<Type>();

            var FSTexts = new Dictionary<string, Dictionary<FSType, Dictionary<Language, string>>>();
            var FSEnumTexts = new Dictionary<string, Dictionary<Language, Dictionary<string, string>>>();

            foreach (var type in GetNestedClasses(parentType))
            {
                foreach (var nestedType in type.GetNestedTypes())
                {
                    if (!nestedType.GetCustomAttributes<Localized>(true).Any())
                        continue;
                    if (nestedType.IsEnum)
                        enumTypes.Add(nestedType);
                }
                allproperties.Add(GetFSProperties(type));
            }

            foreach (var props in allproperties)
            {
                foreach (var propPair in props)
                {
                    Dictionary<FSType, Dictionary<Language, string>> fsdic = new();
                    var propType = propPair.Value.PropertyType;
                    var prop = propPair.Value;
                    foreach (FSType fstype in Enum.GetValues(typeof(FSType)))
                    {
                        if (typeof(Feature).IsAssignableFrom(prop.DeclaringType))
                        {
                            if (prop.Name == nameof(Feature.Name))
                                if (fstype != FSType.FName)
                                    continue;
                            if (prop.Name == nameof(Feature.Description))
                                if (fstype != FSType.FDescription)
                                    continue;
                        }
                        switch (fstype)
                        {
                            case FSType.FSDisplayName:
                                if (prop.GetCustomAttribute<FSDisplayName>() == null)
                                    continue;
                                if (propType == typeof(FLabel))
                                    continue;
                                break;
                            case FSType.FSDescription:
                                if (prop.GetCustomAttribute<FSDescription>() == null)
                                    continue;
                                if (propType == typeof(FLabel))
                                    continue;
                                break;
                            case FSType.FSButtonText:
                                if (propType != typeof(FButton))
                                    continue;
                                break;
                            case FSType.FSLabelText:
                                if (propType != typeof(FLabel))
                                    continue;
                                break;
                            case FSType.FSHeader:
                                if (prop.GetCustomAttribute<FSHeader>() == null)
                                    continue;
                                break;
                            case FSType.FName:
                                if (prop.Name != nameof(Feature.Name) || !typeof(Feature).IsAssignableFrom(prop.DeclaringType))
                                    continue;
                                break;
                            case FSType.FDescription:
                                if (prop.Name != nameof(Feature.Description) || !typeof(Feature).IsAssignableFrom(prop.DeclaringType))
                                    continue;
                                break;
                            default:
                                continue;
                        }

                        var languages = new Dictionary<Language, string>();

                        foreach (Language language in Enum.GetValues(typeof(Language)))
                        {
                            if (defaultData == null || defaultData.Internal.FeatureSettingsTexts == null || !defaultData.Internal.FeatureSettingsTexts.TryGetValue(propPair.Key, out var dfsdic) || !dfsdic.TryGetValue(fstype, out var dlandic) || !dlandic.TryGetValue(language, out var defaultText))
                            {
                                defaultText = null;
                            }
                            languages[language] = defaultText;
                        }

                        fsdic[fstype] = languages;
                    }
                    FSTexts[propPair.Key] = fsdic;
                }
            }

            foreach (var type in enumTypes)
            {
                var names = Enum.GetNames(type);
                var enumdic = new Dictionary<Language, Dictionary<string, string>>();
                foreach (Language language in Enum.GetValues(typeof(Language)))
                {
                    var languagedic = new Dictionary<string, string>();
                    foreach (var name in names)
                    {
                        if (defaultData == null || defaultData.Internal.FeatureSettingsEnumTexts == null || !defaultData.Internal.FeatureSettingsEnumTexts.TryGetValue(type.FullName, out var dlandic) || !dlandic.TryGetValue(language, out var pair) || !pair.TryGetValue(name, out var defaultText))
                        {
                            defaultText = null;
                        }
                        languagedic[name] = defaultText;
                    }
                    enumdic[language] = languagedic;
                }
                FSEnumTexts[type.FullName] = enumdic;
            }

            FeatureInternalLocalizationData internalData = new()
            {
                FeatureSettingsTexts = FSTexts,
                FeatureSettingsEnumTexts = FSEnumTexts,
                ExtraTexts = defaultData?.Internal?.ExtraTexts ?? new()
            };

            var externalAllproperties = new List<Dictionary<string, PropertyInfo>>();
            var externalEnumTypes = new HashSet<Type>();
            var externalFSTexts = new Dictionary<string, Dictionary<FSType, Dictionary<Language, string>>>();
            var externalFSEnumTexts = new Dictionary<string, Dictionary<Language, Dictionary<string, string>>>();

            foreach (var externalType in feature.LocalizationExternalTypes)
            {
                if (externalType.IsClass)
                {
                    foreach (var type in GetNestedClasses(externalType))
                    {
                        foreach (var nestedType in type.GetNestedTypes())
                        {
                            if (!nestedType.GetCustomAttributes<Localized>(true).Any())
                                continue;
                            if (nestedType.IsEnum)
                                externalEnumTypes.Add(nestedType);
                        }
                        externalAllproperties.Add(GetFSProperties(type));
                    }

                    foreach (var props in externalAllproperties)
                    {
                        foreach (var propPair in props)
                        {
                            Dictionary<FSType, Dictionary<Language, string>> fsdic = new();
                            var propType = propPair.Value.PropertyType;
                            var prop = propPair.Value;
                            foreach (FSType fstype in Enum.GetValues(typeof(FSType)))
                            {
                                if (typeof(Feature).IsAssignableFrom(prop.DeclaringType))
                                {
                                    if (prop.Name == nameof(Feature.Name))
                                        if (fstype != FSType.FName)
                                            continue;
                                    if (prop.Name == nameof(Feature.Description))
                                        if (fstype != FSType.FDescription)
                                            continue;
                                }
                                switch (fstype)
                                {
                                    case FSType.FSDisplayName:
                                        if (prop.GetCustomAttribute<FSDisplayName>() == null)
                                            continue;
                                        if (propType == typeof(FLabel))
                                            continue;
                                        break;
                                    case FSType.FSDescription:
                                        if (prop.GetCustomAttribute<FSDescription>() == null)
                                            continue;
                                        if (propType == typeof(FLabel))
                                            continue;
                                        break;
                                    case FSType.FSButtonText:
                                        if (propType != typeof(FButton))
                                            continue;
                                        break;
                                    case FSType.FSLabelText:
                                        if (propType != typeof(FLabel))
                                            continue;
                                        break;
                                    case FSType.FSHeader:
                                        if (prop.GetCustomAttribute<FSHeader>() == null)
                                            continue;
                                        break;
                                    case FSType.FName:
                                        if (prop.Name != nameof(Feature.Name) || !typeof(Feature).IsAssignableFrom(prop.DeclaringType))
                                            continue;
                                        break;
                                    case FSType.FDescription:
                                        if (prop.Name != nameof(Feature.Description) || !typeof(Feature).IsAssignableFrom(prop.DeclaringType))
                                            continue;
                                        break;
                                    default:
                                        continue;
                                }

                                var languages = new Dictionary<Language, string>();

                                foreach (Language language in Enum.GetValues(typeof(Language)))
                                {
                                    if (defaultData == null || defaultData.External.ExternalFeatureSettingsTexts == null || !defaultData.External.ExternalFeatureSettingsTexts.TryGetValue(propPair.Key, out var dfsdic) || !dfsdic.TryGetValue(fstype, out var dlandic) || !dlandic.TryGetValue(language, out var defaultText))
                                    {
                                        defaultText = null;
                                    }
                                    languages[language] = defaultText;
                                }

                                fsdic[fstype] = languages;
                            }
                            externalFSTexts[propPair.Key] = fsdic;
                        }
                    }
                }
                else if (externalType.IsEnum)
                {
                    externalEnumTypes.Add(externalType);
                }

                foreach (var type in externalEnumTypes)
                {
                    var names = Enum.GetNames(type);
                    var enumdic = new Dictionary<Language, Dictionary<string, string>>();
                    foreach (Language language in Enum.GetValues(typeof(Language)))
                    {
                        var languagedic = new Dictionary<string, string>();
                        foreach (var name in names)
                        {
                            if (defaultData == null || defaultData.External.ExternalEnumTexts == null || !defaultData.External.ExternalEnumTexts.TryGetValue(type.FullName, out var dlandic) || !dlandic.TryGetValue(language, out var pair) || !pair.TryGetValue(name, out var defaultText))
                            {
                                defaultText = null;
                            }
                            languagedic[name] = defaultText;
                        }
                        enumdic[language] = languagedic;
                    }
                    externalFSEnumTexts[type.FullName] = enumdic;
                }
            }

            FeatureExternalLocalizationData ExternalData = new()
            {
                ExternalFeatureSettingsTexts = externalFSTexts,
                ExternalEnumTexts = externalFSEnumTexts
            };

            return new() { Internal = internalData, External = ExternalData };
        }

        private class FeaturePatchInfo
        {
            internal ArchivePatch ArchivePatchInfo { get; private set; }
            internal MethodBase OriginalMethod { get; private set; }
            internal HarmonyLib.HarmonyMethod HarmonyPrefixMethod { get; private set; }
            internal HarmonyLib.HarmonyMethod HarmonyPostfixMethod { get; private set; }
            internal HarmonyLib.HarmonyMethod HarmonyTranspilerMethod { get; private set; }
            internal HarmonyLib.HarmonyMethod HarmonyFinalizerMethod { get; private set; }
            internal HarmonyLib.HarmonyMethod HarmonyILManipulatorMethod { get; private set; }
            internal MethodInfo PrefixPatchMethod { get; private set; }
            internal MethodInfo PostfixPatchMethod { get; private set; }
            internal MethodInfo TranspilerPatchMethod { get; private set; }
            internal MethodInfo FinalizerPatchMethod { get; private set; }
            internal MethodInfo ILManipulatorPatchMethod { get; private set; }

            public FeaturePatchInfo(MethodBase original, MethodInfo prefix, MethodInfo postfix, MethodInfo transpiler, MethodInfo finalizer, MethodInfo ilManipulator, ArchivePatch archivePatch, bool wrapTryCatch = true)
            {
                OriginalMethod = original;

                PrefixPatchMethod = prefix;
                PostfixPatchMethod = postfix;
                TranspilerPatchMethod = transpiler;
                FinalizerPatchMethod = finalizer;
                ILManipulatorPatchMethod = ilManipulator;

                ArchivePatchInfo = archivePatch;

                if (prefix != null)
                    HarmonyPrefixMethod = new HarmonyLib.HarmonyMethod(prefix)
                    {
                        wrapTryCatch = wrapTryCatch
                    };
                if (postfix != null)
                    HarmonyPostfixMethod = new HarmonyLib.HarmonyMethod(postfix)
                    {
                        wrapTryCatch = wrapTryCatch
                    };
                if (transpiler != null)
                    HarmonyTranspilerMethod = new HarmonyLib.HarmonyMethod(transpiler)
                    {
                        wrapTryCatch = wrapTryCatch
                    };
                if (finalizer != null)
                    HarmonyFinalizerMethod = new HarmonyLib.HarmonyMethod(finalizer)
                    {
                        wrapTryCatch = wrapTryCatch
                    };
                if (ilManipulator != null)
                    HarmonyILManipulatorMethod = new HarmonyLib.HarmonyMethod(ilManipulator)
                    {
                        wrapTryCatch = wrapTryCatch
                    };
            }
        }

        internal bool Store<T>(string key, T obj)
        {
            if (_storage.TryGetValue(key, out var _))
            {
                return false;
            }
            _storage.Add(key, obj);
            return true;
        }

        internal bool Retrieve<T>(string key, out T value)
        {
            if (_storage.TryGetValue(key, out var val))
            {
                value = (T)val;
                return true;
            }
            value = default;
            return false;
        }

        internal void RequestDisable(string reason)
        {
            _FILogger.Info($"Feature {_feature.Identifier} has requested to be disabled: {reason}");
            InternalyDisableFeature(InternalDisabledReason.DisabledByRequest);
        }

        private void InternalyDisableFeature(InternalDisabledReason reason)
        {
            InternalDisabled = true;
            DisabledReason |= reason;
            FeatureManager.Instance.DisableFeature(_feature, setConfig: false);
        }

        [Flags]
        internal enum InternalDisabledReason
        {
            RundownConstraintMismatch,
            BuildConstraintMismatch,
            MainInitMethodFailed,
            PatchInitMethodFailed,
            UpdateMethodFailed,
            LateUpdateMethodFailed,
            ForceDisabled,
            DisabledViaShouldInit,
            DisabledByRequest,
            TypeLoadException,
            Other,
            ShouldInitFailed,
            LateShouldInitFailed,
            DisabledViaLateShouldInit,
        }
    }
}