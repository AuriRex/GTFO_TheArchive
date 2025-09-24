using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.Exceptions;
using TheArchive.Core.FeaturesAPI.Components;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Localization;
using TheArchive.Core.Localization.Data;
using TheArchive.Core.Localization.Services;
using TheArchive.Core.Managers;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.FeaturesAPI;

internal class FeatureInternal
{
    internal string ModuleGroupId { get; set; }
    internal ModuleGroup ModuleGroup { get; private set; }
    internal FeatureLocalizationService Localization { get; private set; }
    private static GameBuildInfo BuildInfo => Feature.BuildInfo;
    internal bool InternalDisabled { get; private set; }
    private InternalDisabledReason DisabledReason { get; set; }
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
    internal bool InitialEnabledState { get; private set; }
    internal IEnumerable<FeatureSettingsHelper> Settings => _settingsHelpers;
    internal RundownFlags Rundowns { get; private set; } = RundownFlags.None;
    internal IArchiveLogger FeatureLoggerInstance { get; private set; }
    internal Assembly OriginAssembly { get; private set; }
    internal IArchiveModule ArchiveModule { get; private set; }
    internal string DisplayName
    {
        get
        {
            if (!_doLocalizeName)
                return _feature.Name;
            
            var propID = $"{_featureType.FullName}.{nameof(Feature.Name)}";
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
            if (!_doLocalizeDescription)
                return _feature.Description;
            
            var propID = $"{_featureType.FullName}.{nameof(Feature.Description)}";
            if (Localization.TryGetFSText(propID, FSType.FDescription, out var text))
            {
                return text;
            }
            
            return _feature.Description;
        }
    }

    private string _asmDisplayName;
    internal string AsmDisplayName
    {
        get
        {
            if (string.IsNullOrEmpty(_asmDisplayName))
            {
                _asmDisplayName = OriginAssembly.GetCustomAttribute<ModSettingsDisplayName>()?.DisplayName ?? OriginAssembly.GetName().Name;
            }
            return _asmDisplayName;
        }
    }
    internal string CriticalInfo
    {
        get
        {
            if (!InternalDisabled)
            {
                return string.Empty;
            }

            return $"<#F00>{ArchiveLocalizationService.GetById(2, "Disabled")}</color>: {ArchiveLocalizationService.Get(DisabledReason)}";
        }
    }

    private Feature _feature;
    private HarmonyLib.Harmony _harmonyInstance;

    private readonly List<Type> _patchTypes = new();
    private readonly HashSet<FeaturePatchInfo> _patchInfos = new();
    private readonly HashSet<FeatureSettingsHelper> _settingsHelpers = new();
    private PropertyInfo _isEnabledPropertyInfo;
    private bool _onGameStateChangedMethodUsesGameEnum;
    private MethodInfo _onGameStateChangedMethodInfo;
    private MethodInfo _onLGAreaCullUpdateMethodInfo;
    
    private Type _featureType;
    private PropertyInfo _namePropertyInfo;
    private PropertyInfo _descriptionPropertyInfo;
    private bool _doLocalizeName;
    private bool _doLocalizeDescription;
    
    private static readonly HashSet<string> _usedIdentifiers = new();
    private static readonly IArchiveLogger _FILogger = LoaderWrapper.CreateArSubLoggerInstance(nameof(FeatureInternal), ConsoleColor.DarkYellow);

    private static Type _gameStateType;
    private static Type _lgAreaType;

    private FeatureInternal() { }

