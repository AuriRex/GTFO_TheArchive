using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Settings;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;
using static TheArchive.Core.FeaturesAPI.FeatureInternal;

namespace TheArchive.Core.FeaturesAPI
{
    public class FeatureManager : InitSingletonBase<FeatureManager>
    {

        public HashSet<Feature> RegisteredFeatures { get; private set; } = new HashSet<Feature>();
        public HashSet<Feature> FeaturesRequestingRestart { get; private set; } = new HashSet<Feature>();
        public bool AnyFeatureRequestingRestart => FeaturesRequestingRestart.Count > 0;

        public Dictionary<string, HashSet<Feature>> GroupedFeatures { get; private set; } = new Dictionary<string, HashSet<Feature>>();

        public bool InUpdateCycle { get; private set; } = false;
        public bool InLateUpdateCycle { get; private set; } = false;

        private readonly EnabledFeatures _enabledFeatures;

        private readonly HashSet<FeatureInternal.Update> _activeUpdateMethods = new HashSet<FeatureInternal.Update>();
        private readonly HashSet<FeatureInternal.LateUpdate> _activeLateUpdateMethods = new HashSet<FeatureInternal.LateUpdate>();

        private readonly Stack<UpdateModification<FeatureInternal.Update>> _updateModification = new Stack<UpdateModification<FeatureInternal.Update>>();
        private readonly Stack<UpdateModification<FeatureInternal.LateUpdate>> _lateUpdateModification = new Stack<UpdateModification<FeatureInternal.LateUpdate>>();


        private record struct UpdateModification<T>(bool SetEnabled, T Value);
 

        private Feature _unityAudioListenerHelper;
        private Feature UnityAudioListenerHelper
        {
            get
            {
                if(_unityAudioListenerHelper == null)
                {
                    _unityAudioListenerHelper = GetById("InternalUAudioListenerHelper");
                }
                return _unityAudioListenerHelper;
            }
        }

        private readonly IArchiveLogger _logger = LoaderWrapper.CreateArSubLoggerInstance(nameof(FeatureManager), ConsoleColor.DarkYellow);

        public event Action<Feature> OnFeatureEnabled;
        public event Action<Feature> OnFeatureDisabled;
        public event Action<Feature, bool> OnFeatureRestartRequestChanged;

        private FeatureManager()
        {
            Feature.BuildInfo = ArchiveMod.CurrentBuildInfo;
            _enabledFeatures = LocalFiles.LoadConfig<EnabledFeatures>();
            ArchiveMod.GameStateChanged += OnGameStateChanged;
            ArchiveMod.ApplicationFocusStateChanged += OnApplicationFocusChanged;

            Feature.SetupIs();
        }

        public static bool IsFeatureEnabled(string featureId)
        {
            var feature = Instance.RegisteredFeatures.FirstOrDefault(f => f.Identifier == featureId);

            if (feature == null)
                return false;

            return feature.Enabled;
        }