    internal static void CreateAndAssign(Feature feature, IArchiveModule module)
    {
        _gameStateType ??= ImplementationManager.GameTypeByIdentifier("eGameStateName");
        _lgAreaType ??= ImplementationManager.GameTypeByIdentifier("LG_Area");
        
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
            fi.InternallyDisableFeature(InternalDisabledReason.TypeLoadException);
            _FILogger.Msg(ConsoleColor.Magenta, $"Feature \"{fi._feature.Identifier}\" has been disabled internally! ({fi.DisabledReason})");
            return;
        }
        fi.AfterInit();
    }

    public delegate void Update();
    public delegate void LateUpdate();

    private void Init(Feature feature, IArchiveModule module)
    {
        _feature = feature;

        ArchiveModule = module;

        _featureType = _feature.GetType();
        OriginAssembly = _featureType.Assembly;

        var asmName = ArchiveModule.GetType().Assembly.GetName().Name;
        ModuleGroupId = $"{asmName}.ModuleGroup";
        ModuleGroup = GroupManager.GetModuleGroup(asmName);
        Localization = new(feature, feature.FeatureLogger);

        try
        {
            Localization.Setup();
        }
        catch (Exception ex)
        {
            _FILogger.Error($"Error while trying to setup feature localization for \"{_feature.Identifier}\"!");
            _FILogger.Exception(ex);
        }

        _FILogger.Msg(ConsoleColor.Black, "-");
        _FILogger.Msg(ConsoleColor.Green, $"Initializing {_feature.Identifier} ...");

        if (!_usedIdentifiers.Add(_feature.Identifier))
        {
            throw new ArchiveFeatureDuplicateIDException($"Provided feature id \"{_feature.Identifier}\" has already been registered by {FeatureManager.GetById(_feature.Identifier)}!");
        }

        FeatureLoggerInstance = LoaderWrapper.CreateArSubLoggerInstance($"F::{_feature.Identifier}", ConsoleColor.Cyan);

        HideInModSettings = _featureType.GetCustomAttribute<HideInModSettings>() != null;
        DoNotSaveToConfig = _featureType.GetCustomAttribute<DoNotSaveToConfig>() != null;
        AutomatedFeature = _featureType.GetCustomAttribute<AutomatedFeature>() != null;
        DisableModSettingsButton = _featureType.GetCustomAttribute<DisallowInGameToggle>() != null;

        if (_featureType.GetCustomAttribute<ForceDisable>() != null)
        {
            InternalDisabled = true;
            DisabledReason |= InternalDisabledReason.ForceDisabled;
        }

        foreach (var constraint in _featureType.GetCustomAttributes<RundownConstraint>())
        {
            Rundowns |= constraint.Rundowns;
        }

        if (!AnyRundownConstraintMatches(_featureType))
        {
            InternalDisabled = true;
            DisabledReason |= InternalDisabledReason.RundownConstraintMismatch;
        }

        if (!AnyBuildConstraintMatches(_featureType))
        {
            InternalDisabled = true;
            DisabledReason |= InternalDisabledReason.BuildConstraintMismatch;
        }

        _namePropertyInfo = _featureType.GetProperty(nameof(Feature.Name));
        _doLocalizeName = _namePropertyInfo?.GetCustomAttribute<IgnoreLocalization>() == null;
        
        _descriptionPropertyInfo = _featureType.GetProperty(nameof(Feature.Description));
        _doLocalizeDescription = _descriptionPropertyInfo?.GetCustomAttribute<IgnoreLocalization>() == null;
        
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

        var featureMethods = _featureType.GetMethods();

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

        var featureProperties = _featureType.GetProperties(AccessTools.allDeclared);

        var settingsProps = featureProperties
            .Where(pi => pi.GetCustomAttribute<FeatureConfig>() != null);

        _isEnabledPropertyInfo = featureProperties
            .FirstOrDefault(pi => (pi.Name == "IsEnabled" || pi.GetCustomAttribute<SetEnabledStatus>() != null)
                                  && pi.SetMethod != null
                                  && pi.GetMethod != null
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

        var staticLocalizationServicePropertyInfo = featureProperties
            .FirstOrDefault(pi => (pi.Name == "Localization" || pi.GetCustomAttribute<SetStaticLocalizationService>() != null)
                                  && pi.SetMethod != null
                                  && pi.GetMethod != null
                                  && pi.GetMethod.IsStatic
                                  && pi.GetMethod.ReturnType == typeof(ILocalizationService));

        if (staticLocalizationServicePropertyInfo != null)
        {
            _FILogger.Debug($"Found Localization property \"{staticLocalizationServicePropertyInfo.Name}\" on Feature {_feature.Identifier}. Populating ...");
            staticLocalizationServicePropertyInfo.SetValue(null, _feature.Localization);
        }
        
        var staticLInstancePropertyInfo = featureProperties
            .FirstOrDefault(pi => (pi.Name == "Instance" || pi.Name == "Self" || pi.GetCustomAttribute<SetStaticInstance>() != null)
                                  && pi.SetMethod != null
                                  && pi.GetMethod != null
                                  && pi.GetMethod.IsStatic
                                  && pi.GetMethod.ReturnType.IsAssignableTo(_featureType));

        if (staticLInstancePropertyInfo != null)
        {
            _FILogger.Debug($"Found Instance/Self property \"{staticLInstancePropertyInfo.Name}\" on Feature {_feature.Identifier}. Populating ...");
            staticLInstancePropertyInfo.SetValue(null, _feature);
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

#pragma warning disable CS0618 // Type or member is obsolete
        _onLGAreaCullUpdateMethodInfo = featureMethods
            .FirstOrDefault(mi => (mi.Name == "OnAreaCull" || mi.GetCustomAttribute<IsAreaCullUpdateMethod>() != null)
                                  && !mi.IsStatic
                                  && mi.DeclaringType != typeof(Feature)
                                  && mi.GetParameters().Length == 2
                                  && (mi.GetParameters()[0].ParameterType == _lgAreaType || mi.GetParameters()[0].ParameterType == typeof(object))
                                  && mi.GetParameters()[1].ParameterType == typeof(bool));
#pragma warning restore CS0618 // Type or member is obsolete
        
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

        var potentialPatchTypes = _featureType.GetNestedTypes(AnyBindingFlagss).Where(nt => nt.GetCustomAttribute<ArchivePatch>() != null);

        foreach (var type in potentialPatchTypes)
        {
            if (AnyRundownConstraintMatches(type) && AnyBuildConstraintMatches(type))
            {
                _patchTypes.Add(type);
                continue;
            }

            _FILogger.Debug($"{_feature.Identifier}: ignoring {type.FullName} (Rundown | Build not matching.)");
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

                var patchTypeMethods = patchType.GetMethods(AnyBindingFlagss);

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

                var priorityMethod = patchTypeMethods
                    .FirstOrDefault(mi => mi.ReturnType == typeof(int)
                                          && (mi.Name == "Priority" || mi.GetCustomAttribute<IsPriorityProvider>() != null)
                                          && AnyRundownConstraintMatches(mi)
                                          && AnyBuildConstraintMatches(mi));

                if (priorityMethod != null)
                {
                    if (!priorityMethod.IsStatic)
                        throw new ArchivePatchMethodNotStaticException($"Method \"{priorityMethod.Name}\" in Feature \"{feature.Identifier}\" must be static!");

                    archivePatchInfo.Priority = (int)priorityMethod.Invoke(null, null);
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
                            original = archivePatchInfo.Type.GetMethod(archivePatchInfo.MethodName, AnyBindingFlagss, null, archivePatchInfo.ParameterTypes, null);
                            break;
                        }

                        original = archivePatchInfo.Type.GetMethod(archivePatchInfo.MethodName, AnyBindingFlagss);
                        break;
                    case ArchivePatch.PatchMethodType.Getter:
                        original = archivePatchInfo.Type.GetProperty(archivePatchInfo.MethodName, AnyBindingFlagss)?.GetMethod;
                        break;
                    case ArchivePatch.PatchMethodType.Setter:
                        original = archivePatchInfo.Type.GetProperty(archivePatchInfo.MethodName, AnyBindingFlagss)?.SetMethod;
                        break;
                    case ArchivePatch.PatchMethodType.Constructor:
                        original = archivePatchInfo.Type.GetConstructor(AnyBindingFlagss, null, archivePatchInfo.ParameterTypes, null);
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

                if (prefixMethodInfo == null && postfixMethodInfo == null && finalizerMethodInfo == null && transpilerMethodInfo == null && ilManipulatorMethodInfo == null)
                {
                    throw new ArchivePatchNoPatchMethodException($"Patch class \"{patchType.FullName}\" doesn't contain a Prefix, Postfix, Finalizer, Transpiler or ILManipulator method, at least one is required!");
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
                    InternallyDisableFeature(InternalDisabledReason.PatchInitMethodFailed);
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
                InternallyDisableFeature(InternalDisabledReason.DisabledViaLateShouldInit);
                return;
            }
        }
        catch (Exception ex)
        {
            _FILogger.Error($"{nameof(Feature.LateShouldInit)} method on {nameof(Feature)} failed: {ex}: {ex.Message}");
            _FILogger.Exception(ex);
            InternallyDisableFeature(InternalDisabledReason.LateShouldInitFailed);
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
            InternallyDisableFeature(InternalDisabledReason.MainInitMethodFailed);
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

    private void LoadFeatureSettings(bool refreshDisplayName = false)
    {
        if (InternalDisabled) return;

        foreach (var settingsHelper in _settingsHelpers)
        {
            try
            {
                _FILogger.Info($"Loading config {_feature.Identifier} [{settingsHelper.PropertyName}] ({settingsHelper.TypeName}) ...");

                var configInstance = LocalFiles.LoadFeatureConfig(ArchiveModule.GetType().Assembly.GetName().Name,
                    $"{_feature.Identifier}_{settingsHelper.PropertyName}", settingsHelper.SettingType);
                if (refreshDisplayName)
                    settingsHelper.RefreshDisplayName();
                settingsHelper.SetupViaFeatureInstance(configInstance);

                if (settingsHelper.Settings.Any(fs => !fs.HideInModSettings))
                    AllAdditionalSettingsAreHidden = false;
            }
            catch (Exception ex)
            {
                _FILogger.Error($"An error occured while loading config {_feature.Identifier} [{settingsHelper.PropertyName}] ({settingsHelper.TypeName})!");
                _FILogger.Exception(ex);
            }
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

            try
            {
                var configInstance = settingsHelper.GetFeatureInstance();

                LocalFiles.SaveFeatureConfig(ArchiveModule.GetType().Assembly.GetName().Name, $"{_feature.Identifier}_{settingsHelper.PropertyName}", settingsHelper.SettingType, configInstance);
            }
            catch (Exception ex)
            {
                _FILogger.Error($"Failed to save config file {_feature.Identifier} [{settingsHelper.PropertyName}] ({settingsHelper.TypeName})!");
                _FILogger.Exception(ex);
            }
            
            settingsHelper.IsDirty = false;
        }
    }

    private void SaveAndReloadFeatureSettings()
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
            _onGameStateChangedMethodInfo?.Invoke(_feature, new[] { gameState });
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

    internal void OnLGAreaCullUpdate(object lgArea, bool active)
    {
        if (InternalDisabled) return;

        if (!_feature.Enabled) return;

        try
        {
            _onLGAreaCullUpdateMethodInfo?.Invoke(_feature, new[] { lgArea, active });
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

    private static Dictionary<string, PropertyInfo> GetFSProperties(Type type)
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

    private static FeatureSettingLocalizationData GenerateFeatureSettingLocalizationData(
        List<Dictionary<string, PropertyInfo>> allProperties,
        HashSet<Type> enumTypes,
        FeatureSettingLocalizationData existData)
    {
        var result = new FeatureSettingLocalizationData();

        foreach (var props in allProperties)
        {
            foreach (var (propKey, prop) in props)
            {
                var propType = prop.PropertyType;

                //CollectEnumTypesFromProperty(propType, enumTypes);

                if (!result.FeatureSettingTexts.ContainsKey(propKey))
                {
                    result.FeatureSettingTexts[propKey] = new Dictionary<FSType, Dictionary<Language, string>>();
                }

                foreach (FSType fstype in Enum.GetValues<FSType>())
                {
                    if (!ShouldProcessFSType(prop, propType, fstype))
                        continue;

                    var languages = CreateLanguageDictionary(propKey, fstype, existData);
                    result.FeatureSettingTexts[propKey][fstype] = languages;
                }
            }
        }

        ProcessEnumLocalization(result, enumTypes, existData);

        return result;
    }

    private static bool ShouldProcessFSType(PropertyInfo prop, Type propType, FSType fstype)
    {
        var isFeatureProperty = typeof(Feature).IsAssignableFrom(prop.DeclaringType);

        return fstype switch
        {
            FSType.FName => isFeatureProperty && prop.Name == nameof(Feature.Name),
            FSType.FDescription => isFeatureProperty && prop.Name == nameof(Feature.Description),
            FSType.FSDisplayName => !IsLabelType(propType) && prop.GetCustomAttribute<FSDisplayName>() != null,
            FSType.FSDescription => !IsLabelType(propType) && prop.GetCustomAttribute<FSDescription>() != null,
            FSType.FSButtonText => propType == typeof(FButton),
            FSType.FSLabelText => propType == typeof(FLabel),
            FSType.FSHeader => prop.GetCustomAttribute<FSHeader>() != null,
            FSType.FSTooltipHeader => prop.GetCustomAttribute<FSTooltip>() != null,
            FSType.FSTooltipText => prop.GetCustomAttribute<FSTooltip>() != null,
            _ => false
        };

        static bool IsLabelType(Type type) => type == typeof(FLabel);
    }

    private static Dictionary<Language, string> CreateLanguageDictionary(
        string propKey,
        FSType fstype,
        FeatureSettingLocalizationData existData)
    {
        var languages = new Dictionary<Language, string>();

        foreach (Language language in Enum.GetValues<Language>())
        {
            string defaultText = null;

            if (existData?.FeatureSettingTexts?.TryGetValue(propKey, out var fstypeDict) == true &&
                fstypeDict.TryGetValue(fstype, out var languageDict) &&
                languageDict.TryGetValue(language, out var existingText))
            {
                defaultText = existingText;
            }

            languages[language] = defaultText;
        }

        return languages;
    }

    private static void ProcessEnumLocalization(
        FeatureSettingLocalizationData result,
        HashSet<Type> enumTypes,
        FeatureSettingLocalizationData existData)
    {
        foreach (var enumType in enumTypes)
        {
            var enumData = new Dictionary<Language, Dictionary<string, string>>();
            result.EnumTexts[enumType.FullName] = enumData;

            foreach (Language language in Enum.GetValues<Language>())
            {
                var languageDict = new Dictionary<string, string>();
                enumData[language] = languageDict;

                foreach (var enumName in Enum.GetNames(enumType))
                {
                    string defaultText = null;

                    if (existData?.EnumTexts?.TryGetValue(enumType.FullName, out var existingEnumData) == true &&
                        existingEnumData?.TryGetValue(language, out var existingLanguageDict) == true &&
                        existingLanguageDict.TryGetValue(enumName, out var existingText))
                    {
                        defaultText = existingText;
                    }

                    languageDict[enumName] = defaultText;
                }
            }
        }
    }

    internal static FeatureLocalizationData GenerateFeatureLocalization(Feature feature, FeatureLocalizationData existData = null)
    {
        var result = new FeatureLocalizationData()
        {
            GenericTexts = existData?.GenericTexts ?? new()
        };

        var (internalProperties, internalEnumTypes) = CollectInternalTypesData(feature);
        result.Internal = GenerateFeatureSettingLocalizationData(internalProperties, internalEnumTypes, existData?.Internal);

        var (externalProperties, externalEnumTypes) = CollectExternalTypesData(feature);
        result.External = GenerateFeatureSettingLocalizationData(externalProperties, externalEnumTypes, existData?.External);

        return result;
    }

    private static (List<Dictionary<string, PropertyInfo>> properties, HashSet<Type> enumTypes)
        CollectInternalTypesData(Feature feature)
    {
        var allProperties = new List<Dictionary<string, PropertyInfo>>();
        var enumTypes = new HashSet<Type>();

        foreach (var type in GetNestedClasses(feature.GetType()))
        {
            CollectNestedEnumTypes(type, enumTypes);

            allProperties.Add(GetFSProperties(type));
        }

        return (allProperties, enumTypes);
    }

    private static (List<Dictionary<string, PropertyInfo>> properties, HashSet<Type> enumTypes)
        CollectExternalTypesData(Feature feature)
    {
        var externalAllProperties = new List<Dictionary<string, PropertyInfo>>();
        var externalEnumTypes = new HashSet<Type>();

        foreach (var externalType in feature.ExternalLocalizedTypes)
        {
            if (externalType.IsEnum)
            {
                externalEnumTypes.Add(externalType);
            }
            else if (externalType.IsClass)
            {
                foreach (var type in GetNestedClasses(externalType))
                {
                    CollectNestedEnumTypes(type, externalEnumTypes);

                    externalAllProperties.Add(GetFSProperties(type));
                }
            }
        }

        return (externalAllProperties, externalEnumTypes);
    }

    private static void CollectNestedEnumTypes(Type type, HashSet<Type> enumTypes)
    {
        foreach (var nestedType in type.GetNestedTypes())
        {
            if (nestedType.IsEnum)
            {
                if (HasLocalizedAttribute(nestedType))
                    enumTypes.Add(nestedType);
            }
        }
    }

    private static bool HasLocalizedAttribute(Type type)
    {
        return type.GetCustomAttributes<Localized>(true).Any();
    }

    private static void CollectEnumTypesFromProperty(Type propType, HashSet<Type> enumTypes)
    {
        if (propType.IsEnum)
        {
            enumTypes.Add(propType);
        }
        else if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(propType);
            if (underlyingType?.IsEnum == true)
            {
                enumTypes.Add(underlyingType);
            }
        }
        else if (propType.IsGenericType)
        {
            foreach (var genericArg in propType.GetGenericArguments())
            {
                if (genericArg.IsEnum)
                {
                    enumTypes.Add(genericArg);
                }
            }
        }
    }

    private class FeaturePatchInfo
    {
        internal ArchivePatch ArchivePatchInfo { get; }
        internal MethodBase OriginalMethod { get; }
        internal HarmonyLib.HarmonyMethod HarmonyPrefixMethod { get; }
        internal HarmonyLib.HarmonyMethod HarmonyPostfixMethod { get; }
        internal HarmonyLib.HarmonyMethod HarmonyTranspilerMethod { get; }
        internal HarmonyLib.HarmonyMethod HarmonyFinalizerMethod { get; }
        internal HarmonyLib.HarmonyMethod HarmonyILManipulatorMethod { get; }

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        internal MethodInfo PrefixPatchMethod { get; private set; }
        internal MethodInfo PostfixPatchMethod { get; private set; }
        internal MethodInfo TranspilerPatchMethod { get; private set; }
        internal MethodInfo FinalizerPatchMethod { get; private set; }
        internal MethodInfo ILManipulatorPatchMethod { get; private set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

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
                    wrapTryCatch = wrapTryCatch,
                    priority = archivePatch.Priority,
                    after = prefix.GetCustomAttributes<ArchiveAfter>(true)
                            .SelectMany(attr => attr.After).Distinct().ToArray(),
                    before = prefix.GetCustomAttributes<ArchiveBefore>(true)
                            .SelectMany(attr => attr.Before).Distinct().ToArray(),
                };
            if (postfix != null)
                HarmonyPostfixMethod = new HarmonyLib.HarmonyMethod(postfix)
                {
                    wrapTryCatch = wrapTryCatch,
                    priority = archivePatch.Priority,
                    after = postfix.GetCustomAttributes<ArchiveAfter>(true)
                            .SelectMany(attr => attr.After).Distinct().ToArray(),
                    before = postfix.GetCustomAttributes<ArchiveBefore>(true)
                            .SelectMany(attr => attr.Before).Distinct().ToArray(),
                };
            if (transpiler != null)
                HarmonyTranspilerMethod = new HarmonyLib.HarmonyMethod(transpiler)
                {
                    wrapTryCatch = wrapTryCatch,
                    priority = archivePatch.Priority,
                    after = transpiler.GetCustomAttributes<ArchiveAfter>(true)
                            .SelectMany(attr => attr.After).Distinct().ToArray(),
                    before = transpiler.GetCustomAttributes<ArchiveBefore>(true)
                            .SelectMany(attr => attr.Before).Distinct().ToArray(),
                };
            if (finalizer != null)
                HarmonyFinalizerMethod = new HarmonyLib.HarmonyMethod(finalizer)
                {
                    wrapTryCatch = wrapTryCatch,
                    priority = archivePatch.Priority,
                    after = finalizer.GetCustomAttributes<ArchiveAfter>(true)
                            .SelectMany(attr => attr.After).Distinct().ToArray(),
                    before = finalizer.GetCustomAttributes<ArchiveBefore>(true)
                            .SelectMany(attr => attr.Before).Distinct().ToArray(),
                };
            if (ilManipulator != null)
                HarmonyILManipulatorMethod = new HarmonyLib.HarmonyMethod(ilManipulator)
                {
                    wrapTryCatch = wrapTryCatch,
                    priority = archivePatch.Priority,
                    after = ilManipulator.GetCustomAttributes<ArchiveAfter>(true)
                            .SelectMany(attr => attr.After).Distinct().ToArray(),
                    before = ilManipulator.GetCustomAttributes<ArchiveBefore>(true)
                            .SelectMany(attr => attr.Before).Distinct().ToArray(),
                };
        }
    }

    internal void RequestDisable(string reason)
    {
        _FILogger.Info($"Feature {_feature.Identifier} has requested to be disabled: {reason}");
        InternallyDisableFeature(InternalDisabledReason.DisabledByRequest);
    }

    private void InternallyDisableFeature(InternalDisabledReason reason)
    {
        InternalDisabled = true;
        DisabledReason |= reason;
        FeatureManager.Instance.DisableFeature(_feature, setConfig: false);
    }

    // ReSharper disable UnusedMember.Global
    [Flags]
    internal enum InternalDisabledReason : int
    {
        None = 0,
        RundownConstraintMismatch = 1 << 0,
        BuildConstraintMismatch = 1 << 1,
        MainInitMethodFailed = 1 << 2,
        PatchInitMethodFailed = 1 << 3,
        UpdateMethodFailed = 1 << 4,
        LateUpdateMethodFailed = 1 << 5,
        ForceDisabled = 1 << 6,
        DisabledViaShouldInit = 1 << 7,
        DisabledByRequest = 1 << 8,
        TypeLoadException = 1 << 9,
        Other = 1 << 10,
        ShouldInitFailed = 1 << 11,
        LateShouldInitFailed = 1 << 12,
        DisabledViaLateShouldInit = 1 << 13,
    }
    // ReSharper restore UnusedMember.Global
}