        internal void OnGameDataInitialized()
        {
            _logger.Debug($"{nameof(OnGameDataInitialized)}()");
            Feature.GameDataInited = true;
            try
            {
                foreach (var feature in RegisteredFeatures)
                {
                    feature.FeatureInternal.GameDataInitialized();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception thrown in {nameof(OnGameDataInitialized)}! {ex}: {ex.Message}");
                _logger.Exception(ex);
            }
        }

        internal void OnDatablocksReady()
        {
            _logger.Debug($"{nameof(OnDatablocksReady)}()");
            Feature.DataBlocksReady = true;
            try
            {
                foreach (var feature in RegisteredFeatures)
                {
                    feature.FeatureInternal.DatablocksReady();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception thrown in {nameof(OnDatablocksReady)}! {ex}: {ex.Message}");
                _logger.Exception(ex);
            }
        }

        internal void OnApplicationQuit()
        {
            _logger.Info($"{nameof(OnApplicationQuit)}()");
            try
            {
                Feature.IsApplicationQuitting = true;

                _logger.Info($"Saving settings ... ");

                SaveConfig();

                foreach (var feature in RegisteredFeatures)
                {
                    SaveFeatureConfig(feature);
                }

                //_logger.Info($"Unpatching ... ");

                foreach (var feature in RegisteredFeatures)
                {
                    feature.FeatureInternal.Quit();
                    
                    //DisableFeature(feature);
                }
            }
            catch(Exception ex)
            {
                _logger.Error($"Exception thrown in {nameof(OnApplicationQuit)}! {ex}: {ex.Message}");
                _logger.Exception(ex);
            }
        }

        internal void OnUpdate()
        {
            while (_updateModification.Count > 0)
            {
                var mod = _updateModification.Pop();
                if (mod.SetEnabled)
                {
                    _activeUpdateMethods.Add(mod.Value);
                }
                else
                {
                    _activeUpdateMethods.Remove(mod.Value);
                }
            }

            InUpdateCycle = true;
            foreach(var update in _activeUpdateMethods)
            {
                try
                {
                    update.Invoke();
                }
                catch(Exception ex)
                {
                    _logger.Error($"Update method on {update.Target.GetType().FullName} threw an exception! {ex}: {ex.Message}");
                    _logger.Exception(ex);
                    _logger.Warning($"Removing Update method on {update.Target.GetType().FullName}! (Update won't be called anymore!! Toggle the Feature to re-enable)");
                    _updateModification.Push(new UpdateModification<FeatureInternal.Update>(false, update));
                }
            }
            InUpdateCycle = false;
        }

        internal void OnLateUpdate()
        {
            while (_lateUpdateModification.Count > 0)
            {
                var mod = _lateUpdateModification.Pop();
                if (mod.SetEnabled)
                {
                    _activeLateUpdateMethods.Add(mod.Value);
                }
                else
                {
                    _activeLateUpdateMethods.Remove(mod.Value);
                }
            }

            InLateUpdateCycle = true;
            foreach (var lateUpdate in _activeLateUpdateMethods)
            {
                try
                {
                    lateUpdate.Invoke();
                }
                catch (Exception ex)
                {
                    _logger.Error($"LateUpdate method on {lateUpdate.Target.GetType().FullName} threw an exception! {ex}: {ex.Message}");
                    _logger.Exception(ex);
                    _logger.Warning($"Removing LateUpdate method on {lateUpdate.Target.GetType().FullName}! (LateUpdate won't be called anymore!! Toggle the Feature to re-enable)");
                    _lateUpdateModification.Push(new UpdateModification<FeatureInternal.LateUpdate>(false, lateUpdate));
                }
            }
            InLateUpdateCycle = false;
        }

        internal void OnFeatureSettingChanged(FeatureSetting setting)
        {
            setting.Helper.Feature.FeatureInternal.FeatureSettingChanged(setting);
        }

        private void OnGameStateChanged(int state)
        {
            Feature.PreviousGameState = Feature.CurrentGameState;
            Feature.CurrentGameState = state;
            foreach(var feature in RegisteredFeatures)
            {
                if (!feature.Enabled) continue;

                feature.FeatureInternal.GameStateChanged(state);
            }
        }

        internal void OnLGAreaCullUpdate(object lg_area, bool active)
        {
            foreach (var feature in RegisteredFeatures)
            {
                if (!feature.Enabled) continue;

                feature.FeatureInternal.OnLGAreaCullUpdate(lg_area, active);
            }
        }

        internal void OnApplicationFocusChanged(bool focus)
        {
            Feature.IsApplicationFocused = focus;

            foreach (var feature in RegisteredFeatures)
            {
                if (!feature.Enabled) continue;

                feature.FeatureInternal.ApplicationFocusChanged(focus);
            }
        }

        public void InitFeature(Type type, IArchiveModule module)
        {
            Feature feature = (Feature) Activator.CreateInstance(type);
            InitFeature(feature, module);
            CheckSpecialFeatures();
        }

        private void InitFeature(Feature feature, IArchiveModule module)
        {
            FeatureInternal.CreateAndAssign(feature, module);
            if (feature.Enabled)
            {
                if (feature.FeatureInternal.HasUpdateMethod)
                    _activeUpdateMethods.Add(feature.FeatureInternal.UpdateDelegate);
                if (feature.FeatureInternal.HasLateUpdateMethod)
                    _activeLateUpdateMethods.Add(feature.FeatureInternal.LateUpdateDelegate);
            }
            RegisteredFeatures.Add(feature);
            feature.Group.Features.Add(feature);
        }

        public void EnableFeature(Feature feature, bool setConfig = true)
        {
            if (feature == null) return;

            if (setConfig)
            {
                SetEnabledInConfig(feature, true);
            }

            if (feature.RequiresRestart) return;

            if (feature.Enabled) return;

            if (!feature.AppliesToThisGameBuild) return;

            _logger.Msg(ConsoleColor.Green, $"Enabling {(feature.IsAutomated ? "automated " : String.Empty)}{nameof(Feature)} {feature.Identifier} ...");

            feature.FeatureInternal.Enable();

            SetAllUpdateMethodsEnabled(feature, true);

            OnFeatureEnabled?.Invoke(feature);
        }

        public void DisableFeature(Feature feature, bool setConfig = true)
        {
            if (feature == null) return;

            if (setConfig)
            {
                SetEnabledInConfig(feature, false);
            }

            if (feature.RequiresRestart) return;

            if (!feature.Enabled) return;

            if(feature.AppliesToThisGameBuild)
                _logger.Msg(ConsoleColor.Red, $"Disabling {nameof(Feature)} {feature.Identifier} ...");

            feature.FeatureInternal.Disable();

            SetAllUpdateMethodsEnabled(feature, false);

            OnFeatureDisabled?.Invoke(feature);
        }

        public void SetAllUpdateMethodsEnabled(Feature feature, bool enable)
        {
            SetUpdateMethodEnabled(feature, enable);
            SetLateUpdateMethodEnabled(feature, enable);
        }

        private void SetUpdateMethodEnabled(Feature feature, bool enable)
        {
            if (!feature.FeatureInternal.HasUpdateMethod)
                return;

            SetUpdateMethodEnabled(feature.FeatureInternal.UpdateDelegate, enable);
        }

        private void SetUpdateMethodEnabled(FeatureInternal.Update update, bool enable)
        {
            if (update == null)
                return;

            if (InUpdateCycle)
            {
                _updateModification.Push(new UpdateModification<FeatureInternal.Update>(enable, update));
                return;
            }

            if (enable)
            {
                _activeUpdateMethods.Add(update);
                return;
            }

            _activeUpdateMethods.Remove(update);
        }

        private void SetLateUpdateMethodEnabled(Feature feature, bool enable)
        {
            if (!feature.FeatureInternal.HasLateUpdateMethod)
                return;

            SetLateUpdateMethodEnabled(feature.FeatureInternal.LateUpdateDelegate, enable);
        }

        private void SetLateUpdateMethodEnabled(FeatureInternal.LateUpdate update, bool enable)
        {
            if (update == null)
                return;

            if (InLateUpdateCycle)
            {
                _lateUpdateModification.Push(new UpdateModification<FeatureInternal.LateUpdate>(enable, update));
                return;
            }

            if (enable)
            {
                _activeLateUpdateMethods.Add(update);
                return;
            }

            _activeLateUpdateMethods.Remove(update);
        }

        public void CheckSpecialFeatures()
        {
            var requireAudioListner = RegisteredFeatures.Where(f => f.Enabled && f.RequiresUnityAudioListener);
            if (requireAudioListner.Count() > 0)
            {
                if (UnityAudioListenerHelper != null && !UnityAudioListenerHelper.Enabled)
                {
                    _logger.Notice($"Some Features require a UnityEngine AudioListener: [{string.Join("], [", requireAudioListner.Select(f => f.Identifier))}]");
                    EnableFeature(UnityAudioListenerHelper);
                }
            }
            else
            {
                if (UnityAudioListenerHelper != null && UnityAudioListenerHelper.Enabled)
                    DisableFeature(UnityAudioListenerHelper);
            }
        }

        public static void EnableAutomatedFeature(Type type)
        {
            var feature = Instance.RegisteredFeatures.FirstOrDefault(x => x.GetType() == type);

            if (feature == null) return;

            if (!feature.IsAutomated) return;

            Instance.EnableFeature(feature, false);
        }

        public static void DisableAutomatedFeature(Type type)
        {
            var feature = Instance.RegisteredFeatures.FirstOrDefault(x => x.GetType() == type);

            if (feature == null) return;

            if (!feature.IsAutomated) return;

            Instance.DisableFeature(feature, false);
        }

        internal static void RequestRestart(Feature feature)
        {
            if(Instance.FeaturesRequestingRestart.Add(feature))
                Instance.OnFeatureRestartRequestChanged?.Invoke(feature, true);
        }

        internal static void RevokeRestartRequest(Feature feature)
        {
            if(Instance.FeaturesRequestingRestart.Remove(feature))
                Instance.OnFeatureRestartRequestChanged?.Invoke(feature, false);
        }

        internal static Feature GetById(string featureIdentifier)
        {
            return Instance.RegisteredFeatures.FirstOrDefault(f => f.Identifier == featureIdentifier);
        }

        internal static Feature GetByType<T>() where T : Feature
        {
            return GetByType(typeof(T));
        }

        internal static Feature GetByType(Type type)
        {
            return Instance.RegisteredFeatures.FirstOrDefault(f => f.GetType() == type);
        }

        public static bool IsFeatureEnabled<T>() where T : Feature
        {
            var feature = GetByType<T>();
            return feature != null && feature.Enabled;
        }

        public static void SaveFeatureConfig(Feature feature)
        {
            feature.FeatureInternal.SaveFeatureSettings();
        }

        internal static void Internal_Init()
        {
            Instance = new FeatureManager();
        }

        public void DEBUG_DISABLE()
        {
            foreach(var f in RegisteredFeatures)
            {
                DisableFeature(f);
            }
        }

        public static void ToggleFeature(Feature feature)
        {
            Instance.ToggleFeatureInstance(feature);
        }

        public void ToggleFeatureInstance(Feature feature)
        {
            bool enabled = (feature.AppliesToThisGameBuild && !feature.RequiresRestart) ? feature.Enabled : IsEnabledInConfig(feature);

            if (feature.RequiresRestart)
            {
                // enabled hasn't been set so checking for equal is actually doing the (correct) opposite
                if (feature.FeatureInternal.InitialEnabledState == enabled)
                    feature.RequestRestart();
                else
                    feature.RevokeRestartRequest();
            }

            if (enabled)
            {
                DisableFeature(feature);
            }
            else
            {
                EnableFeature(feature);
            }
        }

        public void DEBUG_ENABLE()
        {
            foreach (var f in RegisteredFeatures)
            {
                EnableFeature(f);
            }
        }

        public void SaveConfig()
        {
            LocalFiles.SaveConfig(_enabledFeatures);
        }

        internal static void SetEnabledInConfig(Feature feature, bool value)
        {
            Instance.SetFeatureEnabledInConfig(feature, value);
        }

        private void SetFeatureEnabledInConfig(Feature feature, bool value)
        {
            if (feature.FeatureInternal.DoNotSaveToConfig) return;

            if (_enabledFeatures.Features.TryGetValue(feature.Identifier, out var currentValue))
            {
                if(currentValue == value)
                {
                    return;
                }
                else
                {
                    _enabledFeatures.Features.Remove(feature.Identifier);
                }
            }
            _enabledFeatures.Features.Add(feature.Identifier, value);
        }

        public static bool IsEnabledInConfig(Feature feature)
        {
            return Instance.IsFeatureEnabledInConfig(feature);
        }

        private bool IsFeatureEnabledInConfig(Feature feature)
        {
            if (feature.FeatureInternal.DoNotSaveToConfig)
            {
                if(feature.FeatureInternal.AutomatedFeature)
                    return false;
                return true;
            }

            if (_enabledFeatures.Features.TryGetValue(feature.Identifier, out var value))
            {
                return value;
            }

            var shouldEnable = feature.GetType().GetCustomAttribute<Attributes.EnableFeatureByDefault>() != null;

            SetFeatureEnabledInConfig(feature, shouldEnable);
            return shouldEnable;
        }

        internal static void AddGroupedFeature(Feature feature)
        {
            Instance.AddGroupedFeatureI(feature);
        }

        private void AddGroupedFeatureI(Feature feature)
        {
            if (feature == null || !feature.BelongsToGroup) return;

            if(!GroupedFeatures.TryGetValue(feature.Group, out var featureSet))
            {
                GroupedFeatures.Add(feature.Group, new HashSet<Feature>() {
                    feature
                });
                return;
            }

            featureSet.Add(feature);
        }

        public static void InvokeButtonPressed(Feature feature, ButtonSetting setting)
        {
            if (feature == null || setting == null) return;

            try
            {
                setting.Callback?.Invoke();
            }
            catch (Exception ex)
            {
                feature.FeatureLogger.Error($"Button {setting.ButtonID} callback threw an exception! {ex}: {ex.Message}");
                feature.FeatureLogger.Exception(ex);
            }

            feature.FeatureInternal.OnButtonPressed(setting);
        }
    }
}
